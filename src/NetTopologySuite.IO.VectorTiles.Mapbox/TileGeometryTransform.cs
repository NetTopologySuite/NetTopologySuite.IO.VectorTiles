using GeoAPI.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    /// <summary>
    /// A transformation utility from WGS84 coordinates to a local tile coordinate system in pixel
    /// </summary>
    internal struct TileGeometryTransform
    {
        /// <summary>
        /// Initializes this transformation utility
        /// </summary>
        /// <param name="tile">The tile's bounds</param>
        /// <param name="extent">The tile's extent in pixel. Tiles are always square.</param>
        public TileGeometryTransform(Tiles.Tile tile, uint extent) : this()
        {
            Top = tile.Top;
            Left = tile.Left;
            LatitudeStep = (tile.Top - tile.Bottom) / extent;
            LongitudeStep = (tile.Right - tile.Left) / extent;
        }

        /// <summary>
        /// Gets a value indicating the latitude of the top-left corner of the tile
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Gets a value indicating the longitude of the top-left corner of the tile
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// Gets a value indicating the height of tile's pixel 
        /// </summary>
        public double LatitudeStep { get; }

        /// <summary>
        /// Gets a value indicating the width of tile's pixel 
        /// </summary>
        public double LongitudeStep { get; }

        /// <summary>
        /// Transforms the coordinate at <paramref name="index"/> of <paramref name="sequence"/> to the tile coordinate system.
        /// The return value is the position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).
        /// </summary>
        /// <param name="sequence">The input sequence</param>
        /// <param name="index">The index of the coordinate to transform</param>
        /// <param name="currentX">The current horizontal component of the cursor location. This value is updated.</param>
        /// <param name="currentY">The current vertical component of the cursor location. This value is updated.</param>
        /// <returns>The position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).</returns>
        public (int x, int y) Transform(ICoordinateSequence sequence, int index, ref int currentX, ref int currentY)
        {
            int localX = (int) ((sequence.GetOrdinate(index, Ordinate.X) - Left) / LongitudeStep);
            int localY = (int) ((Top - sequence.GetOrdinate(index, Ordinate.Y)) / LatitudeStep);
            int dx = localX - currentX;
            int dy = localY - currentY;
            currentX = localX;
            currentY = localY;

            return (dx, dy);
        }
    }
}
