using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tilers;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tilers
{
    public class PointTilerTests
    {
        [Fact]
        public void PointTiler_Tests()
        {
            ulong tileId = PointTiler.Tile(new Point(-180, 85.0511), 0);
            Assert.Equal(0UL, tileId);

            tileId = PointTiler.Tile(new Point(-180, 85.0511), 1);
            Assert.Equal(1UL, tileId);

            tileId = PointTiler.Tile(new Point(0, 0), 1);
            Assert.Equal(4UL, tileId);

            tileId = PointTiler.Tile(new Point(-90, 45), 5);
            Assert.Equal(701UL, tileId);
        }
    }
}
