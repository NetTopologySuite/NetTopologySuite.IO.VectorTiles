using NetTopologySuite.IO.VectorTiles.Tiles;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tiles
{
    public class TileTests
    {
        [Fact]
        public void Tile_FromLatLon()
        {
            var tile = new Tile(0,0, 0);

            (double longitude, double latitude) center = (tile.CenterLon, tile.CenterLat);
            var top = tile.Top;
            var bottom = tile.Bottom;

            var tile2 = Tile.CreateAroundLocation(center.latitude, center.longitude, 2);
            
        }
    }
}