using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Genealogy.Chronicle;

namespace Genealogy.Inspector
{
	public class EventListBox : ListBox
	{
		public EventListBox()
			: base()
		{
			ItemHeight = 50;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			Event eventItem = Items[e.Index] as Event;
			if (eventItem == null)
				base.OnDrawItem(e);
			else {
				if (e.State.HasFlag(DrawItemState.Selected))
					e.Graphics.FillRectangle(Brushes.Beige, e.Bounds);
				else
					e.Graphics.FillRectangle(Brushes.White, e.Bounds);

				bool drawSeparator = false;
				if (Items.Count > e.Index + 1) {
					Event nextEvent = Items[e.Index + 1] as Event;
					drawSeparator = nextEvent != null && nextEvent.Year != eventItem.Year;
				}

				StringFormat leftCenterFormat = new StringFormat();
				leftCenterFormat.Alignment = StringAlignment.Near;
				leftCenterFormat.LineAlignment = StringAlignment.Center;

				e.Graphics.DrawImage(
					getImage(eventItem),
					new RectangleF(e.Bounds.Left + 5, e.Bounds.Top + 5, 40, e.Bounds.Height - 10));

				e.Graphics.DrawString(
					eventItem.Year.ToString(),
					new Font(Font, FontStyle.Bold),
					Brushes.Black,
					new RectangleF(e.Bounds.Left + 50, e.Bounds.Top + 5, 40, e.Bounds.Height - 10),
					leftCenterFormat
				);

				e.Graphics.DrawString(
					getFirstLine(eventItem),
					Font,
					Brushes.Black,
					new RectangleF(e.Bounds.Left + 95, e.Bounds.Top + 5, e.Bounds.Width - 100, (e.Bounds.Height - 10) / 2.0f),
					leftCenterFormat
				);

				e.Graphics.DrawString(
					getSecondLine(eventItem),
					Font,
					Brushes.Black,
					new RectangleF(e.Bounds.Left + 95, e.Bounds.Top + 5 + ((e.Bounds.Height - 10) / 2.0f), e.Bounds.Width - 100, (e.Bounds.Height - 10) / 2.0f),
					leftCenterFormat
				);

				if (drawSeparator)
					e.Graphics.DrawLine(Pens.Gray, e.Bounds.Left + 10, e.Bounds.Bottom - 2, e.Bounds.Right - 10, e.Bounds.Bottom - 2);
				e.DrawFocusRectangle();
			}
		}

		private static Bitmap asterisk = new Bitmap(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("Genealogy.Inspector.resources.asterisk.png"));
		private static Bitmap succession = new Bitmap(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("Genealogy.Inspector.resources.succession.png"));
		private static Bitmap cross = new Bitmap(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("Genealogy.Inspector.resources.cross.png"));
		private static Bitmap rings = new Bitmap(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("Genealogy.Inspector.resources.rings.png"));

		private Image getImage(Event ev)
		{
			if (ev is BirthEvent)
				return asterisk;
			else if (ev is DeathEvent)
				return cross;
			else if (ev is MarriageEvent)
				return rings;
			else if (ev is EstablishingEvent)
				return Crown.GetCrown((ev as EstablishingEvent).Title.Rank);
			else if (ev is SuccessionEvent)
				return succession;
			return null;
		}

		private string getFirstLine(Event ev)
		{
			if (ev is BirthEvent)
				return string.Format("Birth of {0} {1}", (ev as BirthEvent).Person.Firstname, (ev as BirthEvent).Person.Birthname);
			else if (ev is DeathEvent) {
				Person deceased = (ev as DeathEvent).Person;
				return string.Format("Death of {0} {1}{2}",
				                     deceased.Firstname,
				                     deceased.Lastname,
				                     (deceased.Lastname != deceased.Birthname ? " née " + deceased.Birthname : "")
				);
			} else if (ev is EstablishingEvent)
				return string.Format("Established regime: {0} of {1}",
				                     (ev as EstablishingEvent).Title.Rank,
				                     Realm.JoinRealmNames((ev as EstablishingEvent).Title.Realms)
				);
			else if (ev is MarriageEvent) {
				Marriage m = (ev as MarriageEvent).Marriage;
				return string.Format("Marriage of {0} {1} to {2} {3}",
				                     m.Husband.Firstname,
				                     m.Husband.Lastname,
				                     m.Wife.Firstname,
				                     m.Wife.getLastname(m.Start)
				);
			} else if (ev is SuccessionEvent)
				return (ev as SuccessionEvent).Successor.ToString((ev as SuccessionEvent).Successor.Start);
			return string.Empty;
		}

		private string getSecondLine(Event ev)
		{
			if (ev is BirthEvent) {
				Person born = (ev as BirthEvent).Person;
				if (born.Father != null)
					return string.Format("to {0} {1} and {2} {3} (née {4})",
					                     born.Father.Firstname,
					                     born.Father.getLastname(born.YearOfBirth),
					                     born.Mother.Firstname,
					                     born.Mother.getLastname(born.YearOfBirth),
					                     born.Mother.Birthname
					);
			} else if (ev is DeathEvent)
				return string.Join(", ",
				                   (ev as DeathEvent).Person.Titles.Select(r => string.Format("{0} of {1} since {2}",
				                                                                              r.Title.Rank,
				                                                                              Realm.JoinRealmNames(r.Title.Realms),
				                                                                              r.Start
																							)
									)
				);
			else if (ev is EstablishingEvent)
				return (ev as EstablishingEvent).Title.Reigns[0].ToString(ev.Year);
			else if (ev is MarriageEvent) {
				Marriage m = (ev as MarriageEvent).Marriage;
				int degree = Person.RelationshipDegree(m.Husband, m.Wife);
				return "Relationship degree: " + (degree == -1 ? "none" : degree.ToString());
			} else if (ev is SuccessionEvent)
				return "follows " + (ev as SuccessionEvent).Predecessor.ToString(ev.Year);
			return string.Empty;
		}
	}
}