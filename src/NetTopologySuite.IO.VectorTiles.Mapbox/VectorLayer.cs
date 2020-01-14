namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    /// <summary>
    /// A single vector layer.
    /// </summary>
    public class VectorLayer
    {
        /// <summary>
        /// The maximum zoom.
        /// </summary>
        public int maxzoom { get; set; }
        
        /// <summary>
        /// The minimum zoom.
        /// </summary>
        public int minzoom { get; set; }
        
        /// <summary>
        /// The id.
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// The description.
        /// </summary>
        public string description { get; set; }
    }
}