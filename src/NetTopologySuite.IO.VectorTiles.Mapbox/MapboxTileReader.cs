using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    public class MapboxTileReader
    {


        private readonly GeometryFactory _factory;

        public MapboxTileReader() : this(new GeometryFactory(new PrecisionModel(), 4326))
        {
        }

        public MapboxTileReader(GeometryFactory factory)
        {
            _factory = factory;
        }

        public VectorTile Read(Stream stream, Tiles.Tile tileDefinition)
        {


            // Deserialize the tile
            var tile = ProtoBuf.Serializer.Deserialize<Mapbox.Tile>(stream);

            var vectorTile = new VectorTile { TileId = tileDefinition.Id };
            foreach (var mbTileLayer in tile.Layers)
            {
                Debug.Assert(mbTileLayer.Version == 2U);

                var tgs = new TileGeometryTransform(tileDefinition, mbTileLayer.Extent);
                var layer = new Layer {Name = mbTileLayer.Name};
                foreach (var mbTileFeature in mbTileLayer.Features)
                {
                    var feature = ReadFeature(tgs, mbTileLayer, mbTileFeature);
                    layer.Features.Add(feature);
                }
                vectorTile.Layers.Add(layer);
            }

            return vectorTile;
        }

        private IFeature ReadFeature(TileGeometryTransform tgs, Tile.Layer mbTileLayer, Tile.Feature mbTileFeature)
        {
            var geometry = ReadGeometry(tgs, mbTileFeature.Type, mbTileFeature.Geometry);
            var attributes = ReadAttributeTable(mbTileFeature, mbTileLayer.Keys, mbTileLayer.Values);
            return new Feature(geometry, attributes);
        }

        private Geometry ReadGeometry(TileGeometryTransform tgs, Tile.GeomType type, IList<uint> geometry)
        {
            switch (type)
            {
                case Tile.GeomType.Point:
                    return ReadPoint(tgs, geometry);

                case Tile.GeomType.LineString:
                    return ReadLineString(tgs, geometry);

                case Tile.GeomType.Polygon:
                    return ReadPolygon(tgs, geometry);
            }

            return null;
        }
        
        private Geometry ReadPoint(TileGeometryTransform tgs, IList<uint> geometry)
        {
            int currentIndex = 0; int currentX = 0; int currentY = 0;
            var sequences = ReadCoordinateSequences(tgs, geometry, ref currentIndex, ref currentX, ref currentY, forPoint:true);
            return CreatePuntal(sequences);
        }

        private Geometry ReadLineString(TileGeometryTransform tgs, IList<uint> geometry)
        {
            int currentIndex = 0; int currentX = 0; int currentY = 0;
            var sequences = ReadCoordinateSequences(tgs, geometry, ref currentIndex, ref currentX, ref currentY);
            return CreateLineal(sequences);
        }

        private Geometry ReadPolygon(TileGeometryTransform tgs, IList<uint> geometry)
        {
            int currentIndex = 0; int currentX = 0; int currentY = 0;
            var sequences = ReadCoordinateSequences(tgs, geometry, ref currentIndex, ref currentX, ref currentY, 1);
            return CreatePolygonal(sequences);
        }

        private Geometry CreatePuntal(CoordinateSequence[] sequences)
        {
            if (sequences == null || sequences.Length == 0)
                return null;

            var points = new Point[sequences.Length];
            for (int i = 0; i < sequences.Length; i++)
                points[i] = _factory.CreatePoint(sequences[i]);

            if (points.Length == 1)
                return points[0];

            return _factory.CreateMultiPoint(points);
        }

        private Geometry CreateLineal(CoordinateSequence[] sequences)
        {
            if (sequences == null || sequences.Length == 0)
                return null;

            var lineStrings = new LineString[sequences.Length];
            for (int i = 0; i < sequences.Length; i++)
                lineStrings[i] = _factory.CreateLineString(sequences[i]);

            if (lineStrings.Length == 1)
                return lineStrings[0];

            return _factory.CreateMultiLineString(lineStrings);
        }

        private Geometry CreatePolygonal(CoordinateSequence[] sequences)
        {
            List<Polygon> polygons = new List<Polygon>();

            LinearRing shell = null;
            List<LinearRing> holes = new List<LinearRing>();

            for (int i = 0; i < sequences.Length; i++)
            {
                var ring = _factory.CreateLinearRing(sequences[i]);
                if (ring.IsCCW)
                {
                    if (shell != null)
                    {
                        polygons.Add(_factory.CreatePolygon(shell, holes.ToArray()));
                        holes.Clear();
                    }
                    shell = ring;
                }
                else
                {
                    if (shell == null)
                        throw new InvalidOperationException();
                    holes.Add(ring);
                }
            }

            polygons.Add(_factory.CreatePolygon(shell, holes.ToArray()));

            if (polygons.Count == 1)
                return polygons[0];

            return _factory.CreateMultiPolygon(polygons.ToArray());
        }

        private CoordinateSequence[] ReadCoordinateSequences(
            TileGeometryTransform tgs, IList<uint> geometry,
            ref int currentIndex, ref int currentX, ref int currentY, int buffer = 0, bool forPoint = false)
        {
            (var command, int count) = ParseCommandInteger(geometry[currentIndex]);
            Debug.Assert(command == MapboxCommandType.MoveTo);
            if (count > 1)
            {
                currentIndex++;
                return ReadSinglePointSequences(tgs, geometry, count, ref currentIndex, ref currentX, ref currentY);
            }

            var sequences = new List<CoordinateSequence>();
            var currentPosition = (currentX, currentY);
            while (currentIndex < geometry.Count)
            {
                (command, count) = ParseCommandInteger(geometry[currentIndex++]);
                Debug.Assert(command == MapboxCommandType.MoveTo);
                Debug.Assert(count == 1);

                // Read the current position
                currentPosition = ParseOffset(currentPosition, geometry, ref currentIndex);

                if (!forPoint)
                {
                    // Read the next command (should be LineTo)
                    (command, count) = ParseCommandInteger(geometry[currentIndex++]);
                    if (command != MapboxCommandType.LineTo) count = 0;
                }
                else
                {
                    count = 0;
                }

                // Create sequence, add starting point
                var sequence = _factory.CoordinateSequenceFactory.Create(1 + count + buffer, 2);
                int sequenceIndex = 0;
                TransformOffsetAndAddToSequence(tgs, currentPosition, sequence, sequenceIndex++);

                // Read and add offsets
                for (int i = 1; i <= count; i++)
                {
                    currentPosition = ParseOffset(currentPosition, geometry, ref currentIndex);
                    TransformOffsetAndAddToSequence(tgs, currentPosition, sequence, sequenceIndex++);
                }

                // Check for ClosePath command
                if (currentIndex < geometry.Count)
                {
                    (command, _) = ParseCommandInteger(geometry[currentIndex]);
                    if (command == MapboxCommandType.ClosePath)
                    {
                        Debug.Assert(buffer > 0);
                        sequence.SetOrdinate(sequenceIndex, Ordinate.X, sequence.GetOrdinate(0, Ordinate.X));
                        sequence.SetOrdinate(sequenceIndex, Ordinate.Y, sequence.GetOrdinate(0, Ordinate.Y));

                        currentIndex++;
                        sequenceIndex++;
                    }
                }

                Debug.Assert(sequenceIndex == sequence.Count);

                sequences.Add(sequence);
            }

            // update current position values
            currentX = currentPosition.currentX;
            currentY = currentPosition.currentY;

            return sequences.ToArray();
        }

        private CoordinateSequence[] ReadSinglePointSequences(TileGeometryTransform tgs, IList<uint> geometry,
            int numSequences, ref int currentIndex, ref int currentX, ref int currentY)
        {
            var res = new CoordinateSequence[numSequences];
            var currentPosition = (currentX, currentY);
            for (int i = 0; i < numSequences; i++)
            {
                res[i] = _factory.CoordinateSequenceFactory.Create(1, 2);

                currentPosition = ParseOffset(currentPosition, geometry, ref currentIndex);
                TransformOffsetAndAddToSequence(tgs, currentPosition, res[i], 0);
            }

            currentX = currentPosition.currentX;
            currentY = currentPosition.currentY;
            return res;
        }

        private void TransformOffsetAndAddToSequence(TileGeometryTransform tgs, (int x, int y) localPosition, CoordinateSequence sequence, int index)
        {
            var (longitude, latitude) = tgs.TransformInverse(localPosition.x, localPosition.y);
            sequence.SetOrdinate(index, Ordinate.X, longitude);
            sequence.SetOrdinate(index, Ordinate.Y, latitude);
        }

        private (int, int) ParseOffset((int x, int y) currentPosition, IList<uint> parameterIntegers, ref int offset)
        {
            return (currentPosition.x + Decode(parameterIntegers[offset++]),
                    currentPosition.y + Decode(parameterIntegers[offset++]));
        }

        private static int Decode(uint parameterInteger)
        {
            return ((int) (parameterInteger >> 1) ^ ((int)-(parameterInteger & 1)));
        }

        private static (MapboxCommandType, int) ParseCommandInteger(uint commandInteger)
        {
            return unchecked(((MapboxCommandType) (commandInteger & 0x07U), (int)(commandInteger >> 3)));
           
        }

        private static IAttributesTable ReadAttributeTable(Tile.Feature mbTileFeature, List<string> keys, List<Tile.Value> values)
        {
            var att = new AttributesTable();
            for (int i = 0; i < mbTileFeature.Tags.Count; i += 2)
            {
                string key = keys[(int)mbTileFeature.Tags[i]];
                var value = values[(int) mbTileFeature.Tags[i + 1]];
                if (value.HasBoolValue)
                    att.Add(key, value.BoolValue);
                else if (value.HasDoubleValue)
                    att.Add(key, value.DoubleValue);
                else if (value.HasFloatValue)
                    att.Add(key, value.FloatValue);
                else if (value.HasIntValue)
                    att.Add(key, value.IntValue);
                else if (value.HasSIntValue)
                    att.Add(key, value.SintValue);
                else if (value.HasStringValue)
                    att.Add(key, value.StringValue);
                else if (value.HasUIntValue)
                    att.Add(key, value.UintValue);
                else
                    att.Add(key, null);
            }

            return att;
        }
    }
}
