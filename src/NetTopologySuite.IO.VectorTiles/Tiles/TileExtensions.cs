using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Tiles
{
    internal static class TileExtensions
    {
        /// <summary>
        /// Gets the polygon representing the given tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <param name="margin">The margin (in %).</param>
        /// <returns>The polygon.</returns>
        public static Polygon ToPolygon(this Tile tile, int margin = 5)
        {
            var xMar = System.Math.Abs(tile.Right - tile.Left);
            var yMar = System.Math.Abs(tile.Top - tile.Bottom);
            return new Polygon(new LinearRing(new GeoAPI.Geometries.Coordinate[] {
                new GeoAPI.Geometries.Coordinate(tile.Left - xMar, tile.Top + yMar),
                new GeoAPI.Geometries.Coordinate(tile.Right + xMar, tile.Top + yMar),
                new GeoAPI.Geometries.Coordinate(tile.Right + xMar, tile.Bottom - yMar),
                new GeoAPI.Geometries.Coordinate(tile.Left - xMar, tile.Bottom - yMar),
                new GeoAPI.Geometries.Coordinate(tile.Left - xMar, tile.Top + yMar)
            }));
        }
    }
}