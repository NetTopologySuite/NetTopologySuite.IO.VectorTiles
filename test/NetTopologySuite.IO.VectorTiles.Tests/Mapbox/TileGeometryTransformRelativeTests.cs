using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using Xunit;
using Tile = NetTopologySuite.IO.VectorTiles.Tiles.Tile;

namespace NetTopologySuite.IO.VectorTiles.Tests.Mapbox
{
    public class TileGeometryTransformRelativeTests
    {
        [Theory]
        [InlineData(0.0, 0.0, 0, 0)]
        [InlineData(0.0, 256.0, 0, 2048)]
        [InlineData(100.0, 100.0, 800, 800)]
        public void TileGeometryTransform_Transform_Regression1(double oldX, double oldY, int newX, int newY)
        {
            var tileGeometry = new TileGeometryTransformRelative(new Tile(0, 0, 0), 4096, 512);
            int x = 0;
            int y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(oldX, oldY),
            }), 0, ref x, ref y);
            
            Assert.Equal(newX, x);
            Assert.Equal(newY, y);
        }
        
        [Fact]
        public void TileGeometryTransform_TransformInverse_Regression2()
        {
            var tileGeometry = new TileGeometryTransformRelative(new Tile(0, 0, 1), 4096, 512);

            (double x, double y) = tileGeometry.TransformInverse(2048, 3026);
            
            Assert.Equal(256.0, x);
            Assert.Equal(378.25, y);
        }

        [Fact]
        public void FloatingPointError()
        {
            const double dx = 256.125;
            const double dy = 128.375;

            var tileGeometry = new TileGeometryTransformRelative(new Tile(651, 335, 10), 4096, 512);
            int x = 0;
            int y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(dx, dy),
            }), 0, ref x, ref y);
                
            Assert.Equal(2049, x);
            Assert.Equal(1027, y);

            (double actualX, double actualY) = tileGeometry.TransformInverse(x, y);

            // comparing only 5 decimal places
            Assert.Equal(dx, actualX, 5);
            Assert.Equal(dy, actualY, 5);
        }
    }
}
