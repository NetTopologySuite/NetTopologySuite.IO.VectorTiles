using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetTopologySuite.IO.VectorTiles.Tests")]
namespace NetTopologySuite.IO.VectorTiles.Tiles
{
    /// <summary>
    /// Represents a tile.
    /// </summary>
    public class Tile
    {
        private readonly ulong _id;

        /// <summary>
        /// Creates a new tile from a given id.
        /// </summary>
        /// <param name="id"></param>
        public Tile(ulong id)
        {
            _id = id;

            var (x, y, zoom) = Tile.CalculateTile(id);
            this.X = x;
            this.Y = y;
            this.Zoom = zoom;
            this.CalculateBounds();
        }

        /// <summary>
        /// Creates a new tile.
        /// </summary>
        public Tile(int x, int y, int zoom)
        {
            this.X = x;
            this.Y = y;
            this.Zoom = zoom;

            _id = Tile.CalculateTileId(zoom, x, y);
            this.CalculateBounds();
        }

        private void CalculateBounds()
        {
            var n = Math.PI - ((2.0 * Math.PI * this.Y) / Math.Pow(2.0, this.Zoom));
            this.Left = (double) ((this.X / Math.Pow(2.0, this.Zoom) * 360.0) - 180.0);
            this.Top = (double) (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            n = Math.PI - ((2.0 * Math.PI * (this.Y + 1)) / Math.Pow(2.0, this.Zoom));
            this.Right = (double) (((this.X + 1) / Math.Pow(2.0, this.Zoom) * 360.0) - 180.0);
            this.Bottom = (double) (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            this.CenterLat = (double) ((this.Top + this.Bottom) / 2.0);
            this.CenterLon = (double) ((this.Left + this.Right) / 2.0);
        }

        /// <summary>
        /// The X position of the tile.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y position of the tile.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// The zoom level for this tile.
        /// </summary>
        public int Zoom { get; private set; }

        /// <summary>
        /// Gets the top.
        /// </summary>
        public double Top { get; private set; }

        /// <summary>
        /// Get the bottom.
        /// </summary>
        public double Bottom { get; private set; }

        /// <summary>
        /// Get the left.
        /// </summary>
        public double Left { get; private set; }

        /// <summary>
        /// Gets the right.
        /// </summary>
        public double Right { get; private set; }

        /// <summary>
        /// Gets the center lat.
        /// </summary>
        public double CenterLat { get; private set; }

        /// <summary>
        /// Gets the center lon.
        /// </summary>
        public double CenterLon { get; private set; }

        /// <summary>
        /// Gets the parent tile.
        /// </summary>
        public Tile Parent => new Tile(this.X / 2, this.Y / 2, this.Zoom - 1);

        /// <summary>
        /// Returns a hashcode for this tile position.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^
                   this.Y.GetHashCode() ^
                   this.Zoom.GetHashCode();
        }

        /// <summary>
        /// Returns true if the given object represents the same tile.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Tile other)
            {
                return other.X == this.X &&
                       other.Y == this.Y &&
                       other.Zoom == this.Zoom;
            }

            return false;
        }

        /// <summary>
        /// Returns a description for this tile.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.X}x-{this.Y}y@{this.Zoom}z";
        }

        /// <summary>
        /// Returns true if the given tiles are direct neighbours.
        /// </summary>
        /// <param name="tileId1">The first tile id.</param>
        /// <param name="tileId2">The second tile id.</param>
        /// <returns></returns>
        public static bool IsDirectNeighbour(ulong tileId1, ulong tileId2)
        {
            if (tileId1 == tileId2) return false;
            
            var tile1 = Tile.CalculateTile(tileId1);
            var tile2 = Tile.CalculateTile(tileId2);

            if (tile1.zoom != tile2.zoom)
            {
                return false;
            }

            if (tile1.x == tile2.x)
            {
                return (tile1.y == tile2.y + 1) ||
                       (tile1.y == tile2.y - 1);
            }
            else if (tile1.y == tile2.y)
            {
                return (tile1.x == tile2.x + 1) ||
                       (tile1.x == tile2.x - 1);
            }

            return false;
        }

