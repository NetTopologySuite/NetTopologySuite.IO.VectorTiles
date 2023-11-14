using NetTopologySuite.IO.VectorTiles.Tiles;
using Xunit;
using System;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tiles
{
    public class TileRangeTests
    {
        [Fact]
        public void TileRange_Zoom16Count()
        {
            int zoom = 16;

            //Get the number of tiles in a row/col at zoom level 16.
            int size = (int)Math.Pow(2, zoom);

            //Create tile range.
            var tileRange = new TileRange(0, 0, size - 1, size - 1, zoom);           

            //Ensure that the count is greater than zero. The Count property used to be an int32 which is too small when zoom >= 16.
            Assert.True(tileRange.Count > 0);

            //The count should be equal to the difference in tile ID's between the current zoom level and the next one.
            Assert.Equal(tileRange.Count, (long)Tile.CalculateTileId(zoom + 1) - (long)Tile.CalculateTileId(zoom));
        }
    }
}
