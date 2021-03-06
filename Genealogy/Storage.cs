using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Genealogy.Events;
using Genealogy.Succession;

namespace Genealogy
{
	public class Storage : IEventProvider
	{
		private static readonly XmlSchema xsd = XmlSchema.Read(
			Assembly.GetExecutingAssembly().GetManifestResourceStream("Genealogy.StorageFileSchema.xsd"),
			null
		);

		#region attributes
		private readonly XmlDocument document = new XmlDocument();
		private readonly XPathNavigator navigator;
		private readonly Dictionary<uint, Person> personRegister = new Dictionary<uint, Person>();
		private readonly List<PendingMarriage> pendingMarriages = new List<PendingMarriage>();

		delegate void personIDDelegate(uint id);
		private event personIDDelegate personAdded;

		public Realm[] Realms {
			get;
			private set;
		}

		public Person[] Persons {
			get;
			private set;
		}

		public Person[] RootAncestors {
			get;
			private set;
		}

		public Title[] Titles {
			get;
			private set;
		}
		#endregion

		public Storage(string filename)
		{
			try {
				document.Load(filename);

				document.Schemas.Add(xsd);
				document.Validate(null);

				navigator = document.CreateNavigator();

				loadPersons();
				loadTitles();
				loadRealms();
			} catch (StorageException) {
				throw;
			} catch (XmlSchemaValidationException e) {
				throw new StorageException("XML document is invalid: " + e.Message, e);
			} catch (Exception e) {
				throw new StorageException("Problem loading file: " + e.Message, e);
			}
		}

		public IEnumerable<Event> Events {
			get {
				return
					from p in Persons.Cast<IEventProvider>().Concat(Titles)
					from e in p.Events
					select e;
			}
		}

		private void loadRealms() {
			List<Realm> realms = new List<Realm>();

			var nodes = navigator.Select("/data/realms/realm");
			while (nodes.MoveNext()) {
				Realm realm = new Realm(
					nodes.Current.GetAttribute("name", ""),
					Titles.First(t => t.ID == uint.Parse(nodes.Current.GetAttribute("ruler", "")))
				);
				realms.Add(realm);
				loadRealms(realms, nodes.Current.Select("./realm"), realm);
			}

			this.Realms = realms.ToArray();
		}

		private void loadRealms(List<Realm> realms, XPathNodeIterator nodes, Realm container)
		{
			while (nodes.MoveNext()) {
				Realm realm = new Realm(
					nodes.Current.GetAttribute("name", ""),
					container,
					Titles.First(t => t.ID.ToString() == nodes.Current.GetAttribute("ruler", ""))
				);
				realms.Add(realm);
				loadRealms(realms, nodes.Current.Select("./realm"), realm);
			}
		}

		#region load persons
		private void loadPersons()
		{
			var ancestorNodes = navigator.Select("/data/persons/person");
			while (ancestorNodes.MoveNext()) {
				Person current = new Person(
					uint.Parse(ancestorNodes.Current.GetAttribute("id", "")),
					int.Parse(ancestorNodes.Current.GetAttribute("birth", "")),
					int.Parse(ancestorNodes.Current.GetAttribute("death", "")),
					getEnumValue<Gender>(ancestorNodes.Current.GetAttribute("gender", "")),
					ancestorNodes.Current.GetAttribute("firstname", ""),
					ancestorNodes.Current.GetAttribute("birthname", "")
				);
				addPerson(current);

				if (current.Gender == Gender.Male)
					processMarriages(current);
			}

			if (pendingMarriages.Count > 0)
				throw new StorageException("Could not create all marriages");

			Persons = personRegister.Values.ToArray();
			RootAncestors = Persons.Where(p => p.Father == null).ToArray();
		}

		private void processMarriages(Person husband)
		{
			var marriageNodes = navigator.Select("/data/persons//person[@id=" + husband.ID + "]/marriages/marriage");
			while (marriageNodes.MoveNext()) {
				uint wifeID = uint.Parse(marriageNodes.Current.GetAttribute("wife", ""));
				int year = int.Parse(marriageNodes.Current.GetAttribute("year", ""));
				XPathNodeIterator childNodes = marriageNodes.Current.Select("./children/person");
				if (personRegister.ContainsKey(wifeID))
					processMarriage(husband, year, childNodes, wifeID);
				else
					new PendingMarriage(this, husband, year, childNodes, wifeID);
			}
		}

		private void processMarriage(Person husband, int year, XPathNodeIterator childNodes, uint wifeID)
		{
			processChildren(
				husband.marryTo(
					personRegister[wifeID],
					year
				),
				childNodes
			);
		}

		private void processChildren(Marriage m, XPathNodeIterator childNodes) {
			while (childNodes.MoveNext()) {
				var current = m.addChild(
					uint.Parse(childNodes.Current.GetAttribute("id", "")),
					int.Parse(childNodes.Current.GetAttribute("birth", "")),
					int.Parse(childNodes.Current.GetAttribute("death", "")),
					getEnumValue<Gender>(childNodes.Current.GetAttribute("gender", "")),
					childNodes.Current.GetAttribute("firstname", "")
				);
				addPerson(current);

				if (current.Gender == Gender.Male)
					processMarriages(current);
			}
		}

