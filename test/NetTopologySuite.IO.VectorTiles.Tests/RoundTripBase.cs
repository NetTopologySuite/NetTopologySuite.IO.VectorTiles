using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using Newtonsoft.Json;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests
{
    public class RoundTripBase
    {
        private const string UID = "uid";

        static RoundTripBase()
        {
            AttributesTable.AddAttributeWithIndexer = true;
        }

        /// <summary>
        /// Extent of a tile in pixel
        /// </summary>
        private const uint Extent = 4096 /* * 8 */;

        protected readonly GeometryFactory Factory = new GeometryFactory(new PrecisionModel(), 4326);

        protected readonly WKTReader Reader = new WKTReader(new GeometryFactory(new PrecisionModel(), 4326));

        protected string LayerName;
        protected IAttributesTable FeatureProperties;
        protected Geometry FeatureGeometry;
        protected JsonSerializer Serializer = GeoJsonSerializer.CreateDefault();

        public RoundTripBase()
        {
            LayerName = "water";
            FeatureProperties = new AttributesTable(new[]
            {
                new KeyValuePair<string, object>(UID, 123U),
                new KeyValuePair<string, object>("foo", "bar"),
                new KeyValuePair<string, object>("baz", "foo"),
            });

            FeatureGeometry = Reader.Read("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))");
        }

        protected void AssertRoundTrip(Geometry inputGeometry, Geometry expectedGeometry,
            string? name = null, IAttributesTable? properties = null, uint? id = null,
            int expectedNumFeatures = 1, IAttributesTable? expectedProperties = null,
            uint minLinealExtent = MapboxTileWriter.DefaultMinLinealExtent,
            uint minPolygonalExtent = MapboxTileWriter.DefaultMinPolygonalExtent, bool poorQuality = false)
        {
            if (inputGeometry == null)
                inputGeometry = FeatureGeometry;
            if (expectedGeometry == null)
                expectedGeometry = inputGeometry;

            if (string.IsNullOrWhiteSpace(name))
                name = LayerName;
            if (properties == null)
                properties = FeatureProperties;
            if (expectedProperties == null)
                expectedProperties = properties;

            var featureS = new Feature(inputGeometry, properties);
            if (id.HasValue)
                featureS.Attributes[UID] = id;

            var vtS = new VectorTile {TileId = 0};
            var lyrS = new Layer {Name = name};
            lyrS.Features.Add(featureS);
            vtS.Layers.Add(lyrS);

            VectorTile? vtD = null;
            using (var ms = new MemoryStream())
            {
                vtS.Write(ms, minLinealExtent, minPolygonalExtent, Extent);
                ms.Position = 0;
                vtD = new MapboxTileReader(Factory).Read(ms, new VectorTiles.Tiles.Tile(0));
            }

            Assert.NotNull(vtD);
            Assert.False(vtD.IsEmpty, "Parsed vector tile is empty");
            Assert.Equal(1, vtD.Layers.Count);
            Assert.Equal(expectedNumFeatures, vtD.Layers[0].Features.Count);
            var featureD = vtD.Layers[0].Features[0];

            // Perform geometry checks
            CheckGeometry(expectedGeometry, featureD.Geometry, poorQuality);

            // Perform attribute checks
            CheckAttributes(expectedProperties, featureD.Attributes);
        }

        private void CheckGeometry(Geometry expected, Geometry parsed, bool poorQuality = false)
        {
            // Type checked
            Assert.Equal(expected.OgcGeometryType, parsed.OgcGeometryType);

            // Coordinates count checked
            Assert.Equal(expected.Coordinates.Length, parsed.Coordinates.Length);

            // If the conversion is known to have poor quality than don't test it.
            if (poorQuality) return;

            double pixelDiff = (85.5 * 2);
            double error = 2 * System.Math.Sqrt(pixelDiff * pixelDiff * 2) / Extent;
            if (expected is IPuntal)
            {
                // In MultiPoint case check all points that forms it
                for (int i = 0; i < expected.NumGeometries; i++)
                {
                    var expectedPoint = (Point)expected.GetGeometryN(i);
                    var parsedPoint = (Point)parsed.GetGeometryN(i);
                    double test = expectedPoint.Distance(parsedPoint);
                    Assert.True(test < error);
                }
            }
            else
            {
                double similarityMeasure = new HausdorffSimilarityMeasure().Measure(expected, parsed);
                Assert.True(similarityMeasure > 1 - error, $"Similarity measure insufficient: {similarityMeasure}\n\tExpected: {expected}\n\tActual: {parsed}");
            }
        }

        private void CheckAttributes(IAttributesTable expected, IAttributesTable parsed)
        {
            Assert.Equal(expected.Count, parsed.Count);

            string[] expectedNames = expected.GetNames();
            string[] parsedNames = expected.GetNames();
            for (int i = 0; i < expectedNames.Length; i++)
            {
                Assert.Equal(expectedNames[i], parsedNames[i]);
                if (expectedNames[i] != UID)
                    Assert.Equal(expected[expectedNames[i]], parsed[parsedNames[i]]);
                else
                    Assert.Equal((ulong) (uint) expected[expectedNames[i]], parsed[parsedNames[i]]);
            }
        }

        protected void AssertRoundTripEmptyTile(string inputDefinition, string? name = null,
            IAttributesTable? properties = null, uint? id = null,
            uint minLinealExtent = MapboxTileWriter.DefaultMinLinealExtent,
            uint minPolygonalExtent = MapboxTileWriter.DefaultMinPolygonalExtent)
        {
            var inputGeometry = ParseGeometry(inputDefinition);

            if (string.IsNullOrWhiteSpace(name))
                name = LayerName;
            properties ??= FeatureProperties;

            var featureS = new Feature(inputGeometry, properties);
            if (id.HasValue)
                featureS.Attributes[UID] = id;

            var vtS = new VectorTile { TileId = 0 };
            var lyrS = new Layer { Name = name };
            lyrS.Features.Add(featureS);
            vtS.Layers.Add(lyrS);

            VectorTile? vtD = null;
            using (var ms = new MemoryStream())
            {
                vtS.Write(ms, minLinealExtent, minPolygonalExtent);
                ms.Position = 0;
                vtD = new MapboxTileReader(Factory).Read(ms, new VectorTiles.Tiles.Tile(0));
            }

            Assert.NotNull(vtD);
            Assert.True(vtD.IsEmpty);
        }

        protected void AssertRoundTrip(string inputDefinition, string? expectedDefinition = null,
            string? name = null, IAttributesTable? properties = null, uint? id = null,
            int expectedNumFeatures = 1, IAttributesTable? expectedProperties = null,
            uint minLinealExtent = MapboxTileWriter.DefaultMinLinealExtent,
            uint minPolygonalExtent = MapboxTileWriter.DefaultMinPolygonalExtent, bool poorQuality = false)
        {
            var input = ParseGeometry(inputDefinition);

            var expected = !string.IsNullOrEmpty(expectedDefinition)
                ? ParseGeometry(expectedDefinition)
                : input.Copy();

            AssertRoundTrip(input, expected, name, properties, id,
                expectedNumFeatures, expectedProperties, minLinealExtent, minPolygonalExtent, poorQuality);
        }

        private Geometry ParseGeometry(string definition)
        {
            if (!definition.TrimStart().StartsWith("{"))
                return Reader.Read(definition);
            return Serializer.Deserialize<Geometry>(new JsonTextReader(new StringReader(definition)));
        }

        protected static IAttributesTable? ToAttributesTable(params (string Label, object? Value)[]? attributes)
        {
            if (attributes == null || attributes.Length == 0)
                return null;

            AttributesTable.AddAttributeWithIndexer = true;
            var res = new AttributesTable();
            foreach ((string label, object? value) in attributes)
                res[label] = value;

            return res;
        }
    }
}
