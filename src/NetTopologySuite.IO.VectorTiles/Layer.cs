using System.Collections.Generic;
using NetTopologySuite.Features;

namespace NetTopologySuite.IO.VectorTiles
{
    /// <summary>
    /// Abstract representation of a layer.
    /// </summary>
    public abstract class Layer
    {
        /// <summary>
        /// Gets or sets the name of the layer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public virtual bool IsEmpty { get; } = true;
        
        /// <summary>
        /// Gets the features.
        /// </summary>
        public IList<Feature> Features { get; } = new List<Feature>();
    }
}