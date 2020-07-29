using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using Xunit;
using Tile = NetTopologySuite.IO.VectorTiles.Tiles.Tile;

namespace NetTopologySuite.IO.VectorTiles.Tests.Mapbox
{
    public class TileGeometryTransformTests
    {
        [Fact]
        public void TileGeometryTransform_Transform_WebMercatorRegression1()
        {
            var tileGeometry = new TileGeometryTransform(new Tile(0, 0, 0), 4096);
            var x = 0;
            var y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(0, 0),
            }), 0, ref x, ref y);
            
            Assert.Equal(2048, x);
            Assert.Equal(2048, y);
        }
        
        [Fact]
        public void TileGeometryTransform_Transform_WebMercatorRegression2()
        {
            var tileGeometry = new TileGeometryTransform(new Tile(0, 0, 1), 4096);
            var x = 0;
            var y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(-90, 85.0/2),
            }), 0, ref x, ref y);
            
            Assert.Equal(2048, x);
            Assert.Equal(3025, y);
        }
    }
}