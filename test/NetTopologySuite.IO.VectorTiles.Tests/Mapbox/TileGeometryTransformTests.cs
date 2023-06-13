using System;
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
            int x = 0;
            int y = 0;
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
            int x = 0;
            int y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(-90, 85.0/2),
            }), 0, ref x, ref y);
            
            Assert.Equal(2048, x);
            Assert.Equal(3026, y);
        }

        [Fact]
        public void FloatingPointError()
        {
            const double expectedLat = 49.13987159729003;
            const double expectedLon = 52.55401993319131;

            var tileGeometry = new TileGeometryTransform(new Tile(651, 335, 10), 4096);
            int x = 0;
            int y = 0;
            tileGeometry.Transform(new CoordinateArraySequence(new Coordinate[]
            {
                new Coordinate(expectedLat, expectedLon),
            }), 0, ref x, ref y);
                
            Assert.Equal(3177, x);
            Assert.Equal(2732, y);

            (double actualLat, double actualLon) = tileGeometry.TransformInverse(x, y);

            // comparing only 5 decimal places
            Assert.Equal(expectedLat, actualLat, 5);
            Assert.Equal(expectedLon, actualLon, 5);
        }
    }
}
