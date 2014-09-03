using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Genealogy.Chronicle;
using Genealogy.Succession;

namespace Genealogy
{
	public class Storage : IEventProvider
	{
		#region attributes
		private readonly XmlDocument document = new XmlDocument();
		private readonly XPathNavigator navigator;
		private readonly Dictionary<int, Person> personRegister = new Dictionary<int, Person>();
		private readonly List<PendingMarriage> pendingMarriages = new List<PendingMarriage>();

		delegate void personIDDelegate(int id);
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
			document.Load(filename);
			navigator = document.CreateNavigator();

			loadPersons();
			loadTitles();
			loadRealms();
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
					Titles.First(t => t.ID.ToString() == nodes.Current.GetAttribute("ruler", ""))
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
			var ancestorNodes = navigator.Select ("/data/persons/person");
			while (ancestorNodes.MoveNext()) {
				Person current = new Person (
					int.Parse(ancestorNodes.Current.GetAttribute("id", "")),
					int.Parse(ancestorNodes.Current.GetAttribute ("birth", "")),
					int.Parse(ancestorNodes.Current.GetAttribute ("death", "")),
					getGender(ancestorNodes.Current.GetAttribute ("gender", "")),
					ancestorNodes.Current.GetAttribute("firstname", ""),
					ancestorNodes.Current.GetAttribute("birthname", "")
				);
				addPerson(current);

				if (current.Gender == Gender.Male)
					processMarriages (current);
			}

			if (pendingMarriages.Count > 0)
				throw new Exception();

			Persons = personRegister.Values.ToArray();
			RootAncestors = Persons.Where (p => p.Father == null).ToArray ();
		}

		private void processMarriages(Person husband)
		{
			var marriageNodes = navigator.Select ("/data/persons//person[@id=" + husband.ID + "]/marriages/marriage");
			while (marriageNodes.MoveNext()) {
				int wifeID = int.Parse(marriageNodes.Current.GetAttribute ("wife", ""));
				if (personRegister.ContainsKey(wifeID))
					processMarriage(husband, marriageNodes.Current, wifeID);
				else
					new PendingMarriage(this, husband, marriageNodes.Current, wifeID);
			}
		}

		private void processMarriage(Person husband, XPathNavigator marriageNode, int wifeID)
		{
			processChildren(
				husband.marryTo(
					personRegister[wifeID],
					int.Parse(marriageNode.GetAttribute ("year", ""))
				),
				marriageNode
			);
		}

		private void processChildren(Marriage m, XPathNavigator node) {
			var childNodes = node.Select ("./children/person");
			while (childNodes.MoveNext()) {
				var current = m.addChild (
					int.Parse(childNodes.Current.GetAttribute ("id", "")),
					int.Parse(childNodes.Current.GetAttribute ("birth", "")),
					int.Parse(childNodes.Current.GetAttribute ("death", "")),
					getGender(childNodes.Current.GetAttribute ("gender", "")),
					childNodes.Current.GetAttribute("firstname", "")
				);
				addPerson(current);

				if (current.Gender == Gender.Male)
					processMarriages(current);
			}
		}

		private void addPerson(Person p)
		{
			personRegister.Add(p.ID, p);
			if (personAdded != null)
				personAdded(p.ID);
		}

		private Gender getGender(string value)
		{
			Gender result;
			if (!Enum.TryParse(value, true, out result))
				throw new Exception();
			return result;
		}

		private struct PendingMarriage {
			private readonly Storage store;
			private readonly Person husband;
			private readonly XPathNavigator node;
			private readonly int wife;

			internal PendingMarriage(Storage s, Person husband, XPathNavigator node, int wife) {
				this.store = s;
				this.husband = husband;
				this.node = node;
				this.wife = wife;

				s.personAdded += onPersonAdded;
				s.pendingMarriages.Add(this);
			}

			public void onPersonAdded(int id)
			{
				if (id == wife) {
					execute();
					store.personAdded -= onPersonAdded;
					store.pendingMarriages.Remove(this);
				}
			}

			private void execute()
			{
				store.processMarriage(husband, node, wife);
			}
		}
		#endregion

		#region load titles
		private void loadTitles()
		{
			List<Title> titles = new List<Title>();

			var titleNodes = navigator.Select("/data/titles/title");
			while (titleNodes.MoveNext()) {
				titles.Add(
					new Title(
						int.Parse(titleNodes.Current.GetAttribute("id", "")),
						personRegister[ int.Parse(titleNodes.Current.GetAttribute("firstRuler", "")) ],
						int.Parse(titleNodes.Current.GetAttribute("established", "")),
						getSuccession(titleNodes.Current.SelectSingleNode("succession")),
						getRank(titleNodes.Current.GetAttribute("rank", ""))
					)
				);
			}
			this.Titles = titles.ToArray();
		}

		private SuccessionStrategy getSuccession(XPathNavigator node)
		{
			var strategyNodes = node.SelectChildren("strategy", "");
			if (strategyNodes.Count == 1) {
				strategyNodes.MoveNext();
				return getStrategyImpl(strategyNodes.Current);
			}

			SuccessionStrategy[] strategies = new SuccessionStrategy[strategyNodes.Count];
			while (strategyNodes.MoveNext())
				strategies[strategyNodes.CurrentPosition] = getStrategyImpl(strategyNodes.Current);

			return new FallbackSuccessionStrategy (strategies);
		}

		private SuccessionStrategy getStrategyImpl(XPathNavigator node)
		{
			GenderPreference pref = getGenderPreferenceImpl(node.GetAttribute("preference", ""));
			Lineality lin = getLinealityImpl(node.GetAttribute("lineality", ""));

			switch (node.GetAttribute("name", "").ToLower()) {
				case "primogeniture":
					return new Primogeniture(pref, lin);
				default :
					throw new Exception();
			}
		}

		private GenderPreference getGenderPreferenceImpl(string name)
		{
			switch (name.ToLower()) {
				case "maleonly":
					return MaleOnlyPreference.Instance;
				case "male":
					return MalePreference.Instance;
				case "absolute":
					return AbsolutePreference.Instance;
				case "female":
					return FemalePreference.Instance;
				case "femaleOnly":
					return FemaleOnlyPreference.Instance;
				default:
					throw new Exception();
			}
		}

		private Lineality getLinealityImpl(string name)
		{
			switch (name.ToLower()) {
				case "agnatic":
					return AgnaticLineality.Instance;
				case "cognatic":
					return CognaticLineality.Instance;
				case "uterine":
					return UterineLineality.Instance;
				default:
					throw new Exception();
			}
		}

		private Rank getRank(string name) {
			Rank result;
			if (!Enum.TryParse(name, out result))
				throw new Exception();
			return result;
		}
		#endregion
	}
}

