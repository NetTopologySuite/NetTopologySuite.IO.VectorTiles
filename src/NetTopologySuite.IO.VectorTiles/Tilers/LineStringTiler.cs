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
        /// <remarks>It's possible this returns too many tiles, it's up to the 'Cut' method to exactly decide what tiles a linestring belongs in.</remarks>
        public static IEnumerable<ulong> Tiles(this LineString lineString, int zoom)
        {
            // always return the tile of the first coordinate.
            var previousTileId = Tile.CreateAroundLocationId(lineString.Coordinates[0].Y, lineString.Coordinates[0].X, zoom);
            yield return previousTileId;
            
            // return all the next tiles.
            HashSet<ulong> tiles = null;
            for (var c = 1; c < lineString.Coordinates.Length; c++)
            {
                var coordinate = lineString.Coordinates[c];
                var tileId = Tile.CreateAroundLocationId(coordinate.Y, coordinate.X, zoom);
                
                // only return changed ids.
                if (tileId == previousTileId) continue;
                
                // always two tiles or more, create hashset.
                // make sure to return only unique tiles.
                if (tiles == null) tiles = new HashSet<ulong> {previousTileId};

                // if the tiles are not neighbours then also return everything in between.
                if (!Tile.IsDirectNeighbour(tileId, previousTileId))
                { 
                    // determine all tiles between the two.
                    var previousCoordinate = lineString.Coordinates[c - 1];
                    var previousTile = new Tile(previousTileId);
                    var previousTileCoordinates =
                        previousTile.SubCoordinates(previousCoordinate.Y, previousCoordinate.X);
                    var nextTile = new Tile(tileId);
                    var nextTileCoordinates = nextTile.SubCoordinates(coordinate.Y, coordinate.X);

                    foreach (var (x, y) in Shared.LineBetween(previousTileCoordinates.x, previousTileCoordinates.y,
                        nextTileCoordinates.x, nextTileCoordinates.y))
                    {
                        var betweenTileId = Tile.CalculateTileId(zoom, x, y);
                        if (tiles.Contains(betweenTileId)) continue;
                        tiles.Add(betweenTileId);
                        yield return betweenTileId;
                    }
                }
                else
                {
                    if (tiles.Contains(tileId)) continue;
                    tiles.Add(tileId);
                    yield return tileId;
                }
                
                previousTileId = tileId; 
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
