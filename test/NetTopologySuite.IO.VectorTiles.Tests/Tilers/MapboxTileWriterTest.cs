using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using System.IO;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tilers
{
    public class MapboxTileWriterTest
    {
        private readonly GeometryFactory _factory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);

        [Fact]
        public void TestEmptyPoint()
        {
            DoTestEmpty(_factory.CreatePoint());
        }
        [Fact]
        public void TestEmptyMultiPoint()
        {
            DoTestEmpty(_factory.CreateMultiPoint());
        }
        [Fact]
        public void TestEmptyLineString()
        {
            DoTestEmpty(_factory.CreateLineString());
        }
        [Fact]
        public void TestEmptyMultiLineString()
        {
            DoTestEmpty(_factory.CreateMultiLineString());
        }
        [Fact]
        public void TestEmptyPolygon()
        {
            DoTestEmpty(_factory.CreatePolygon());
        }
        [Fact]
        public void TestEmptyMultiPolygon()
        {
            DoTestEmpty(_factory.CreateMultiPolygon());
        }

        [Fact]
        public void TestPartlyEmptyMultiPoint()
        {
            var points = new Point[] {
                _factory.CreatePoint(new Coordinate(0, 0)),
                _factory.CreatePoint(),
                _factory.CreatePoint(new Coordinate(90, 0)) };
            DoTestEmpty(_factory.CreateMultiPoint(points), 1);
        }

        [Fact]
        public void TestPartlyEmptyMultiLineString()
        {
            var lines = new LineString[] {
                (LineString)_factory.CreatePoint(new Coordinate(0, 0)).Buffer(10).Boundary,
                _factory.CreateLineString(),
                (LineString)_factory.CreatePoint(new Coordinate(90, 0)).Buffer(10).Boundary };
            DoTestEmpty(_factory.CreateMultiLineString(lines), 1);
        }

        [Fact]
        public void TestPartlyEmptyMultiPolygon()
        {
            var polys = new Polygon[] {
                (Polygon)_factory.CreatePoint(new Coordinate(0, 0)).Buffer(10),
                _factory.CreatePolygon(),
                (Polygon)_factory.CreatePoint(new Coordinate(90, 0)).Buffer(10) };
            DoTestEmpty(_factory.CreateMultiPolygon(polys), 1);
        }

        private static void DoTestEmpty(Geometry geom, int numGeoms = 0)
        {
            Assert.NotNull(geom);
            if (numGeoms == 0) Assert.True(geom.IsEmpty);

            var vt1 = new VectorTile();
            var l1 = vt1.GetOrCreate("tst");
            var att = new AttributesTable();
            att.Add("id", 9);
            var f1 = new Feature(geom, att);
            l1.Features.Add(f1);
            using var ms = new MemoryStream();
            var e = Record.Exception(() => MapboxTileWriter.Write(vt1, ms, 1, 2));
            Assert.Null(e);
            Assert.True(ms.Length > 0);

            var rdr = new MapboxTileReader(geom.Factory);
            VectorTile vt2 = null;

            ms.Seek(0, SeekOrigin.Begin);
            e = Record.Exception(() => vt2 = rdr.Read(ms, new VectorTiles.Tiles.Tile(0)));
            Assert.Null(e);
            Assert.NotNull(vt2);
            Assert.Equal(1, vt2.Layers.Count);
            var l2 = vt2.Layers[0];
            Assert.Equal("tst", l2.Name);
            Assert.Equal(numGeoms, l2.Features.Count);

            if (numGeoms == 0) return;

            var f2 = l2.Features[0];
            Assert.Equal(f2.Geometry.OgcGeometryType, f1.Geometry.OgcGeometryType);
        }
    }
}
