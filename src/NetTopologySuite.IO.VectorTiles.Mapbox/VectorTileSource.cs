namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    /// <summary>
    /// The mapbox mvt style.json definition.
    /// </summary>
    public class VectorTileSource
    {
        /// <summary>
        /// The tile urls.
        /// </summary>
        public string[] tiles { get; set; }

        /// <summary>
        /// The minimum zoom.
        /// </summary>
        public int minzoom { get; set; }

        /// <summary>
        /// The maximum zoom.
        /// </summary>
        public int maxzoom { get; set; }

        /// <summary>
        /// The bounds.
        /// </summary>
        public double[] bounds { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The attribution required if any.
        /// </summary>
        public string attribution { get; set; } =
            "<a href=\"http://www.openstreetmap.org/about/\" target=\"_blank\">&copy; OpenStreetMap contributors</a>";

        /// <summary>
        /// The format.
        /// </summary>
        public string format { get; set; } = "pbf";

        /// <summary>
        /// The id.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The base name.
        /// </summary>
        public string basename { get; set; }

        /// <summary>
        /// The layers.
        /// </summary>
        public VectorLayer[] vector_layers { get; set; }

        /// <summary>
        /// The version #.
        /// </summary>
        public string version { get; set; } = "1.0";

        /// <summary>
        /// The tile json version #.
        /// </summary>
        public string tilejson { get; set; } = "2.0.0";
    }
}