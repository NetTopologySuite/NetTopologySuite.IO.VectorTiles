using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tiles;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Issues
{
    public class Issue29
    {
        [Fact]
        public void MvtWriterGeometryIssue()
        {
            writeTile(8, 6, 4);
            writeTile(34, 24, 6);
        }

        private static void writeTile(int x, int y, int z)
        {
            var geometry = new WKTReader().Read(ItalyWkt);
            var feature = new Feature(geometry, new AttributesTable(new Dictionary<string, object>()
        {
            { "id", 1 }
        }));
            var tileDefinition = new VectorTiles.Tiles.Tile(x, y, z);
            var vectorTile = new VectorTile { TileId = tileDefinition.Id };
            var layer = new Layer { Name = "layer1" };
            vectorTile.Layers.Add(layer);
            layer.Features.Add(feature);

            byte[] result;
            MemoryStream? ms;
            using (ms = new MemoryStream(1024 * 32))
            {
                vectorTile.Write(ms);
                result = ms.ToArray();
            }

            File.WriteAllBytes($"{x}_{y}_{z}.mvt", result);
        }

        private static string ItalyWkt
        {
            get
            {
                return File.ReadAllText(Path.Combine("Issues", $"{typeof(Issue29).Name}.wkt"));
            }
        }
    }
}
