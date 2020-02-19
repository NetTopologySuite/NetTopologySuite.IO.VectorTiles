using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Features;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using NetTopologySuite.IO.VectorTiles.Tiles;
using Newtonsoft.Json;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests
{
    public class BaseTestCase
    {
        static BaseTestCase()
        {
            FeatureExtensions.IdAttributeName = "uid";
        }

        /// <summary>
        /// Extent of a tile in pixel
        /// </summary>
        private const uint Extent = 8 * 4096;

        protected readonly IGeometryFactory Factory =
            GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);

        protected readonly WKTReader Reader = new WKTReader(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326));

        protected string LayerName;
        protected IAttributesTable FeatureProperties;
        protected IGeometry FeatureGeometry = null;
        protected JsonSerializer Serializer = GeoJsonSerializer.CreateDefault();

        public BaseTestCase()
        {
            LayerName = "water";
            FeatureProperties = new AttributesTable(new []
            {
                new KeyValuePair<string, object>("uid", 123),
                new KeyValuePair<string, object>("foo", "bar"),
                new KeyValuePair<string, object>("baz", "foo"),
            });

            FeatureGeometry = Reader.Read( "POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))");
        }

        protected void AssertRoundTrip(IGeometry inputGeometry, IGeometry expectedGeometry,
            string name = null, IAttributesTable properties = null, uint? id = null,
            double expectedNumFeatures = 1, IAttributesTable expectedProperties = null)
        {
            if (inputGeometry == null)
                inputGeometry = FeatureGeometry;
            if (string.IsNullOrWhiteSpace(name))
                name = LayerName;
            if (properties == null)
                properties = FeatureProperties;
            if (expectedProperties == null)
                expectedProperties = FeatureProperties;

            var featureS = new Feature(inputGeometry, properties);
            if (id.HasValue)
                featureS.Attributes[FeatureExtensions.IdAttributeName] = id;

            var vtS = new VectorTile {TileId = 0};
            var lyrS = new Layer {Name = name};
            lyrS.Features.Add(featureS);
            vtS.Layers.Add(lyrS);

            VectorTile vtD = null;
            using (var ms = new MemoryStream())
            {
                vtS.Write(ms, Extent);
                ms.Position = 0;
                vtD = new MapboxTileReader(Factory).Read(ms, new Tiles.Tile(0));
            }

            Assert.NotNull(vtD);
            Assert.False(vtD.IsEmpty);
            Assert.Equal(1, vtD.Layers.Count);
            Assert.Equal(expectedNumFeatures, vtD.Layers[0].Features.Count);
            var featureD = vtD.Layers[0].Features[0];

            if (featureS.Geometry is IPoint)
                Assert.True(featureS.Geometry.Distance(featureD.Geometry) < 0.05);
            else
                Assert.True(new HausdorffSimilarityMeasure().Measure(featureS.Geometry, featureD.Geometry) > 0.975);

            // TODO CHECK PROPERTIES
        }

        protected void AssertRoundTrip(string inputDefinition = null, int inputDefinitionKind = 0, string expectedDefinition = null, int expectedDefinitionKind = 0)
        {
            var input = ParseGeometry(inputDefinition, inputDefinitionKind);

            var expected = !string.IsNullOrEmpty(expectedDefinition)
                ? ParseGeometry(expectedDefinition, expectedDefinitionKind)
                : input.Copy();

            AssertRoundTrip(input, expected);
        }

        private IGeometry ParseGeometry(string definition, int definitionKind = 0)
        {
            switch (definitionKind)
            {
                case 0:
                    return Reader.Read(definition);
                case 1:
                    return Serializer.Deserialize<IGeometry>(new JsonTextReader(new StringReader(definition)));
            }
            throw new NotSupportedException();
        }
    }

    public class TestDifferentGeometryFormats : BaseTestCase
    {
        [Fact]
        public void test_encoder()
        {
            AssertRoundTrip("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))");
        }

        [Fact]
        public void test_encoder_point()
        {
            AssertRoundTrip("POINT (1 2)");
        }

        [Fact]
        public void test_encoder_multipoint()
        {
            AssertRoundTrip("MULTIPOINT (1 2, 3 4)");
        }

        [Fact]
        public void test_encoder_linestring()
        {
            AssertRoundTrip("LINESTRING (30 10, 10 30, 40 40)");
        }

        [Fact]
        public void test_encoder_multilinestring()
        {
            AssertRoundTrip("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");
        }

        [Fact]
        public void test_encoder_polygon()
        {
            AssertRoundTrip("POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))");
        }

        [Fact]
        public void test_encoder_polygon_w_hole()
        {
            AssertRoundTrip("POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10), (20 30, 35 35, 30 20, 20 30))"); // noqa
        }

        [Fact]
        public void test_encoder_multipolygon()
        {
            AssertRoundTrip(
                "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)), ((15 5, 40 10, 10 20, 5 10, 15 5)))"); //   # noqa
        }

        [Fact]
        public void test_encoder_multipolygon_w_hole()
        {
            AssertRoundTrip(
                "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))"); //  # noqa
        }

        [Fact]
        public void test_encoder_quantize_before_orient()
        {
            AssertRoundTrip("POLYGON ((0 0, 4 0, 4 4, 0 4, 0 0), (1 1, 3 2, 2 2, 1 1))"); //  # noqa
        }

        [Fact]
        public void test_encoder_winding_order_polygon()
        {
            // example from the spec
            // https://github.com/mapbox/vector-tile-spec/tree/master/2.1#4355-example-polygon   (# noqa)
            // the order given in the example is clockwise in a y-up coordinate
            // system, but the coordinate system given for the example is y-down!
            // therefore the y coordinate in this example is flipped negative.
            AssertRoundTrip("POLYGON ((3 -6, 8 -12, 20 -34, 3 -6))");
        }

        [Fact]
        public void test_encoder_winding_order_polygon_reverse()
        {
            // tests that encode _corrects_ the winding order
            // example is the same as above - note the flipped coordinate system.
            AssertRoundTrip("POLYGON ((3 -6, 20 -34, 8 -12, 3 -6))");
        }

        [Fact]
        public void test_encoder_winding_order_multipolygon()
        {
            // example from the spec
            // https://github.com/mapbox/vector-tile-spec/tree/master/2.1#4356-example-multi-polygon   (# noqa)
            // the order given in the example is clockwise in a y-up coordinate
            // system, but the coordinate system given for the example is y-down!
            AssertRoundTrip("MULTIPOLYGON (" +
                            "((0 0, 10 0, 10 -10, 0 -10, 0 0))," +
                            "((11 -11, 20 -11, 20 -20, 11 -20, 11 -11)," +
                            " (13 -13, 13 -17, 17 -17, 17 -13, 13 -13)))");
        }

        [Fact]
        public void test_encoder_ensure_winding_after_quantization()
        {
            //# should be single polygon with hole
            AssertRoundTrip("POLYGON ((0 0, 4 0, 4 4, 0 4, 0 0), (1 1, 3 2.4, 2 1.6, 1 1))"); //   # noqa
            // but becomes multi-polygon
            // expected_geometry=[[[[0, 0], [0, 4], [4, 4], [4, 0], [0, 0]]],
            //                   [[[1, 1], [2, 2], [3, 2], [1, 1]]]])
        }

        [Fact]
        public void test_with_wkt()
        {
            AssertRoundTrip("LINESTRING(-71.160281 42.258729,-71.160837 43.259113,-71.161144 42.25932)",
                expectedDefinition: @"{ 'type': 'LineString', 'coordinates': [[-71, 42], [-71, 43], [-71, 42]] }",
                expectedDefinitionKind: 1);
        }

        public void test_with_invalid_geometry()
        {
            //expected_result = ('Can\'t do geometries that are not wkt, wkb, or '
            //                   'shapely geometries')
            //with AssertRaises(NotImplementedError) as ex:
            //    mapbox_vector_tile.encode([{
            //        "name": self.layer_name,
            //        "features": [{
            //            "geometry": "xyz",
            //            "properties": self.feature_properties
            //        }]
            //    }])
            //AssertEqual(str(ex.exception), expected_result)
        }
//    public void test_encode_unicode_property() {
//        if PY3:
//            func = str
//        else:
//            func = unicode
//        geometry = "LINESTRING(-71.160281 42.258729,-71.160837 43.259113,-71.161144 42.25932)"  # noqa
//        properties = {
//            "foo": func(self.feature_properties["foo"]),
//            "baz": func(self.feature_properties["baz"]),
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={
//                'type': 'LineString',
//                'coordinates': [[-71, 42], [-71, 43], [-71, 42]]
//            },
//            properties=properties)

//    public void test_encode_unicode_property_key() {
//        geometry = "LINESTRING(-71.160281 42.258729,-71.160837 43.259113,-71.161144 42.25932)"  # noqa
//        properties = {
//            u'☺': u'☺'
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={
//                'type': 'LineString',
//                'coordinates': [[-71, 42], [-71, 43], [-71, 42]]
//            },
//            properties=properties)
//    }
//    public void test_encode_float_little_endian() {
//        geometry = "LINESTRING(-71.160281 42.258729,-71.160837 43.259113,-71.161144 42.25932)"  # noqa
//        properties = {
//            'floatval': 3.14159
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={
//                'type': 'LineString',
//                'coordinates': [[-71, 42], [-71, 43], [-71, 42]]
//            },
//            properties=properties)
//    }
//    public void test_encode_feature_with_id() {
//        geometry = 'POINT(1 1)'
//        AssertRoundTrip(input_geometry=geometry,
//                             expected_geometry={
//                                 'type': 'Point',
//                                 'coordinates': [1, 1]
//                             },
//                             id=42)

//    public void test_encode_point() {
//        geometry = 'POINT(1 1)'
//        AssertRoundTrip(input_geometry=geometry,
//                             expected_geometry={
//                                 'type': 'Point',
//                                 'coordinates': [1, 1]
//                             },
//                             id=42)
//    }
//    public void test_encode_polygon_reverse_winding_order() {
//        geometry = 'POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))'
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={
//                'type': 'Polygon',
//                'coordinates': [[[0, 0], [0, 1], [1, 1], [1, 0], [0, 0]]]
//            })
//    }
//    public void test_encode_multipoint() {
//        geometry = 'MULTIPOINT((10 10), (20 20), (10 40), (40 40), (30 30), (40 20), (30 10))'  # noqa
//        AssertRoundTrip(input_geometry=geometry,
//                             expected_geometry={
//                                 'type': 'MultiPoint',
//                                 'coordinates': [
//                                     [10, 10], [20, 20], [10, 40], [40, 40],
//                                     [30, 30], [40, 20], [30, 10]
//                                 ]
//                             })
//    }
//    public void test_encode_multilinestring() {
//        geometry = 'MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))'  # noqa
//        AssertRoundTrip(input_geometry=geometry,
//                             expected_geometry={
//                                 'type': 'MultiLineString',
//                                 'coordinates': [
//                                     [[10, 10], [20, 20], [10, 40]],
//                                     [[40, 40], [30, 30], [40, 20], [30, 10]],
//                                 ]
//                             })
//    }
//    public void test_encode_multipolygon_normal_winding_order() {
//        geometry = 'MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))'  # noqa
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'MultiPolygon',
//                               'coordinates': [
//                                   [[[40, 40], [45, 30], [20, 45], [40, 40]]],
//                                   [
//                                       [[20, 35], [45, 20], [30, 5],
//                                        [10, 10], [10, 30], [20, 35]],
//                                       [[30, 20], [20, 25], [20, 15], [30, 20]]
//                                   ]
//                               ]},
//            expected_len=1)
//    }
//    public void test_encode_multipolygon_normal_winding_order_zero_area() {
//        geometry = 'MULTIPOLYGON (((40 40, 40 20, 40 45, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))'  # noqa
//        # NB there is only one resultant polygon here
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Polygon',
//                               'coordinates': [
//                                   [[20, 35], [45, 20], [30, 5],
//                                    [10, 10], [10, 30], [20, 35]],
//                                   [[30, 20], [20, 25], [20, 15], [30, 20]]
//                               ]},
//            expected_len=1)
//    }
//    public void test_encode_multipolygon_reverse_winding_order() {
//        geometry = 'MULTIPOLYGON (((10 10, 10 0, 0 0, 0 10, 10 10), (8 8, 2 8, 2 0, 8 0, 8 8)))'  # noqa
//        # NB there is only one resultant polygon here
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Polygon',
//                               'coordinates': [
//                                   [[10, 10], [10, 0], [0, 0],
//                                    [0, 10], [10, 10]],
//                                   [[8, 8], [2, 8], [2, 0], [8, 0], [8, 8]]
//                               ]},
//            expected_len=1)
//    }
//    public void test_encode_property_bool() {
//        geometry = 'POINT(0 0)'
//        properties = {
//            'test_bool_true': True,
//            'test_bool_false': False
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Point', 'coordinates': [0, 0]},
//            properties=properties)
//    }
//    public void test_encode_property_long() {
//        geometry = 'POINT(0 0)'
//        properties = {
//            'test_int': int (1),
//            'test_long': long (1)
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Point', 'coordinates': [0, 0]},
//            properties=properties)
//    }
//    public void test_encode_property_null() {
//        geometry = 'POINT(0 0)'
//        properties = {
//            'test_none': None,
//            'test_empty': ""
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Point', 'coordinates': [0, 0]},
//            properties=properties,
//            expected_properties={'test_empty': ''})
//    }
//    public void test_encode_property_list() {
//        geometry = 'POINT(0 0)'
//        properties = {
//            'test_list': [1, 2, 3],
//            'test_empty': ""
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry={'type': 'Point', 'coordinates': [0, 0]},
//            properties=properties,
//            expected_properties={'test_empty': ''})
//    }
//    public void test_encode_multiple_values_test() {
//        geometry = 'POINT(0 0)'
//        properties1 = dict(foo= 'bar', baz= 'bar')
//        properties2 = dict(quux= 'morx', baz= 'bar')
//        name = 'foo'
//        feature1 = dict(geometry= geometry, properties= properties1)
//        feature2 = dict(geometry= geometry, properties= properties2)
//        source = [{
//            "name": name,
//            "features": [feature1, feature2]
//        }]
//        encoded = encode(source)
//        decoded = decode(encoded)
//        AssertIn(name, decoded)
//        layer = decoded[name]
//        features = layer['features']
//        AssertEqual(2, len(features))
//        AssertEqual(features[0]['properties'], properties1)
//        AssertEqual(features[1]['properties'], properties2)
//    }
//    public void test_encode_rounding_floats() {
//        geometry = 'LINESTRING(1.1 1.1, 41.5 41.8)'
//        exp_geoemtry = {
//            'type': 'LineString',
//            'coordinates': [[1, 1], [42, 42]]
//        }
//        AssertRoundTrip(
//            input_geometry=geometry,
//            expected_geometry=exp_geoemtry,
//        )
//    }
//    public void test_too_small_linestring() {
//        from mapbox_vector_tile import encode
//        from mapbox_vector_tile.encoder import on_invalid_geometry_make_valid
//        import shapely.wkt
//        shape = shapely.wkt.loads(
//            'LINESTRING(-71.160281 42.258729,-71.160837 42.259113,-71.161144 42.25932)')  # noqa
//        features = [dict(geometry = shape, properties ={ })]
//        pbf = encode({ 'name': 'foo', 'features': features},
//                     on_invalid_geometry=on_invalid_geometry_make_valid)
//        result = decode(pbf)
//        features = result['foo']['features']
//        AssertEqual(0, len(features))
//    }
//public void test_encode_1_True_values() {
//        geometry = 'POINT(0 0)'
//        properties = {
//            'foo': True,
//            'bar': 1,
//        }
//        source = [{
//            'name': 'layer',
//            'features': [{
//                'geometry': geometry,
//                'properties': properties
//            }]
//        }]
//        encoded = encode(source)
//        decoded = decode(encoded)
//        layer = decoded['layer']
//        features = layer['features']
//        act_props = features[0]['properties']
//        AssertEquals(act_props['foo'], True)
//        AssertEquals(act_props['bar'], 1)
//        AssertTrue(isinstance(act_props['foo'], bool))
//        AssertFalse(isinstance(act_props['bar'], bool))

//    }
    }
}
