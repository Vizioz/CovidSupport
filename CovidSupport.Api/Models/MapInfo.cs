namespace CovidSupport.Api.Models
{
    public class MapInfo
    {
        public double[] LatLng { get; set; }

        public int Zoom { get; set; }

        public double? Lat => this.LatLng?.Length > 0 ? this.LatLng[0] : (double?)null;

        public double? Lng => this.LatLng?.Length > 1 ? this.LatLng[1] : (double?)null;
    }
}
