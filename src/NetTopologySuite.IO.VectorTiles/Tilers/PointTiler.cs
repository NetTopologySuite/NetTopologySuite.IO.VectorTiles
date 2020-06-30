using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Tilers
{
    internal static class PointTiler
    {
        /// <summary>
        /// Returns the tile this point is part of.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="zoom">The zoom.</param>
        /// <returns>The tile.</returns>
        public static ulong Tile(this Point point, int zoom)
        {
            return Tiles.Tile.CreateAroundLocationId(point.Y, point.X, zoom);
        }
    }
}
