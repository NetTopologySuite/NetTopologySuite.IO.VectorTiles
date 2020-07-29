using System;

namespace NetTopologySuite.IO.VectorTiles.Tiles.WebMercator
{
    public static class WebMercatorHandler
    {
        // https://gist.github.com/nagasudhirpulla/9b5a192ccaca3c5992e5d4af0d1e6dc4
        
        private const int EarthRadius = 6378137;
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;
        
        //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
        public static (double x, double y) LatLonToMeters(double lat, double lon)
        {
            var x = lon * OriginShift / 180;
            var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            y = y * OriginShift / 180;
            return (x, y);
        }
        
        //Converts XY point from (Spherical) Web Mercator EPSG:3785 (unofficially EPSG:900913) to lat/lon in WGS84 Datum
        public static (double x, double y) MetersToLatLon((double x, double y) m)
        {
            var x = (m.x / OriginShift) * 180;
            var y = (m.y / OriginShift) * 180;
            y = 180 / Math.PI * (2 * Math.Atan(Math.Exp(y * Math.PI / 180)) - Math.PI / 2);
            return (x, y);
        }
        
        //Converts EPSG:900913 to pyramid pixel coordinates in given zoom level
        public static (double x, double y) MetersToPixels((double x, double y) m, int zoom, int tileSize)
        {
            var res = Resolution(zoom, tileSize);
            var x = (m.x + OriginShift) / res;
            var y = (m.y + OriginShift) / res;
            return (x, y);
        }
        
        //Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
        public static (double x, double y) PixelsToMeters((double x, double y) p, int zoom, int tileSize)
        {
            var res = Resolution(zoom, tileSize);
            var x = p.x * res - OriginShift;
            var y = p.y * res - OriginShift;
            return (x, y);
        }
        
        //Resolution (meters/pixel) for given zoom level (measured at Equator)
        public static double Resolution(int zoom, int tileSize)
        {
            return InitialResolution(tileSize) / (Math.Pow(2, zoom));
        }

        public static double InitialResolution(int tileSize)
        {
            return  2 * Math.PI * EarthRadius / tileSize;
        }
    }
}