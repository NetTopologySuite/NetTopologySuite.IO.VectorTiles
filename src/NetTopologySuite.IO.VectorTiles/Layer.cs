using System.Collections.Generic;
using NetTopologySuite.Features;

namespace NetTopologySuite.IO.VectorTiles
{
    /// <summary>
    /// Abstract representation of a layer.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Gets or sets the name of the layer.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public virtual bool IsEmpty => this.Features == null || this.Features.Count == 0;

        /// <summary>
        /// Gets the features.
        /// </summary>
        public IList<IFeature> Features { get; } = new List<IFeature>();
    }
}