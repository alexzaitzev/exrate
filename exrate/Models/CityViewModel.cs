namespace Exrate.Models
{
    public class CityViewModel : DropDownModel
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int MapZoom { get; set; }
    }
}