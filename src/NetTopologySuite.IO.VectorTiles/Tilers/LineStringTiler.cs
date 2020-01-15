using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tiles;
using NetTopologySuite.Operation.Overlay;

[assembly: InternalsVisibleTo("NetTopologySuite.IO.VectorTiles.Tests")]
namespace NetTopologySuite.IO.VectorTiles.Tilers
{
    internal static class LineStringTiler
    {
        /// <summary>
        /// Returns all the tiles this linestring is part of.
        /// </summary>
        /// <param name="lineString">The linestring.</param>
        /// <param name="zoom">The zoom.</param>
        /// <returns>An enumerable of all tiles.</returns>
        public static IEnumerable<ulong> Tiles(this LineString lineString, int zoom)
        {
            // TODO: when a single linesegment crosses over multiple tiles.
            
            var tileId = ulong.MaxValue;
            HashSet<ulong>? tiles = null;
            foreach (var coordinate in lineString.Coordinates)
            {
                var nextTileId = Tile.CreateAroundLocationId(coordinate.Y, coordinate.X, zoom);
                if (nextTileId == tileId) continue;

                if (tiles != null)
                {
                    // two or more tiles.
                    if (tiles.Contains(nextTileId)) continue;
                    tiles.Add(nextTileId);
                    yield return nextTileId;
                }
                else
                {
                    // only one or two tiles.
                    yield return nextTileId;
                    if (tileId == ulong.MaxValue)
                    {
                        tileId = nextTileId;
                    }
                    else
                    {
                        tiles = new HashSet<ulong> {tileId, nextTileId};
                    }
                }
            }
        }

        /// <summary>
        /// Cuts the given linestring in one of more segments.
        /// </summary>
        /// <param name="tilePolygon">The tile polygon.</param>
        /// <param name="lineString">The linestring.</param>
        /// <returns>One or more segments.</returns>
        public static IEnumerable<LineString> Cut(this Polygon tilePolygon, LineString lineString)
        {
            var op = new OverlayOp(lineString, tilePolygon);
            var intersection = op.GetResultGeometry(SpatialFunction.Intersection);
            if (intersection.IsEmpty)
            {
                yield break;
            }

            switch (intersection)
            {
                case LineString ls:
                    // intersection is a linestring.
                    yield return ls;
                    yield break;
                
                case GeometryCollection gc:
                {
                    foreach (var geometry in gc.Geometries)
                    {
                        switch (geometry)
                        {
                            case LineString ls0:
                                yield return ls0;
                                break;
                            case Point _:
                                // The linestring only has a single point in this tile
                                // We skip it
                                // TODO check if this is correct
                                continue;
                            default:
                                throw new Exception(
                                    $"{nameof(LineStringTiler)}.{nameof(Cut)} failed: A geometry was found in the intersection that wasn't a {nameof(LineString)}.");
                        }
                    }

                    yield break;
                }

                default:
                    throw new Exception($"{nameof(LineStringTiler)}.{nameof(Cut)} failed: Unknown result.");
            }
        }
    }
}