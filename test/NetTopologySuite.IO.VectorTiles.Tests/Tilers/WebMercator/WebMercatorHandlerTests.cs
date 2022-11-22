using NetTopologySuite.IO.VectorTiles.Tiles.WebMercator;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tilers.WebMercator
{
    public class WebMercatorHandlerTests
    {
        [Fact]
        public void WebMercatorHandler_LatLonToMetersToPixel()
        {
            var meters = WebMercatorHandler.LatLonToMeters(50, 4);
            Assert.Equal(445277.96317309432, meters.x);
            Assert.Equal(6446275.8410171578, meters.y);

            var pixels = WebMercatorHandler.MetersToPixels(meters, 0, 1024);
            Assert.Equal(523.37777777777785, pixels.x);
            Assert.Equal(676.71575078787225, pixels.y);

            pixels = WebMercatorHandler.MetersToPixels(meters, 1, 1024);
            Assert.Equal(1046.7555555555557, pixels.x);
            Assert.Equal(1353.4315015757445, pixels.y);

            pixels = WebMercatorHandler.MetersToPixels(meters, 2, 1024);
            Assert.Equal(2093.5111111111114, pixels.x);
            Assert.Equal(2706.863003151489, pixels.y);

            pixels = WebMercatorHandler.MetersToPixels(meters, 3, 1024);
            Assert.Equal(4187.0222222222228, pixels.x);
            Assert.Equal(5413.726006302978, pixels.y);
        }

        [Fact]
        public void WebMercatorHandler_MetersToLatLon()
        {
            var latLon = WebMercatorHandler.MetersToLatLon((445277.96317309432, 6446275.8410171578)); //Lat: 50, Lon: 4

            //There is some floating point error when convering back and forth between meters and lat lon, ensure accuracy to 6 decimals.
            Assert.True(System.Math.Abs(latLon.y - 50) < 0.000001);
            Assert.True(System.Math.Abs(latLon.x - 4) < 0.000001);
        }
    }
}
