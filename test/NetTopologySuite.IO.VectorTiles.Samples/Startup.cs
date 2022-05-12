using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetTopologySuite.IO.VectorTiles.Samples
{
    public class Startup
    {
        /** Small area data set options **/

        private static int minZoom = 6;
        private static int maxZoom = 14;

        private static string dataFile = "data/test.geojson";
        //private static string dataFile = "data/address_points.geojson";
        //private static string dataFile = "data/streets.geojson";
        //private static string dataFile = "data/parcels.geojson";

        /** Large area data set options **/

        //private static int minZoom = 1;
        //private static int maxZoom = 8;

        //private static string dataFile = "data/russia.osm.geojson";
        //private static string dataFile = "data/us_counties.geojson";


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapRazorPages());

            string jsonPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), dataFile);
            string json = File.ReadAllText(jsonPath);
            var reader = new GeoJsonReader();
            var features = reader.Read<FeatureCollection>(json);
            static IEnumerable<(IFeature feature, int zoom, string layerName)> ConfigureFeature(IFeature feature)
            {
                if (dataFile.IndexOf("3857") > -1)
                {
                    feature.Geometry.SRID = 3857;
                }

                for (int z = minZoom; z <= maxZoom; z++)
                {
                    var geom = feature?.Geometry;
                    if (geom is null)
                    {
                        throw new ArgumentNullException("geom null");
                    }

                    if (geom is Point || geom is MultiPoint)
                    {
                        yield return (feature, z, "points");
                    }
                    else if (geom is LineString || geom is MultiLineString)
                    {
                        yield return (feature, z, "lines");
                    }
                    else if (geom is Polygon || geom is MultiPolygon)
                    {
                        yield return (feature, z, "polygons");
                    }
                    else
                    {
                        string err = $"{geom.GeometryType} not supported";
                        throw new ArgumentOutOfRangeException(err);
                    }
                }
            }

            //Delete any previously created tiles.
            var dir = new DirectoryInfo("wwwroot/tiles");

            if (dir.Exists)
            {
                dir.Delete(true);
            }

            var tree = new VectorTileTree { { features, ConfigureFeature } };

            tree.GetExtents(out Pages.IndexModel._BBox, out Pages.IndexModel._MinZoom, out Pages.IndexModel._MaxZoom);

            tree.Write("wwwroot/tiles");
        }
    }
}
