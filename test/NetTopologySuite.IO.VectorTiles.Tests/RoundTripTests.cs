using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests
{
    public class TestDifferentGeometryFormats : RoundTripBase
    {
        [Fact]
        public void test_encoder()
        {
            AssertRoundTrip("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))");
        }

        [Fact]
        public void test_encoder_smaller_one_pixel()
        {
            AssertRoundTripEmptyTile("POLYGON ((0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0))");
        }

        [Theory]
        [InlineData(0u, "LINESTRING (0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0)", false, true)]
        [InlineData(1u, "LINESTRING (0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0)", false, true)]
        [InlineData(2u, "LINESTRING (0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0)", true, true)]
        [InlineData(3u, "LINESTRING (0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0)", true, true)]
        public void test_encoder_with_min_lineal_extent(uint minLinealExtent, string wkt, bool empty, bool poorQuality)
        {
            if (!(wkt.StartsWith("LINESTRING") || wkt.StartsWith("MULTILINESTRING")))
                throw Xunit.Sdk.SkipException.ForSkip("Not a lineal geometry");

            if (empty)
                AssertRoundTripEmptyTile(wkt, minLinealExtent: minLinealExtent);
            else
                AssertRoundTrip(wkt, minLinealExtent: minLinealExtent, poorQuality: poorQuality);
        }

        [Theory]
        [InlineData(1, "POLYGON ((0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0))", false, true)]
        [InlineData(2, "POLYGON ((0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0))", true, true)]
        [InlineData(3, "POLYGON ((0 0, 0.1 0, 0.1 0.1, 0 0.1, 0 0))", true, true)]
        public void test_encoder_with_min_polygonal_extent(uint minPolygonalExtent, string wkt, bool empty, bool poorQuality)
        {
            if (!(wkt.StartsWith("POLYGON") || wkt.StartsWith("MULTIPOLYGON")))
                throw Xunit.Sdk.SkipException.ForSkip("Not a polygonal geometry");

            if (empty)
                AssertRoundTripEmptyTile(wkt, minPolygonalExtent: minPolygonalExtent);
            else
                AssertRoundTrip(wkt, minPolygonalExtent: minPolygonalExtent, poorQuality: poorQuality);
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
            AssertRoundTrip("POLYGON ((0 0, 4 0, 4 4, 0 4, 0 0), (1 1, 3 2.4, 2 1.6, 1 1))", //   # noqa
                 "{ \"type\": \"Polygon\", \"coordinates\": [[[0, 0], [0, 4], [4, 4], [4, 0], [0, 0]], [[1, 1], [2, 1.6], [3, 2.4], [1, 1]]] }");
        }

        [Fact]
        public void test_with_wkt()
        {
            AssertRoundTrip("LINESTRING(-2 -2,-2 2, 2 2, 2 -2, -2 -2)");
        }

        [Fact]
        public void test_encode_float_little_endian()
        {

            AssertRoundTrip("LINESTRING(-2 -2,-2 2, 2 2, 2 -2, -2 -2)",
                properties: ToAttributesTable(("floatval", 3.1415f)));

        }

        [Fact]
        public void test_encode_feature_with_id()
        {
            AssertRoundTrip("POINT (1 1)", "{\"type\": \"Point\",\"coordinates\": [1, 1]}", id: 42U);
        }

        [Fact]
        public void test_encode_polygon_reverse_winding_order()
        {
            AssertRoundTrip("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))");
        }

        [Fact]
        public void test_encode_multipoint()
        {
            AssertRoundTrip("MULTIPOINT((10 10), (20 20), (10 40), (40 40), (30 30), (40 20), (30 10))",
                "{\"type\": \"MultiPoint\",\"coordinates\": [[10, 10], [20, 20], [10, 40], [40, 40],[30, 30], [40, 20], [30, 10]]}");
        }

        [Fact]
        public void test_encode_multipoint_point_outside_extent()
        {
            AssertRoundTrip("MULTIPOINT((10 10), (20 20), (185 40), (30 10))",
                "{\"type\": \"MultiPoint\",\"coordinates\": [[10, 10], [20, 20], [30, 10]]}");
        }

        [Fact]
        public void test_encode_multipoint_first_point_outside_extent()
        {
            AssertRoundTrip("MULTIPOINT((185 40), (10 10), (20 20), (30 10))",
                "{\"type\": \"MultiPoint\",\"coordinates\": [[185, 40], [10, 10], [20, 20], [30, 10]]}");
        }

        [Fact]
        public void test_encode_multilinestring()
        {
            AssertRoundTrip("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))",
                "{\"type\": \"MultiLineString\",\"coordinates\": [[[10, 10], [20, 20], [10, 40]],[[40, 40], [30, 30], [40, 20], [30, 10]],]}");
        }

        [Fact]
        public void test_encode_multipolygon_normal_winding_order()
        {
            AssertRoundTrip(
                "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))",
                "{\"type\": \"MultiPolygon\",\"coordinates\": [[[[40, 40], [45, 30], [20, 45], [40, 40]]],[[[20, 35], [45, 20], [30, 5],[10, 10], [10, 30], [20, 35]],[[30, 20], [20, 25], [20, 15], [30, 20]]]]}");
        }

        [Fact]
        public void test_encode_multipolygon_normal_winding_order_zero_area()
        {
                // NB there is only one resultant polygon here
                AssertRoundTrip(
                    "MULTIPOLYGON (((40 40, 40 20, 40 45, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))",
                    "{\"type\": \"Polygon\",\"coordinates\": [[[20, 35], [45, 20], [30, 5],[10, 10], [10, 30], [20, 35]],[[30, 20], [20, 25], [20, 15], [30, 20]]]}");
        }

        [Fact]
        public void test_encode_multipolygon_reverse_winding_order()
        {
                //NB there is only one resultant polygon here
                AssertRoundTrip("MULTIPOLYGON (((10 10, 10 0, 0 0, 0 10, 10 10), (8 8, 2 8, 2 0, 8 0, 8 8)))",
                    "{\"type\": \"Polygon\",\"coordinates\": [[[10, 10], [10, 0], [0, 0],[0, 10], [10, 10]],[[8, 8], [2, 8], [2, 0], [8, 0], [8, 8]]]}");
        }

        [Fact/*(Skip = "Tile.Value: if value is equal to default, the value is not assigned (Boolean: false).")*/]
        public void test_encode_property_bool()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_bool_true", true), ("test_bool_false", false)));
        }

        [Fact]
        public void test_encode_property_long()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_int", 1), ("test_int_zero", 0), ("test_long", 1L), ("test_long_zero", 0L)),
                expectedProperties: ToAttributesTable(("test_int", 1L), ("test_int_zero", 0L), ("test_long", 1L), ("test_long_zero", 0L)));
        }

        [Fact]
        public void test_encode_property_sint()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_sint_zero", 0), ("test_sint_negative", -1), ("test_sint_positive", 1)),
                expectedProperties: ToAttributesTable(("test_sint_zero", 0L), ("test_sint_negative", -1L), ("test_sint_positive", 1L)));
        }

        [Fact]
        public void test_encode_property_uint()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_uint_zero", 0UL), ("test_uint_positive", 1UL), ("test_uint_max", ulong.MaxValue)),
                expectedProperties: ToAttributesTable(("test_uint_zero", 0UL), ("test_uint_positive", 1UL), ("test_uint_max", ulong.MaxValue)));
        }

        [Fact]
        public void test_encode_property_double()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_double", 1.0), ("test_double_zero", 0.0)),
                expectedProperties: ToAttributesTable(("test_double", 1.0), ("test_double_zero", 0.0)));
        }

        [Fact]
        public void test_encode_property_float()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_float", 1.0f), ("test_float_zero", 0.0f)),
                expectedProperties: ToAttributesTable(("test_float", 1.0f), ("test_float_zero", 0.0f)));
        }

        [Fact/*(Skip = "Tile.Value: if value is equal to default, the value is not assigned (String: \"\").")*/]
        public void test_encode_property_null()
        {
            AssertRoundTrip("POINT(0 0)", "{ \"type\": \"Point\", \"coordinates\": [0, 0] }",
                properties: ToAttributesTable(("test_null", null), ("test_empty", "")),
                expectedProperties: ToAttributesTable(("test_empty", "")));
        }

    }
}
