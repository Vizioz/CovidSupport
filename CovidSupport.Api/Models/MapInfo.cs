using ImageProcessor.Common.Extensions;

namespace CovidSupport.Api.Models
{
    public class MapInfo
    {
        public double[] LatLng { get; set; }

        public int Zoom { get; set; }
    }
}
