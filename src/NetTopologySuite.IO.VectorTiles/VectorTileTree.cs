using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.IO.VectorTiles
{
    /// <summary>
    /// A vector tile tree.
    /// </summary>
    public class VectorTileTree : IEnumerable<ulong>
    {
        private readonly Dictionary<ulong, VectorTile> _tiles = new Dictionary<ulong, VectorTile>();

        /// <summary>
        /// Tries to get the given tile.
        /// </summary>
        /// <param name="tileId">The tile id.</param>
        /// <param name="vectorTile">The resulting tile (if any).</param>
        /// <returns>True if the tile exists.</returns>
        public bool TryGet(ulong tileId, out VectorTile vectorTile)
        {
            return _tiles.TryGetValue(tileId, out vectorTile);
        }

        public VectorTile this[ulong tileId]
        {
            get => _tiles[tileId];
            set => _tiles[tileId] = value;
        }

        /// <summary>
        /// Gets an array of all tile IDs in the tree. 
        /// </summary>
        /// <returns>An array of all tile IDs in the tree. </returns>
        public List<ulong> GetTileIds()
        {
            return _tiles.Keys.ToList();
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return _tiles.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the bounding box and min/max zoom level of the tile tree.
        /// </summary>
        /// <param name="bounds">Bounding box as a double array [minX, minY, maxX, maxY]</param>
        /// <param name="minZoom">The min zoom level.</param>
        /// /// <param name="maxZoom">The max zoom level.</param>
        /// <returns></returns>
        public void GetExtents(out double[] bounds, out int minZoom, out int maxZoom)
        {
            var ids = GetTileIds();

            if (ids.Count == 0)
            {
                throw new Exception("Tree is empty.");
            }

            //Use first ID as initial bounds
            (int x, int y, int z) = Tiles.Tile.CalculateTile(ids[1]);

            minZoom = z;
            maxZoom = z;

            bounds = Tiles.Tile.GetBBox(x, y, z);

            double[] bbox;

            foreach (ulong id in ids)
            {
                (x, y, z) = Tiles.Tile.CalculateTile(id);

                minZoom = Math.Min(minZoom, z);
                maxZoom = Math.Max(maxZoom, z);

                //Only need to calculate the bbox for the min zoom level. 
                if (z == minZoom)
                {
                    bbox = Tiles.Tile.GetBBox(x, y, z);

                    bounds[0] = Math.Min(bbox[0], bounds[0]);
                    bounds[1] = Math.Min(bbox[1], bounds[1]);
                    bounds[2] = Math.Max(bbox[2], bounds[2]);
                    bounds[2] = Math.Max(bbox[2], bounds[2]);
                }
            }
        }
    }
}
