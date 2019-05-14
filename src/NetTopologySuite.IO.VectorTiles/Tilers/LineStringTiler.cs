using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tiles;

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
            var tileId = ulong.MaxValue;
            HashSet<ulong> tiles = null;
            foreach (var coordinate in lineString.Coordinates)
            {
                var nextTileId = Tile.CreateAroundLocationId(coordinate.Y, coordinate.X, zoom);
                if (nextTileId == tileId) continue;
                
                if (tiles != null)
                { // two or more tiles.
                    if (tiles.Contains(nextTileId)) continue;
                    tiles.Add(nextTileId);
                    yield return nextTileId;
                }
                else
                { // only one or two tiles.
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
            var op = new NetTopologySuite.Operation.Overlay.OverlayOp(lineString, tilePolygon);
            var intersection = op.GetResultGeometry(NetTopologySuite.Operation.Overlay.SpatialFunction.Intersection);
            if (intersection.IsEmpty)
            {
                yield break;
            }

            if (intersection is LineString ls)
            { // intersection is a linestring.
                yield return ls;
                yield break;
            }
            if (intersection is GeometryCollection gc)
            {
                foreach (var geometry in gc.Geometries)
                {
                    if (!(geometry is LineString ls0))
                    {
                        throw new Exception($"{nameof(LineStringTiler)}.{nameof(Cut)} failed: A geometry was found in the intersection that wasn't a {nameof(LineString)}.");
                    }
                    yield return ls0;
                }
                yield break;
            }
            
            throw new Exception($"{nameof(LineStringTiler)}.{nameof(Cut)} failed: Unknown result.");
        }
    }    
}