using NetTopologySuite.IO.VectorTiles.Tiles.WebMercator;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tilers.WebMercator
{
    public class WebMercatorHandlerTests
    {
        [Fact]
        public void Test1()
        {
            var meters = WebMercatorHandler.LatLonToMeters(50, 4);
            var pixels0 = WebMercatorHandler.MetersToPixels(meters, 0, 1024);
            var pixels1 = WebMercatorHandler.MetersToPixels(meters, 1, 1024);
            var pixels2 = WebMercatorHandler.MetersToPixels(meters, 2, 1024);
            var pixels3 = WebMercatorHandler.MetersToPixels(meters, 3, 1024);
        }
    }
}