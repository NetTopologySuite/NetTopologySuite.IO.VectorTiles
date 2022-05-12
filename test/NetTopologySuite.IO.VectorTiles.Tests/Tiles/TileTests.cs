using NetTopologySuite.IO.VectorTiles.Tiles;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tiles
{
    public class TileTests
    {
        [Fact]
        public void Tile_FromLatLon()
        {
            var tile = new Tile(0, 0, 0);

            var tile2 = Tile.CreateAroundLocation(tile.CenterLat, tile.CenterLon, 2);

            Assert.NotNull(tile2);

            if (tile2 != null)
            {
                Assert.Equal(2, tile2.X);
                Assert.Equal(2, tile2.Y);
            }
        }

        [Fact]
        public void Tile_ToPolygon()
        {
            var tile = new Tile(0, 0, 0);

            //0 margin, and defaults to 4326 coordinates.
            var polygon = tile.ToPolygon(0);

            Assert.Equal("POLYGON ((-180 85.0511287798066, 180 85.0511287798066, 180 -85.0511287798066, -180 -85.0511287798066, -180 85.0511287798066))", polygon.AsText());
        }


        [Fact]
        public void Tile_Buffer_ToPolygon()
        {
            var tile = new Tile(256, 4302, 12);

            //5% margin, and defaults to 4326 coordinates.
            var polygon = tile.ToPolygon(5);

            Assert.Equal("POLYGON ((-157.50439453125 -86.390651399889464, -157.40771484375 -86.390651399889464, -157.40771484375 -86.396732589459972, -157.50439453125 -86.396732589459972, -157.50439453125 -86.390651399889464))", polygon.AsText());
        }
    }
}
