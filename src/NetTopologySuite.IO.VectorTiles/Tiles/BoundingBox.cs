using System;

namespace NetTopologySuite.IO.VectorTiles.Tiles
{
    /// <summary>
    /// Represents a bounding box.
    /// </summary>
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public BoundingBox(double left, double bottom, double right, double top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public double Left { readonly get; set; }
        public double Bottom { readonly get; set; }
        public double Right { readonly get; set; }
        public double Top { readonly get; set; }

        public readonly override bool Equals(object? obj)
        {
            return obj is BoundingBox bounds && Equals(bounds);
        }

        public readonly bool Equals(BoundingBox other)
        {
            return Left == other.Left &&
                   Bottom == other.Bottom &&
                   Right == other.Right &&
                   Top == other.Top;
        }

        public readonly override int GetHashCode()
        {
            int hashCode = 1263652387;
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            return hashCode;
        }
        public readonly double[] ToArray() => new double[] { Left, Bottom, Right, Top };

        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !(left == right);
        }
    }

}
