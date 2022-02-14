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

            var jsonPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "test.geojson");
            var json = File.ReadAllText(jsonPath);
            var reader = new GeoJsonReader();
            var features = reader.Read<FeatureCollection>(json);
            static IEnumerable<(IFeature feature, int zoom, string layerName)> ConfigureFeature(IFeature feature)
            {
                for (var z = 6; z <= 14; z++)
                {
                    var geom = feature?.Geometry;
                    if (geom is null)
                    {
                        throw new ArgumentNullException("geom null");
                    }

                    if (geom is LineString)
                    {
                        yield return (feature, z, "cyclenetwork");
                    }
                    else if (geom is Point)
                    {
                        yield return (feature, z, "cyclenodes");
                    }
                    else
                    {
                        var err = $"{geom.GeometryType} not supported";
                        throw new ArgumentOutOfRangeException(err);
                    }
                }
            }
            var tree = new VectorTileTree { { features, ConfigureFeature } };
            tree.Write("wwwroot/tiles");
        }
    }
}