		private void addPerson(Person p)
		{
			if (personRegister.ContainsKey(p.ID))
				throw new StorageException("Duplicate person ID '" + p.ID + "'");
			personRegister.Add(p.ID, p);
			if (personAdded != null)
				personAdded(p.ID);
		}

		private struct PendingMarriage {
			private readonly Storage store;
			private readonly Person husband;
			private readonly int year;
			private readonly XPathNodeIterator childNodes;
			private readonly uint wife;

			internal PendingMarriage(Storage s, Person husband, int year, XPathNodeIterator childNodes, uint wife) {
				this.store = s;
				this.husband = husband;
				this.year = year;
				this.childNodes = childNodes;
				this.wife = wife;

				s.personAdded += onPersonAdded;
				s.pendingMarriages.Add(this);
			}

			public void onPersonAdded(uint id)
			{
				if (id == wife) {
					execute();
					store.personAdded -= onPersonAdded;
					store.pendingMarriages.Remove(this);
				}
			}

			private void execute()
			{
				store.processMarriage(husband, year, childNodes, wife);
			}
		}
		#endregion

		#region load titles
		private void loadTitles()
		{
			List<Title> titles = new List<Title>();

			var titleNodes = navigator.Select("/data/titles/title");
			while (titleNodes.MoveNext()) {
				Title title = new Title(
					uint.Parse(titleNodes.Current.GetAttribute("id", "")),
					personRegister[uint.Parse(titleNodes.Current.GetAttribute("firstRuler", ""))],
					int.Parse(titleNodes.Current.GetAttribute("established", "")),
					getEnumValue<Rank>(titleNodes.Current.GetAttribute("rank", ""))
				);
				getSuccession(titleNodes.Current.SelectSingleNode("succession"), title);
				titles.Add(title);
			}
			this.Titles = titles.ToArray();
		}

		private void getSuccession(XPathNavigator node, Title title)
		{
			var strategyNodes = node.SelectChildren(XPathNodeType.Element);

			while (strategyNodes.MoveNext()) {
				if (strategyNodes.Current.Name.ToLower() == "appointment")
					getAppointment(strategyNodes.Current, title);
				else {
					IPreferenceFilter[] pref = getPreferenceFilters(strategyNodes.Current.Select("./preferenceFilters/*"), title);
					Lineage lin = getEnumValue<Lineage>(strategyNodes.Current.GetAttribute("lineage", ""));

					switch (strategyNodes.Current.Name.ToLower()) {
						case "primogeniture":
							new Primogeniture(title, pref, lin);
							break;
						case "blood-proximity":
							new ProximityOfBlood(title, pref, lin);
							break;
						case "seniority":
							Seniority.Sorting sorting = getEnumValue<Seniority.Sorting>(node.GetAttribute("sorting", ""));
							new Seniority(title, pref, lin, sorting);
							break;
						default :
							throw new StorageException("Unknown succession strategy '" + node.Name + "'");
					}
				}
			}
		}

		private IPreferenceFilter[] getPreferenceFilters(XPathNodeIterator nodes, Title title)
		{
			IPreferenceFilter[] filters = new IPreferenceFilter[nodes.Count];
			for (int i = 0; nodes.MoveNext(); ++i) {
				switch (nodes.Current.Name) {
					case "genderPreference":
						filters[i] = new GenderPreferenceFilter(
							getEnumValue<GenderPreferenceFilter.Kind>(nodes.Current.GetAttribute("kind", ""))
						);
						break;
					case "porphyrogeniturePreference":
						filters[i] = new PorpyhorgeniturePreferenceFilter(
							title,
							getEnumValue<PorpyhorgeniturePreferenceFilter.FilterKind>(nodes.Current.GetAttribute("filter", "")),
							getEnumValue<PorpyhorgeniturePreferenceFilter.SortingKind>(nodes.Current.GetAttribute("sort", ""))
						);
						break;
					default :
						throw new StorageException("Unknown preference filter '" + nodes.Current.Name + "'");
				}
			}
			return filters;
		}

		private Appointment getAppointment(XPathNavigator node, Title title)
		{
			var successorNodes = node.SelectChildren("successor", "");

			Person[] successors = new Person[successorNodes.Count];
			while (successorNodes.MoveNext())
				successors[successorNodes.CurrentPosition - 1] = personRegister[ uint.Parse(successorNodes.Current.GetAttribute("id-ref", "")) ];

			return new Appointment(title, successors);
		}
		#endregion

		private TEnum getEnumValue<TEnum>(string value) where TEnum : struct {
			TEnum result;
			if (!Enum.TryParse(value, true, out result))
				throw new StorageException("Invalid value '" + value + "' for " + typeof(TEnum).Name);
			return result;
		}
	}
}