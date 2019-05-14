using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using NetTopologySuite.IO.VectorTiles.Tiles;

namespace NetTopologySuite.IO.VectorTiles.Tests.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            var features = (new GeoJsonReader()).Read<FeatureCollection>(File.ReadAllText("test.geojson"));
            var zoom = 14;
            
            var tree = new VectorTileTree();
            tree.Add(features, zoom);
            
            tree.Write("tiles");
        }
    }
}