using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Tiles
{
    internal static class TileExtensions
    {
        private static GeometryFactory _factory;

        public static GeometryFactory Factory
        {
            get => _factory ??= new GeometryFactory(new PrecisionModel(), 4326);
            set => _factory = value;
        }

        /// <summary>
        /// Gets the polygon representing the given tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <param name="margin">The margin (in %).</param>
        /// <returns>The polygon.</returns>
        public static Polygon ToPolygon(this Tile tile, int margin = 5)
        {
            var factor = margin / 100.0;
            var xMar = System.Math.Abs((tile.Right - tile.Left) * factor);
            var yMar = System.Math.Abs((tile.Top - tile.Bottom) * factor);

            // Get the factory
            var factory = Factory;

            // Create and fill sequence
            var cs = factory.CoordinateSequenceFactory.Create(5, 2);
            cs.SetOrdinate(0, Ordinate.X, tile.Left - xMar);
            cs.SetOrdinate(0, Ordinate.Y, tile.Top + yMar);
            cs.SetOrdinate(1, Ordinate.X, tile.Right + xMar);
            cs.SetOrdinate(1, Ordinate.Y, tile.Top + yMar);
            cs.SetOrdinate(2, Ordinate.X, tile.Right + xMar);
            cs.SetOrdinate(2, Ordinate.Y, tile.Bottom - yMar);
            cs.SetOrdinate(3, Ordinate.X, tile.Left - xMar);
            cs.SetOrdinate(3, Ordinate.Y, tile.Bottom - yMar);
            cs.SetOrdinate(4, Ordinate.X, tile.Left - xMar);
            cs.SetOrdinate(4, Ordinate.Y, tile.Top + yMar);

            // create shell
            var shell = factory.CreateLinearRing(cs);

            // return polygon
            return factory.CreatePolygon(shell);
        }
    }
}
