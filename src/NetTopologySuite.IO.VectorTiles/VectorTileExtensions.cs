namespace NetTopologySuite.IO.VectorTiles
{
    /// <summary>
    /// Contains extension method related to vector tiles.
    /// </summary>
    public static class VectorTileExtensions
    {
        /// <summary>
        /// Gets or creates the layer with the given name.
        /// </summary>
        /// <param name="vectorTile">The vector tile.</param>
        /// <param name="layerName">The layer name.</param>
        /// <returns>The layer.</returns>
        internal static Layer GetOrCreate(this VectorTile vectorTile, string layerName)
        {
            for (var l = 0; l < vectorTile.Layers.Count; l++)
            {
                var layer = vectorTile.Layers[l];
                if (layer.Name == layerName) return layer;
            }
            
            var newLayer = new Layer()
            {
                Name = layerName
            };
            vectorTile.Layers.Add(newLayer);
            return newLayer;
        }
    }
}