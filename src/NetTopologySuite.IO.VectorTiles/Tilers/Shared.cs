using System;
using System.Collections.Generic;

namespace NetTopologySuite.IO.VectorTiles.Tilers
{
    internal static class Shared
    {
        /// <summary>
        /// Returns all tiles between the start and end tile that is crossed by the line formed by the two coordinates.
        ///
        /// Coordinates are formed of the integer part of the tile with the offset calculated inside that tile added.
        /// </summary>
        /// <param name="x1">The x coordinate of the start of the line.</param>
        /// <param name="y1">The y coordinate of the start of the line.</param>
        /// <param name="x2">The x coordinate of the end of the line.</param>
        /// <param name="y2">The y coordinate of the end of the line.</param>
        /// <returns>The tiles that form the line between the two given coordinate pairs.</returns>
        internal static IEnumerable<(int x, int y)> LineBetween(double x1, double y1, double x2, double y2)
        {
            var xDiff = x2 - x1;
            var yDiff = y2 - y1;
            
            if (Math.Abs(xDiff) <= double.Epsilon && 
                Math.Abs(yDiff) <= double.Epsilon) yield break;
            
            if (Math.Abs(xDiff) > Math.Abs(yDiff))
            { // we take x as denominator.
                var yPrevious = (int)Math.Floor(y1);
                var xPrevious = (int)Math.Floor(x1);
                var xLast = (int) Math.Floor(x2);
                var yLast = (int) Math.Floor(y2);
                
                var slope = (yDiff / xDiff);
                var y0 = y1 - (slope * x1);
                
                // with an increment of 1 we calculate y.
                var right = (xDiff > 0);
                // var up = (yDiff > 0);
                var xStep = right ? 1 : -1;
                var start = right ? (int)Math.Ceiling(x1) : (int)Math.Floor(x1);
                var end = right ? (int)Math.Floor(x2) : (int)Math.Ceiling(x2);
                yield return (xPrevious, yPrevious);
                for (var x = start; x != end + xStep; x += xStep)
                {
                    // REMARK: this can be more efficient but this way numerically more stable. 
                    // we prefer correctness over performance here.
                    // calculate next y.
                    var y = x * slope + y0;
                    var yRounded = (int)Math.Floor(y);
                    
                    if (yPrevious != yRounded)
                    {
                        // we have moved to a new y
                        // if we go right, we return the same y at the previous x.
                        // if we go left, we return the previous y at the same x.
                        if (right)
                        {
                            yield return (xPrevious, yRounded);
                        }
                        else if(xPrevious != x)
                        {
                            yield return (x, yPrevious);
                        }

                        yield return (x, yRounded);
                        xPrevious = x;
                        yPrevious = yRounded;
                    }
                    else if (xPrevious != x)
                    {
                        yield return (x, yRounded);
                        xPrevious = x;
                        yPrevious = yRounded;
                    }
                }

                if (xLast != xPrevious ||
                    yLast != yPrevious)
                {
                    if (yLast != yPrevious)
                    {
                        // we have moved to a new y
                        // if we go left, we return the previous y at the same x.
                        if (xPrevious != xLast)
                        {
                            yield return (xLast, yPrevious);
                        }
                    }

                    yield return (xLast, yLast);
                }
            }
            else
            { // we take y.
                foreach (var reversed in LineBetween(y1, x1, y2, x2))
                {
                    yield return (reversed.y, reversed.x);
                }
            }
        }
    }
}