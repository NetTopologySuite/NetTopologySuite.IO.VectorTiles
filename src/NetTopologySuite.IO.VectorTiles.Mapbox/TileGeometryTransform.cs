using GeoAPI.Geometries;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    //
    internal struct TileGeometryTransform
    {
        public TileGeometryTransform(Tiles.Tile tile, uint extent) : this()
        {
            Top = tile.Top;
            Left = tile.Left;
            LatitudeStep = (tile.Top - tile.Bottom) / extent;
            LongitudeStep = (tile.Right - tile.Left) / extent;
        }
        public double Top { get; }
        public double Left { get; }
        public double LatitudeStep { get; }
        public double LongitudeStep { get; }

        public (int x, int y) Transform(Coordinate coordinate)
        {
            return ((int) ((coordinate.X - Left) / LongitudeStep),
                    (int) ((Top - coordinate.Y) / LatitudeStep));
        }
        public (int x, int y) Transform(Coordinate coordinate, ref int lastX, ref int lastY)
        {
            var res = Transform(coordinate);
            int dx = res.x - lastX;
            int dy = res.y - lastY;
            lastX = res.x;
            lastY = res.y;

            return (dx, dy);
        }
        public (int x, int y) Transform(ICoordinateSequence sequence, int index, ref int currentX, ref int currentY)
        {
            int localX = (int) (sequence.GetOrdinate(index, Ordinate.X) / LongitudeStep);
            int localY = (int) (Top - sequence.GetOrdinate(index, Ordinate.Y) / LatitudeStep);
            int dx = localX - currentX;
            int dy = localY - currentY;
            currentX = localX;
            currentY = localY;

            return (dx, dy);
        }
    }
}
