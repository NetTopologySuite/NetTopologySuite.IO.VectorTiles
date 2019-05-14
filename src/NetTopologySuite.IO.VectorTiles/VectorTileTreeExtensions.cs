using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tilers;
using NetTopologySuite.IO.VectorTiles.Tiles;

namespace NetTopologySuite.IO.VectorTiles
{
    public static class VectorTileTreeExtensions
    {
        /// <summary>
        /// Gets or creates a tile.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="tileId">The tile id.</param>
        /// <returns>The vector tile.</returns>
        internal static VectorTile TryGetOrCreate(this VectorTileTree tree, ulong tileId)
        {
            if (tree.TryGet(tileId, out var vectorTile)) return vectorTile;
            
            vectorTile = new VectorTile();
            // TODO: remove this layer, when we can configure layers.
            vectorTile.Layers.Add(new Layer()
            {
                Name = "default"
            });
            tree[tileId] = vectorTile;
            return vectorTile;
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="zoom">The zoom.</param>
        public static void Add(this VectorTileTree tree, FeatureCollection features, int zoom = 14)
        {
            tree.Add(features.Features, zoom);
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="zoom">The zoom.</param>
        public static void Add(this VectorTileTree tree, IEnumerable<IFeature> features, int zoom = 14)
        {
            foreach (var feature in features)
            {
                switch (feature.Geometry)
                {
                    case Point p:
                    {
                        // a point: easy, this is a member of just one single tile.
                        var tileId = p.Tile(zoom);
                        var vectorTile = tree.TryGetOrCreate(tileId);
                        vectorTile.Layers[0].Features.Add(new Feature(p, feature.Attributes));
                        break;
                    }
                    case LineString ls:
                    {
                        // a linestring: harder, it could be a member of any string of tiles.
                        foreach (var tileId in ls.Tiles(zoom))
                        {
                            var tile = new Tile(tileId);
                            var vectorTile = tree.TryGetOrCreate(tileId);
                            var tilePolygon = tile.ToPolygon();
                            foreach (var segment in tilePolygon.Cut(ls))
                            {
                                vectorTile.Layers[0].Features.Add(new Feature(segment, feature.Attributes));
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}