        /// <summary>
        /// Calculates the tile id of the tile at position (0, 0) for the given zoom.
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns></returns>
        private static ulong CalculateTileId(int zoom)
        {
            if (zoom == 0)
            {
                // zoom level 0: {0}.
                return 0;
            }
            else if (zoom == 1)
            {
                return 1;
            }
            else if (zoom == 2)
            {
                return 5;
            }
            else if (zoom == 3)
            {
                return 21;
            }
            else if (zoom == 4)
            {
                return 85;
            }
            else if (zoom == 5)
            {
                return 341;
            }
            else if (zoom == 6)
            {
                return 1365;
            }
            else if (zoom == 7)
            {
                return 5461;
            }
            else if (zoom == 8)
            {
                return 21845;
            }
            else if (zoom == 9)
            {
                return 87381;
            }
            else if (zoom == 10)
            {
                return 349525;
            }
            else if (zoom == 11)
            {
                return 1398101;
            }
            else if (zoom == 12)
            {
                return 5592405;
            }
            else if (zoom == 13)
            {
                return 22369621;
            }
            else if (zoom == 14)
            {
                return 89478485;
            }
            else if (zoom == 15)
            {
                return 357913941;
            }
            else if (zoom == 16)
            {
                return 1431655765;
            }
            else if (zoom == 17)
            {
                return 5726623061;
            }
            else if (zoom == 18)
            {
                return 22906492245;
            }

            var size = (ulong) System.Math.Pow(2, 2 * (zoom - 1));
            var tileId = Tile.CalculateTileId(zoom - 1) + size;
            return tileId;
        }

        /// <summary>
        /// Calculates the tile id of the tile at position (x, y) for the given zoom.
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal static ulong CalculateTileId(int zoom, int x, int y)
        {
            var id = Tile.CalculateTileId(zoom);
            var width = (long) System.Math.Pow(2, zoom);
            return id + (ulong) x + (ulong) (y * width);
        }

        /// <summary>
        /// Calculate the tile given the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static (int x, int y, int zoom) CalculateTile(ulong id)
        {
            // find out the zoom level first.
            var zoom = 0;
            if (id > 0)
            {
                // only if the id is at least at zoom level 1.
                while (id >= Tile.CalculateTileId(zoom))
                {
                    // move to the next zoom level and keep searching.
                    zoom++;
                }

                zoom--;
            }

            // calculate the x-y.
            var local = id - Tile.CalculateTileId(zoom);
            var width = (ulong) System.Math.Pow(2, zoom);
            var x = (int) (local % width);
            var y = (int) (local / width);

            return (x, y, zoom);
        }

        /// <summary>
        /// Returns the id of this tile.
        /// </summary>
        public ulong Id => _id;

