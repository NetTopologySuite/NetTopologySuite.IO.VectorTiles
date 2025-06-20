using NetTopologySuite.Geometries;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetTopologySuite.IO.VectorTiles.Tests")]
namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    /// <summary>
    /// A transformation utility from WGS84 coordinates to a local tile coordinate system in pixel
    /// </summary>
    public interface ITileGeometryTransform
    {
        /// <summary>
        /// The zoom level pixel resolution based on the extent.
        /// </summary>
        double ZoomResolution { get; }

        /// <summary>
        /// Transforms the coordinate at <paramref name="index"/> of <paramref name="sequence"/> to the tile coordinate system.
        /// The return value is the position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).
        /// </summary>
        /// <param name="sequence">The input sequence</param>
        /// <param name="index">The index of the coordinate to transform</param>
        /// <param name="currentX">The current horizontal component of the cursor location. This value is updated.</param>
        /// <param name="currentY">The current vertical component of the cursor location. This value is updated.</param>
        /// <returns>The position relative to the local point at (<paramref name="currentX"/>, <paramref name="currentY"/>).</returns>
        (int x, int y) Transform(CoordinateSequence sequence, int index, ref int currentX, ref int currentY);

        /// <summary>
        /// Transforms the point in the local tile pixel coordinates into WGS84 coordinates.
        /// The return value is longitude and latitude of the tile pixel point (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The horizontal component of the point in the tile coordinate system</param>
        /// <param name="y">The vertical component of the point in the tile coordinate system</param>
        /// <returns>WGS84 coordinates of the point in tile "pixel" coordinates (<paramref name="x"/>, <paramref name="y"/>).</returns>
        (double longitude, double latitude) TransformInverse(int x, int y);

        /// <summary>
        /// Check if the point with tile coordinates (<paramref name="x"/>, <paramref name="y"/> lies inside tile extent
        /// </summary>
        /// <param name="x">Horizontal component of the point in the tile coordinate system</param>
        /// <param name="y">Vertical component of the point in the tile coordinate system</param>
        /// <returns>true if point lies inside tile extent</returns>
        bool IsPointInExtent(int x, int y);

        /// <summary>
        /// Checks to see if a geometries envelope is greater than 1 square pixel in size for a specified zoom level.
        /// </summary>
        /// <param name="polygon">Polygon to test.</param>
        /// <returns>true if the <paramref name="polygon"/> is greater than 1 pixel in the tile pixel coordinates</returns>
        bool IsGreaterThanOnePixelOfTile(Geometry geometry);

        (long x, long y) ExtentInPixel(Envelope env);
    }
}
