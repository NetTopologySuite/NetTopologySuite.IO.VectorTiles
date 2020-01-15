using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    // see: https://github.com/mapbox/vector-tile-spec/tree/master/2.1
    public static class MapboxTileWriter
    {
        /// <summary>
        /// Writes the tiles in a /z/x/y.mvt folder structure.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="path">The path.</param>
        /// <param name="extent">The extent.</param>
        /// <remarks>Replaces the files if they are already present.</remarks>
        public static void Write(this VectorTileTree tree, string path, uint extent = 4096)
        {
            foreach (var tileId in tree)
            {
                var tile = new Tiles.Tile(tileId);
                var zFolder = Path.Combine(path, tile.Zoom.ToString());
                if (!Directory.Exists(zFolder)) Directory.CreateDirectory(zFolder);
                var xFolder = Path.Combine(zFolder, tile.X.ToString());
                if (!Directory.Exists(xFolder)) Directory.CreateDirectory(xFolder);
                var file = Path.Combine(xFolder, $"{tile.Y.ToString()}.mvt");
                using (var stream = File.Open(file, FileMode.Create))
                {
                    tree[tileId].Write(stream, extent);
                }
            }
        }
        
        /// <summary>
        /// Writes the tile to the given stream.
        /// </summary>
        /// <param name="vectorTile">The vector tile.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="extent">The extent.</param>
        public static void Write(this VectorTile vectorTile, Stream stream, uint extent = 4096)
        {
            var tile = new Tiles.Tile(vectorTile.TileId);

            double latitudeStep = (tile.Top - tile.Bottom) / extent;
            double longitudeStep = (tile.Right - tile.Left) / extent;
            double top = tile.Top;
            double left = tile.Left;

            var mapboxTile = new Mapbox.Tile();
            foreach (var localLayer in vectorTile.Layers)
            {
                var layer = new Mapbox.Tile.Layer {Version = 2, Name = localLayer.Name, Extent = extent};

                var keys = new Dictionary<string, uint>();
                var values = new Dictionary<string, uint>();

                
                foreach (var localLayerFeature in localLayer.Features)
                {
                    if (localLayerFeature.Geometry is Point p)
                    {
                        var feature = new Mapbox.Tile.Feature();

                        var posX = (int) ((p.X - left) / longitudeStep);
                        var posY = (int) ((top - p.Y) / latitudeStep);
                        GenerateMoveTo(feature.Geometry, posX, posY);
                        feature.Type = Tile.GeomType.Point;
                        
                        AddAttributes(feature.Tags, keys, values, localLayerFeature.Attributes);

                        layer.Features.Add(feature);
                    }
                    else if (localLayerFeature.Geometry is LineString ls)
                    {
                        var feature = new Mapbox.Tile.Feature();

                        var posX = (int) ((ls.Coordinates[0].X - left) / longitudeStep);
                        var posY = (int) ((top - ls.Coordinates[0].Y) / latitudeStep);
                        GenerateMoveTo(feature.Geometry, posX, posY);

                        // generate line to.
                        feature.Geometry.Add(GenerateCommandInteger(2, ls.Coordinates.Length - 1));
                        for (var j = 1; j < ls.Coordinates.Length; j++)
                        {
                            var localPosX = (int) ((ls.Coordinates[j].X - left) / longitudeStep);
                            var localPosY = (int) ((top - ls.Coordinates[j].Y) / latitudeStep);
                            var dx = localPosX - posX;
                            var dy = localPosY - posY;
                            posX = localPosX;
                            posY = localPosY;

                            feature.Geometry.Add(GenerateParameterInteger(dx));
                            feature.Geometry.Add(GenerateParameterInteger(dy));
                        }

                        feature.Type = Tile.GeomType.LineString;

                        AddAttributes(feature.Tags, keys, values, localLayerFeature.Attributes);

                        layer.Features.Add(feature);
                    }
                }
                
                layer.Keys.AddRange(keys.Keys);
                foreach (var value in values.Keys)
                {
                    if (int.TryParse(value, out var intValue))
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            IntValue = intValue
                        });
                    }
                    else if (float.TryParse(value, out var floatValue))
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            FloatValue = floatValue
                        });
                    }
                    else
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            StringValue = value
                        });
                    }
                }
                mapboxTile.Layers.Add(layer);
            }

            ProtoBuf.Serializer.Serialize<Tile>(stream, mapboxTile);
        }

        private static void AddAttributes(List<uint> tags, Dictionary<string, uint> keys,
            Dictionary<string, uint> values, IAttributesTable attributes)
        {
            if (attributes == null) return;

            var aKeys = attributes.GetNames();
            var aValues = attributes.GetValues();

            for (var a = 0; a < aKeys.Length; a++)
            {
                var key = aKeys[a];
                var value = aValues[a];
                
                tags.Add(keys.AddOrGet(key));
                tags.Add(values.AddOrGet(value?.ToString()));
            }
        }

        /// <summary>
        /// Generates a move command. 
        /// </summary>
        private static void GenerateMoveTo(List<uint> geometry, int dx, int dy)
        {
            geometry.Add(GenerateCommandInteger(1, 1));
            geometry.Add(GenerateParameterInteger(dx));
            geometry.Add(GenerateParameterInteger(dy));
        }

        /// <summary>
        /// Generates a close path command.
        /// </summary>
        private static void GenerateClosePath(List<uint> geometry)
        {
            geometry.Add(GenerateCommandInteger(7, 1));
        }

        /// <summary>
        /// Generates a command integer.
        /// </summary>
        private static uint GenerateCommandInteger(int id, int count)
        { // CommandInteger = (id & 0x7) | (count << 3)
            return (uint) ((id & 0x7) | (count << 3));
        }

        /// <summary>
        /// Generates a parameter integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static uint GenerateParameterInteger(int value)
        { // ParameterInteger = (value << 1) ^ (value >> 31)
            return (uint) ((value<<1) ^ (value>> 31));
        }
    }
}