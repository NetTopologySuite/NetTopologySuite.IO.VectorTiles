using System.Collections;
using System.Collections.Generic;

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

        public IEnumerator<ulong> GetEnumerator()
        {
            return _tiles.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}