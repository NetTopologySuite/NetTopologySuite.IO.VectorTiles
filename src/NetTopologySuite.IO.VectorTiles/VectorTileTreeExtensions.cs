using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tilers;
using NetTopologySuite.IO.VectorTiles.Tiles;

namespace NetTopologySuite.IO.VectorTiles
{
    /// <summary>
    /// Contains extension methods related to the vector tile tree.
    /// </summary>
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

            vectorTile = new VectorTile
            {
                TileId = tileId
            };
            tree[tileId] = vectorTile;
            return vectorTile;
        }

        /// <summary>
        /// A function to configure how a feature is translated into the different vector tile layers.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public delegate IEnumerable<(IFeature feature, int zoom, string layerName)> ToFeatureZoomAndLayerFunc(
            IFeature feature);

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="toFeatureZoomAndLayer">The feature, zoom and layer function.</param>
        public static void Add(this VectorTileTree tree, FeatureCollection features, ToFeatureZoomAndLayerFunc toFeatureZoomAndLayer)
        {
            tree.Add(features.ToFeaturesZoomAndLayer(toFeatureZoomAndLayer));
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="toFeatureZoomAndLayer">The feature, zoom and layer function.</param>
        public static void Add(this VectorTileTree tree, IEnumerable<IFeature> features, ToFeatureZoomAndLayerFunc toFeatureZoomAndLayer)
        {
            tree.Add(features.ToFeaturesZoomAndLayer(toFeatureZoomAndLayer));
        }

        internal static IEnumerable<(IFeature feature, int zoom, string layerName)> ToFeaturesZoomAndLayer(
            this IEnumerable<IFeature> features, ToFeatureZoomAndLayerFunc toFeatureZoomAndLayer)
        {
            foreach (var feature in features)
            {
                var featuresZoomAndLayers = toFeatureZoomAndLayer(feature);
                foreach (var (layerFeature, zoom, layerName) in featuresZoomAndLayers)
                {
                    yield return (layerFeature, zoom, layerName);
                }
            }
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="zoom">The zoom.</param>
        /// <param name="layerName">The layer name.</param>
        public static void Add(this VectorTileTree tree, FeatureCollection features, int zoom = 14, string layerName = "default")
        {
            tree.Add(features.Select<IFeature, (IFeature feature, int zoom, string layer)>(x => (x, zoom, layer: layerName)));
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="features">The features to add.</param>
        /// <param name="zoom">The zoom.</param>
        /// <param name="layerName">The layer name.</param>
        public static void Add(this VectorTileTree tree, IEnumerable<IFeature> features, int zoom = 14, string layerName = "default")
        {
            tree.Add(features.Select<IFeature, (IFeature feature, int zoom, string layer)>(x => (x, zoom, layer: layerName)));
        }

        /// <summary>
        /// Adds the given features to the vector tile tree, expanding it if needed.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="featuresZoomAndLayer">The features to add and their zoom and layer.</param>
        public static void Add(this VectorTileTree tree, IEnumerable<(IFeature feature, int zoom, string layerName)> featuresZoomAndLayer)
        {
            foreach (var (feature, zoom, layerName) in featuresZoomAndLayer)
            {
                tree.Add(feature.Geometry, feature.Attributes, zoom, layerName);
            }
        }

        private static void Add(this VectorTileTree tree, Geometry geometry, IAttributesTable attributes, int zoom,
            string layerName)
        {
            switch (geometry)
            {
                case Point p:
                {
                    // a point: easy, this is a member of just one single tile.
                    tree.TryGetOrCreate(p.Tile(zoom)).GetOrCreate(layerName).Features.Add(new Feature(p, attributes));
                    break;
                }
                case LineString ls:
                {
                    // a linestring: harder, it could be a member of any string of tiles.
                    foreach (var tileId in ls.Tiles(zoom))
                    {
                        var tile = new Tile(tileId);
                        var layer = tree.TryGetOrCreate(tileId).GetOrCreate(layerName);
                        var tilePolygon = tile.ToPolygon();
                        foreach (var segment in tilePolygon.Cut(ls))
                        {
                            layer.Features.Add(new Feature(segment, attributes));
                        }
                    }

                    break;
                }
                case Polygon pg:
                {
                    foreach ((ulong id, IPolygonal pgPart) in PolygonTiler.Tiles(pg, zoom))
                    {
                        var layer = tree.TryGetOrCreate(id).GetOrCreate(layerName);
                        var geom = (Geometry) pgPart;
                        for (int i = 0; i < geom.NumGeometries; i++)
                            layer.Features.Add(new Feature(geom.GetGeometryN(i), attributes));
                    }

                    break;
                }
                case GeometryCollection geometryCollection:
                {
                    foreach (var subGeometry in geometryCollection.Geometries)
                    {
                        tree.Add(subGeometry, attributes, zoom, layerName);
                    }

                    break;
                }
            }
        }
    }
}