        /// <summary>
        /// Returns true if this tile is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (this.X < 0 || this.Y < 0 || this.Zoom < 0) return false; // some are negative.
                var size = System.Math.Pow(2, this.Zoom);
                return this.X < size && this.Y < size;
            }
        }

        /// <summary>
        /// Returns the tile at the given location at the given zoom.
        /// </summary>
        public static Tile? CreateAroundLocation(double lat, double lon, int zoom)
        {
            if (!Tile.CreateAroundLocation(lat, lon, zoom, out var x, out var y))
            {
                return null;
            }

            return new Tile(x, y, zoom);
        }

        /// <summary>
        /// Returns the tile at the given location at the given zoom.
        /// </summary>
        public static ulong CreateAroundLocationId(double lat, double lon, int zoom)
        {
            if (!Tile.CreateAroundLocation(lat, lon, zoom, out var x, out var y))
            {
                return ulong.MaxValue;
            }

            return Tile.CalculateTileId(zoom, x, y);
        }

        /// <summary>
        /// A fast method of calculating x-y without creating a tile object.
        /// </summary>
        public static bool CreateAroundLocation(double lat, double lon, int zoom, out int x, out int y)
        {
            if (lon == 180)
            {
                lon = lon - 0.000001;
            }

            if (lat > 85.0511 || lat < -85.0511)
            {
                x = 0;
                y = 0;
                return false;
            }

            var n = (int) System.Math.Floor(System.Math.Pow(2, zoom));

            x = (int) ((lon + 180.0) / 360.0 * (1 << zoom));
            var latRad = lat * Math.PI / 180.0;
            y = (int) ((1.0 - Math.Log(Math.Tan(latRad) +
                                       1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * (1 << zoom));
            return true;
        }

        /// <summary>
        /// Gets the tile id the given lat/lon belongs to one zoom level lower.
        /// </summary>
        public ulong GetSubTileIdFor(double lat, double lon)
        {
            const int factor = 2;
            var zoom = this.Zoom + 1;
            int x = 0, y = 0;
            if (lat >= this.CenterLat && lon < this.CenterLon)
            {
                x = this.X * factor;
                y = this.Y * factor;
            }
            else if (lat >= this.CenterLat && lon >= this.CenterLon)
            {
                x = this.X * factor + factor - 1;
                y = this.Y * factor;
            }
            else if (lat < this.CenterLat && lon < this.CenterLon)
            {
                x = this.X * factor;
                y = this.Y * factor + factor - 1;
            }
            else if (lat < this.CenterLat && lon >= this.CenterLon)
            {
                x = this.X * factor + factor - 1;
                y = this.Y * factor + factor - 1;
            }

            return Tile.CalculateTileId(zoom, x, y);
        }

        /// <summary>
        /// Returns the subtiles of this tile at the given zoom.
        /// </summary>
        public TileRange GetSubTiles(int zoom)
        {
            if (this.Zoom > zoom)
            {
                throw new ArgumentOutOfRangeException(nameof(zoom),
                    "Subtiles can only be calculated for higher zooms.");
            }

            if (this.Zoom == zoom)
            {
                // just return a range of one tile.
                return new TileRange(this.X, this.Y, this.X, this.Y, this.Zoom);
            }

            var factor = 1 << (zoom - this.Zoom);

            return new TileRange(
                this.X * factor,
                this.Y * factor,
                this.X * factor + factor - 1,
                this.Y * factor + factor - 1,
                zoom);
        }

        /// <summary>
        /// Inverts the X-coordinate.
        /// </summary>
        /// <returns></returns>
        public Tile InvertX()
        {
            var n = (int) System.Math.Floor(System.Math.Pow(2, this.Zoom));

            return new Tile(n - this.X - 1, this.Y, this.Zoom);
        }

        /// <summary>
        /// Inverts the Y-coordinate.
        /// </summary>
        /// <returns></returns>
        public Tile InvertY()
        {
            var n = (int) System.Math.Floor(System.Math.Pow(2, this.Zoom));

            return new Tile(this.X, n - this.Y - 1, this.Zoom);
        }

        internal (double x, double y) SubCoordinates(double lat, double lon)
        {
            var leftOffset = lon - this.Left;
            var bottomOffset = lat - this.Bottom;

            return (this.X + (leftOffset / (this.Right - this.Left)),
                this.Y + (bottomOffset / (this.Top - this.Bottom)));
        }

        public string ToGeoJson()
        {
            return $@"{{
                ""type"": ""Polygon"",
                ""coordinates"": [
                  [
                    [
                      {this.Left},
                      {this.Bottom}
                    ],
                    [
                      {this.Right},
                      {this.Bottom}
                    ],
                    [
                      {this.Right},
                      {this.Top}
                    ],
                    [
                      {this.Left},
                      {this.Top}
                    ],
                    [
                      {this.Left},
                      {this.Bottom}
                    ]
                  ]
                ]
              }}";
        }
    }
}