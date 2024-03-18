using System;
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
            IEnumerable<VectorTile> GetTiles()
            {
                foreach (ulong tile in tree)
                {
                    yield return tree[tile];
                }
            }

            GetTiles().Write(path, extent);
        }

        /// <summary>
        /// Writes the tiles in a /z/x/y.mvt folder structure.
        /// </summary>
        /// <param name="vectorTiles">The tiles.</param>
        /// <param name="path">The path.</param>
        /// <param name="extent">The extent.</param>
        /// <remarks>Replaces the files if they are already present.</remarks>
        public static void Write(this IEnumerable<VectorTile> vectorTiles, string path, uint extent = 4096)
        {
            foreach (var vectorTile in vectorTiles)
            {
                var tile = new Tiles.Tile(vectorTile.TileId);
                string zFolder = Path.Combine(path, tile.Zoom.ToString());

                if (!Directory.Exists(zFolder))
                    Directory.CreateDirectory(zFolder);

                string xFolder = Path.Combine(zFolder, tile.X.ToString());

                if (!Directory.Exists(xFolder))
                    Directory.CreateDirectory(xFolder);

                string file = Path.Combine(xFolder, $"{tile.Y}.mvt");

                using var stream = File.Open(file, FileMode.Create);
                vectorTile.Write(stream, extent);
            }
        }

        /// <summary>
        /// Writes the tile to the given stream.
        /// </summary>
        /// <param name="vectorTile">The vector tile.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="extent">The extent.</param>
        /// <param name="idAttributeName">The name of an attribute property to use as the ID for the Feature. Vector tile feature ID's should be integer or ulong numbers.</param>
        public static void Write(this VectorTile vectorTile, Stream stream, uint extent = 4096, string idAttributeName = "id")
        {
            var tile = new Tiles.Tile(vectorTile.TileId);
            var tgt = new TileGeometryTransform(tile, extent);

            var mapboxTile = new Mapbox.Tile();
            foreach (var localLayer in vectorTile.Layers)
            {
                var layer = new Mapbox.Tile.Layer { Version = 2, Name = localLayer.Name, Extent = extent };

                var keys = new Dictionary<string, uint>();
                var values = new Dictionary<Tile.Value, uint>();

                foreach (var localLayerFeature in localLayer.Features)
                {
                    var feature = new Mapbox.Tile.Feature();

                    // Features with empty geometries cannot be encoded
                    if (localLayerFeature.Geometry.IsEmpty)
                        continue;

                    // Encode geometry
                    switch (localLayerFeature.Geometry)
                    {
                        case IPuntal puntal:
                            feature.Type = Tile.GeomType.Point;
                            EncodeTo(feature.Geometry, puntal, tgt);
                            break;
                        case ILineal lineal:
                            feature.Type = Tile.GeomType.LineString;
                            EncodeTo(feature.Geometry, lineal, tgt);
                            break;
                        case IPolygonal polygonal:
                            feature.Type = Tile.GeomType.Polygon;
                            EncodeTo(feature.Geometry, polygonal, tgt, tile.Zoom);
                            break;
                        default:
                            feature.Type = Tile.GeomType.Unknown;
                            break;
                    }

                    // If geometry collapsed during encoding, we don't add the feature at all
                    if (feature.Geometry.Count == 0)
                        continue;

                    // Translate attributes for feature
                    AddAttributes(feature.Tags, keys, values, localLayerFeature.Attributes);

                    //Try and retrieve an ID from the attributes.
                    object id = localLayerFeature.Attributes.GetOptionalValue(idAttributeName);

                    //Converting ID to string, then trying to parse. This will handle situations will ignore situations where the ID value is not actually an integer or ulong number.
                    if (id != null)
                    {
                        if (id is ulong idu)
                            feature.Id = idu;
                        else if (ulong.TryParse(id.ToString(), out ulong idVal))
                            feature.Id = idVal;
                    }

                    // Add feature to layer
                    layer.Features.Add(feature);
                }

                layer.Keys.AddRange(keys.Keys);
                layer.Values.AddRange(values.Keys);

                mapboxTile.Layers.Add(layer);
            }

            ProtoBuf.Serializer.Serialize<Tile>(stream, mapboxTile);
        }

        private static void AddAttributes(List<uint> tags, Dictionary<string, uint> keys,
            Dictionary<Tile.Value, uint> values, IAttributesTable attributes)
        {
            if (attributes == null || attributes.Count == 0)
                return;

            string[] aKeys = attributes.GetNames();
            object[] aValues = attributes.GetValues();

            for (int a = 0; a < aKeys.Length; a++)
            {
                string key = aKeys[a];
                if (string.IsNullOrEmpty(key)) continue;

                var tileValue = ToTileValue(aValues[a]);
                if (tileValue == null) continue;

                tags.Add(keys.AddOrGet(key));
                tags.Add(values.AddOrGet(tileValue));
            }
        }

        private static Tile.Value ToTileValue(object value)
        {
            switch (value)
            {
                case bool boolValue:
                    return new Tile.Value { BoolValue = boolValue };

                case sbyte sbyteValue:
                    return new Tile.Value { IntValue = sbyteValue };
                case short shortValue:
                    return new Tile.Value { IntValue = shortValue };
                case int intValue:
                    return new Tile.Value { IntValue = intValue };
                case long longValue:
                    return new Tile.Value { IntValue = longValue };

                case byte byteValue:
                    return new Tile.Value { UintValue = byteValue };
                case ushort ushortValue:
                    return new Tile.Value { UintValue = ushortValue };
                case uint uintValue:
                    return new Tile.Value { UintValue = uintValue };
                case ulong ulongValue:
                    return new Tile.Value { UintValue = ulongValue };

                case double doubleValue:
                    return new Tile.Value { DoubleValue = doubleValue };
                case float floatValue:
                    return new Tile.Value { FloatValue = floatValue };

                case string stringValue:
                    return new Tile.Value { StringValue = stringValue };
                default:
                    break;
            }

            return null;
        }

        private static void EncodeTo(List<uint> destination, IPuntal puntal, TileGeometryTransform tgt)
        {
            const int CoordinateIndex = 0;

            var geometry = (Geometry)puntal;
            int currentX = 0, currentY = 0;

            int moveToIndex = destination.Count;
            destination.Add(0); //Overwritten below by the MoveTo command

            //var parameters = new List<uint>();
            for (int i = 0; i < geometry.NumGeometries; i++)
            {
                var point = (Point)geometry.GetGeometryN(i);
                // if the point is empty, there is nothing we can do with it
                if (point.IsEmpty) continue;

                int previousX = currentX, previousY = currentY;
                (int x, int y) = tgt.Transform(point.CoordinateSequence, CoordinateIndex, ref currentX, ref currentY);

                if (i == 0 || tgt.IsPointInExtent(currentX, currentY))
                {
                    destination.Add(GenerateParameterInteger(x));
                    destination.Add(GenerateParameterInteger(y));
                }
                else
                {
                    // discard point if it lies outside tile extent and rollback to previous point
                    // only for the case of multipoint
                    currentX = previousX;
                    currentY = previousY;
                }
            }

            destination[moveToIndex] = GenerateCommandInteger(MapboxCommandType.MoveTo, (destination.Count - moveToIndex) / 2);
        }

        private static void EncodeTo(List<uint> destination, ILineal lineal, TileGeometryTransform tgt)
        {
            var geometry = (Geometry)lineal;
            int currentX = 0, currentY = 0;
            for (int i = 0; i < geometry.NumGeometries; i++)
            {
                var lineString = (LineString)geometry.GetGeometryN(i);
                EncodeTo(destination, lineString.CoordinateSequence, tgt, ref currentX, ref currentY, false);
            }
        }

        private static void EncodeTo(List<uint> destination, IPolygonal polygonal, TileGeometryTransform tgt, int zoom)
        {
            var geometry = (Geometry)polygonal;

            //Test the whole polygon geometry is larger than a single pixel.
            if (tgt.IsGreaterThanOnePixelOfTile(geometry))
            {
                int currentX = 0, currentY = 0;
                for (int i = 0; i < geometry.NumGeometries; i++)
                {
                    var polygon = (Polygon)geometry.GetGeometryN(i);

                    //Test that individual polygons are larger than a single pixel.
                    if (!tgt.IsGreaterThanOnePixelOfTile(polygon))
                        continue;

                    EncodeTo(destination, polygon.Shell.CoordinateSequence, tgt, ref currentX, ref currentY, true, false);
                    foreach (var hole in polygon.InteriorRings)
                    {
                        EncodeTo(destination, hole.CoordinateSequence, tgt, ref currentX, ref currentY, true, true);
                    }
                }
            }
        }

        private static void EncodeTo(List<uint> destination, CoordinateSequence sequence, TileGeometryTransform tgt,
            ref int currentX, ref int currentY,
            bool ring = false, bool ccw = false)
        {
            // how many parameters for LineTo command
            // skipping the last point for rings since ClosePath is used instead
            int count = ring ? sequence.Count - 1 : sequence.Count;

            // In case we decide to ditch encoded data, we must reset currentX and currentY
            int initialCurrentX = currentX;
            int initialCurrentY = currentY;

            // If the sequence is empty there is nothing we can do with it.
            if (count == 0)
                return;

            // if we have a ring we need to check orientation
            if (ring)
            {
                if (ccw != Algorithm.Orientation.IsCCW(sequence))
                {
                    sequence = sequence.Copy();
                    CoordinateSequences.Reverse(sequence);
                }
            }

            int initialSize = destination.Count;
            // Start point
            destination.Add(GenerateCommandInteger(MapboxCommandType.MoveTo, 1));
            var position = tgt.Transform(sequence, 0, ref currentX, ref currentY);
            destination.Add(GenerateParameterInteger(position.x));
            destination.Add(GenerateParameterInteger(position.y));

            // Add LineTo command (stub)
            int lineToCount = 0;
            destination.Add(GenerateCommandInteger(MapboxCommandType.LineTo, lineToCount));
            for (int i = 1; i < count; i++)
            {
                position = tgt.Transform(sequence, i, ref currentX, ref currentY);

                if (position.x != 0 || position.y != 0)
                {
                    destination.Add(GenerateParameterInteger(position.x));
                    destination.Add(GenerateParameterInteger(position.y));
                    lineToCount++;
                }
            }
            if (lineToCount > 0)
                destination[initialSize + 3] = GenerateCommandInteger(MapboxCommandType.LineTo, lineToCount);

            // Validate encoded data
            if (ring)
            {
                // A ring has 1 MoveTo and 1 LineTo command.
                // A ring is only valid if we have at least 3 points, otherwise collapse
                if (destination.Count - initialSize - 2 >= 6)
                    destination.Add(GenerateCommandInteger(MapboxCommandType.ClosePath, 1));
                else
                {
                    currentX = initialCurrentX;
                    currentY = initialCurrentY;
                    destination.RemoveRange(initialSize, destination.Count - initialSize);
                }
            }
            else
            {
                // A line has 1 MoveTo and 1 LineTo command.
                // A line is valid if it has at least 2 points
                if (destination.Count - initialSize - 2 < 4)
                {
                    currentX = initialCurrentX;
                    currentY = initialCurrentY;
                    destination.RemoveRange(initialSize, destination.Count - initialSize);
                }
            }
        }

        /*
        /// <summary>
        /// Generates a move command. 
        /// </summary>
        private static void GenerateMoveTo(List<uint> geometry, int dx, int dy)
        {
            geometry.Add(GenerateCommandInteger(MapboxCommandType.MoveTo, 1));
            geometry.Add(GenerateParameterInteger(dx));
            geometry.Add(GenerateParameterInteger(dy));
        }
         
        /// <summary>
        /// Generates a close path command.
        /// </summary>
        private static void GenerateClosePath(List<uint> geometry)
        {
            geometry.Add(GenerateCommandInteger(MapboxCommandType.ClosePath, 1));
        }
         */

        /// <summary>
        /// Generates a command integer.
        /// </summary>
        private static uint GenerateCommandInteger(MapboxCommandType command, int count)
        { // CommandInteger = (id & 0x7) | (count << 3)
            return (uint)(((int)command & 0x7) | (count << 3));
        }

        /// <summary>
        /// Generates a parameter integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static uint GenerateParameterInteger(int value)
        { // ParameterInteger = (value << 1) ^ (value >> 31)
            return (uint)((value << 1) ^ (value >> 31));
        }
    }
}
