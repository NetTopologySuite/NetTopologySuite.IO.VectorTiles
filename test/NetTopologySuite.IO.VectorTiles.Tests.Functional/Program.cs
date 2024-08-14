using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using static NetTopologySuite.IO.VectorTiles.VectorTileTreeExtensions;
using System.Linq;

namespace NetTopologySuite.IO.VectorTiles.Tests.Functional
{
    class Program
    {
        static void Main()
        {
            //Original functional test data. (31,360 coordinates)
            RunTest("test.geojson", 12, 14, "31,360 coordinates");

            //Points for all addresses in Fort Collins, CO. (86,576 points)
            RunTest("address_points.geojson", 12, 14, "86,576 points");

            //Russia polygon boundary and node points. (4,427 features - 30 polygons / 181 lines / 4,245 points - 131,888 coordinates)
            RunTest("russia.osm.geojson", 1, 3, "30 polygons / 181 lines / 4,245 points - 131,888 coordinates");

            //Lines for all streets in Fort Collins, CO. (10,586 features - 10,596 lines - 212,441 coordinates)
            RunTest("streets.geojson", 12, 14, "10,586 features - 10,596 lines - 212,441 coordinates");

            //Polygons for all parcel boundaries in Fort Collins, CO. (69,915 features - 71,450 polygons - 1,499,009 coordinates)
            RunTest("parcels.geojson", 12, 14, "69,915 features - 71,450 polygons - 1,499,009 coordinates");

            //US county polygons. (3,221 features - 3,429 polygons - 99,369 coordinates)
            RunTest("us_counties.geojson", 4, 6, "3,221 features - 3,429 polygons - 99,369 coordinates");
        }

        private static void RunTest(string fileName, int minZoom, int maxZoom, string stats)
        {
            var tilesDir = new DirectoryInfo("tiles");

            // Empty tile directories to ensure they don't have any old files in them that could cause the test to fail.
            if (tilesDir.Exists)
            {
                tilesDir.Delete(true);
            }

            Console.WriteLine($"Testing file \"{fileName}\" ({stats})...\n");

            var stopwatch = new System.Diagnostics.Stopwatch();

            var features = (new GeoJsonReader()).Read<FeatureCollection>(File.ReadAllText($"data/{fileName}"));

            stopwatch.Start();

            // build the vector tile tree.
            var tree = new VectorTileTree
            {
                { features, GetFeatureConfigFunc(minZoom, maxZoom) }
            };

            // write the tiles to disk as mvt.
            Mapbox.MapboxTileWriter.Write(tree, "tiles", 1, 2);

            stopwatch.Stop();

            // write the tiles to disk as geojson.
            GeoJson.GeoJsonTileWriter.Write(tree, "tiles");

            Console.WriteLine($"{tree.Count()} tiles generated");
            Console.WriteLine($"Completed in {stopwatch.Elapsed.TotalMilliseconds} ms");

            Console.WriteLine("\n*************************************************\n");
        }

        /// <summary>
        /// A method that wraps a callback function and passes in zoom level ranges.
        /// </summary>
        /// <param name="minZoom"></param>
        /// <param name="maxZoom"></param>
        /// <returns></returns>
        private static ToFeatureZoomAndLayerFunc GetFeatureConfigFunc(int minZoom, int maxZoom)
        {
            // An example of a callback function that can be used to specify the zoom levels and layers each feature should be added to.
            IEnumerable<(IFeature feature, int zoom, string layerName)> config(IFeature feature)
            {
                var filtered = new List<(IFeature, int, string)>();

                for (int z = minZoom; z <= maxZoom; z++)
                {
                    if (feature.Geometry.OgcGeometryType == OgcGeometryType.GeometryCollection)
                    {
                        for (int i = 0; i < feature.Geometry.NumGeometries; i++)
                        {
                            string layer = GetGeometryLayerName(feature.Geometry.GetGeometryN(i));

                            if (!string.IsNullOrEmpty(layer))
                            {
                                filtered.Add((feature, z, layer));
                            }
                        }
                    }
                    else
                    {
                        string layer = GetGeometryLayerName(feature.Geometry);

                        if (!string.IsNullOrEmpty(layer))
                        {
                            filtered.Add((feature, z, layer));
                        }
                    }
                }

                return filtered;
            }

            return config;
        }

        private static string GetGeometryLayerName(Geometry geom)
        {
            if (geom is Point || geom is MultiPoint)
            {
                return "points";
            }
            else if (geom is LineString || geom is MultiLineString)
            {
               return "lines";
            }
            else if (geom is Polygon || geom is MultiPolygon)
            {
                return "polygons";
            }

            return string.Empty;
        }

    }
}
