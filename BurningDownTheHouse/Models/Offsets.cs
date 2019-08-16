using System.Xml.Serialization;

namespace BurningDownTheHouse.Models
{
	[XmlRoot]
	public class Offsets
	{
		public string PlaceAnywhere { get; set; }
		public string WallPartition { get; set; }
		public string ActiveItem { get; set; }
		public string HighlightItem { get; set; }
		public string TabletopLoop { get; set; }
	}
}
