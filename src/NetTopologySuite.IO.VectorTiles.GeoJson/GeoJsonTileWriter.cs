using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.VectorTiles.GeoJson
{
    public static class GeoJsonTileWriter
    {
        private static JsonSerializer _serializer = GeoJsonSerializer.Create();
        
        /// <summary>
        /// Writes the tiles in a /z/x/y-{layer}.geojson folder structure.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="path">The path.</param>
        /// <remarks>Replaces the files if they are already present.</remarks>
        public static void Write(this VectorTileTree tree, string path)
        {
            foreach (var tileId in tree)
            {
                var tileData = tree[tileId];
                foreach (var layer in tileData.Layers)
                {
                    var tile = new Tiles.Tile(tileId);
                    var zFolder = Path.Combine(path, tile.Zoom.ToString());
                    if (!Directory.Exists(zFolder)) Directory.CreateDirectory(zFolder);
                    var xFolder = Path.Combine(zFolder, tile.X.ToString());
                    if (!Directory.Exists(xFolder)) Directory.CreateDirectory(xFolder);
                    var file = Path.Combine(xFolder, $"{tile.Y.ToString()}-{layer.Name}.geojson");
                    using var stream = File.Open(file, FileMode.Create);
                    layer.Write(stream);
                }
            }
        }
        
        /// <summary>
        /// Writes a layer of the tile to the given stream.
        /// </summary>
        /// <param name="tile">The vector tile.</param>
        /// <param name="stream">The stream to write to.</param>
        public static void Write(this VectorTile tile, Stream stream)
        {
            foreach (var layer in tile.Layers)
            {
                layer?.Write(stream);
            }
        }
        
        /// <summary>
        /// Writes a layer of the tile to the given stream.
        /// </summary>
        /// <param name="layer">The layer data.</param>
        /// <param name="stream">The stream to write to.</param>
        public static void Write(this Layer layer, Stream stream)
        {
            var featureCollection = new FeatureCollection();
            foreach (var feature in layer.Features)
            {
                featureCollection.Add(feature);
            }

            if (featureCollection.Count == 0) return;

            var streamWriter = new StreamWriter(stream, Encoding.Default, 1024, true);
            _serializer.Serialize(streamWriter, featureCollection);
        }
    }
}