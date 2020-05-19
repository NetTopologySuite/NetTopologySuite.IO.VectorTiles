using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Tests.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            var features = (new GeoJsonReader()).Read<FeatureCollection>(File.ReadAllText("test.geojson"));

            // build the vector tile tree.
            var tree = new VectorTileTree();
            tree.Add(features, ConfigureFeature);

            IEnumerable<(IFeature feature, int zoom, string layerName)> ConfigureFeature(IFeature feature)
            {
                for (var z = 12; z <= 14; z++)
                {
                    if (feature.Geometry is LineString)
                    {
                        yield return (feature, z, "cyclenetwork");
                    }
                    else if (feature.Geometry is Polygon)
                    {
                        yield return (feature, z, "polygons");
                    }
                    else
                    {
                        yield return (feature, z, "cyclenodes");
                    }
                }
            }

            // write the tiles to disk as mvt.
            Mapbox.MapboxTileWriter.Write(tree, "tiles");

            // write the tiles to disk as geojson.
            GeoJson.GeoJsonTileWriter.Write(tree, "tiles");
        }
    }
}