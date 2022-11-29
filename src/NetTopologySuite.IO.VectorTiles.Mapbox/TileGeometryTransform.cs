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
    internal struct TileGeometryTransform
    {
        private readonly Tiles.Tile _tile;
        private readonly uint _extent;
        private readonly long _top;
        private readonly long _left;

        /// <summary>
        /// Initializes this transformation utility
        /// </summary>
        /// <param name="tile">The tile's bounds</param>
        /// <param name="extent">The tile's extent in pixel. Tiles are always square.</param>
        public TileGeometryTransform(Tiles.Tile tile, uint extent) : this()
        {
            _tile = tile;
            _extent = extent;

            // Precalculate the resolution of the tile for the specified zoom level.
            this.ZoomResolution = WebMercatorHandler.Resolution(tile.Zoom, (int)extent);

            var meters = WebMercatorHandler.LatLonToMeters(_tile.Top, _tile.Left);
            (_left, _top) = WebMercatorHandler.MetersToPixels(meters, this.ZoomResolution);
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

            double lon = sequence.GetOrdinate(index, Ordinate.X);
            double lat = sequence.GetOrdinate(index, Ordinate.Y);
            
            var meters = WebMercatorHandler.LatLonToMeters(lat, lon);
            var pixels = WebMercatorHandler.MetersToPixels(meters, this.ZoomResolution);
            
            int localX = (int) (pixels.x - _left);
            int localY = (int) (_top - pixels.y);
            int dx = localX - currentX;
            int dy = localY - currentY;
            currentX = localX;
            currentY = localY;

            return (dx, dy);
        }

        public (double longitude, double latitude) TransformInverse(int x, int y)
        {
            long globalX = _left + x;
            long globalY = _top - y;

            var meters = WebMercatorHandler.PixelsToMeters((globalX, globalY), this.ZoomResolution);
            var coordinates = WebMercatorHandler.MetersToLatLon(meters);
            return coordinates;
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
    }
}
