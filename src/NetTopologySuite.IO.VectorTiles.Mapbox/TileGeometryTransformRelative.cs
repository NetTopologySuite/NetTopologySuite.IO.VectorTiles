using System;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tiles.WebMercator;

[assembly: InternalsVisibleTo("NetTopologySuite.IO.VectorTiles.Tests")]
namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    /// <summary>
    /// A transformation utility from WGS84 coordinates to a local tile coordinate system in pixel
    /// </summary>
    public struct TileGeometryTransformRelative : ITileGeometryTransform
    {
        private readonly Tiles.Tile _tile;
        private readonly uint _extent;
        private readonly uint _newExtent;
        private readonly double _factor;

        /// <summary>
        /// Initializes this transformation utility
        /// </summary>
        /// <param name="tile">The tile's bounds</param>
        /// <param name="extent">The tile's extent in pixel. Tiles are always square.</param>
        /// <param name="newExtent">The tile's new extent in pixel. Tiles are always square.</param>
        public TileGeometryTransformRelative(Tiles.Tile tile, uint extent, uint newExtent) : this()
        {
            _extent = extent;
            _newExtent = newExtent;
            _factor = (double)_newExtent / (double)_extent;

            // Precalculate the resolution of the tile for the specified zoom level.
            ZoomResolution = WebMercatorHandler.Resolution(tile.Zoom, (int)extent);
        }

        /// <summary>
        /// The zoom level pixel resolution based on the extent.
        /// </summary>
        public double ZoomResolution { get; }

        /// <summary>
        /// Transforms the coordinate at <paramref name="index"/> of <paramref name="sequence"/> to the tile coordinate system.
        /// The return value is the position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).
        /// </summary>
        /// <param name="sequence">The input sequence</param>
        /// <param name="index">The index of the coordinate to transform</param>
        /// <param name="currentX">The current horizontal component of the cursor location. This value is updated.</param>
        /// <param name="currentY">The current vertical component of the cursor location. This value is updated.</param>
        /// <returns>The position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).</returns>
        public (int x, int y) Transform(CoordinateSequence sequence, int index, ref int currentX, ref int currentY)
        {
            // This should never happen.
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (sequence.Count == 0)
                throw new ArgumentException("sequence is empty.", nameof(sequence));

            double originalX = sequence.GetOrdinate(index, Ordinate.X);
            double originalY = sequence.GetOrdinate(index, Ordinate.Y);
            
            int localX = (int)(originalX / _factor);
            int localY = (int)(originalY / _factor);
            int dx = localX - currentX;
            int dy = localY - currentY;
            currentX = localX;
            currentY = localY;

            return (dx, dy);
        }

        /// <summary>
        /// Transforms the point in the local tile pixel coordinates into 0..511 coordinates.
        /// The return value is x and y of the tile pixel point (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The horizontal component of the point in the tile coordinate system</param>
        /// <param name="y">The vertical component of the point in the tile coordinate system</param>
        /// <returns>0..511 coordinates of the point in tile "pixel" coordinates (<paramref name="x"/>, <paramref name="y"/>).</returns>
        public (double longitude, double latitude) TransformInverse(int x, int y)
        {
            return (x * _factor, y * _factor);
        }

        /// <summary>
        /// Check if the point with tile coordinates (<paramref name="x"/>, <paramref name="y"/> lies inside tile extent
        /// </summary>
        /// <param name="x">Horizontal component of the point in the tile coordinate system</param>
        /// <param name="y">Vertical component of the point in the tile coordinate system</param>
        /// <returns>true if point lies inside tile extent</returns>
        public bool IsPointInExtent(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _extent && y < _extent;
        }

        /// <summary>
        /// Checks to see if a geometries envelope is greater than 1 square pixel in size for a specified zoom level.
        /// </summary>
        /// <param name="polygon">Polygon to test.</param>
        /// <returns>true if the <paramref name="polygon"/> is greater than 1 pixel in the tile pixel coordinates</returns>
        public bool IsGreaterThanOnePixelOfTile(Geometry geometry)
        {
            if (geometry.IsEmpty) return false;

            var env = geometry.EnvelopeInternal;

            double dx = Math.Abs(env.MaxX - env.MinX);
            double dy = Math.Abs(env.MaxY - env.MinY);

            // Both must be greater than 0, and at least one of them needs to be larger than 1. 
            return dx > 0 && dy > 0 && (dx > 1 || dy > 1);
        }

        public (long x, long y) ExtentInPixel(Envelope env)
        {
            return ((long)((env.MaxX - env.MinX) / _factor), (long)((env.MaxY - env.MinY) / _factor));
        }
    }
}
