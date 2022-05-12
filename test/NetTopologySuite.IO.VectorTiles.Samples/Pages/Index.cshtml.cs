using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetTopologySuite.IO.VectorTiles.Samples.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public double[] BBox { get { return IndexModel._BBox; } }

        public int MinZoom { get { return IndexModel._MinZoom; } }

        public int MaxZoom { get { return IndexModel._MaxZoom; } }

        internal static double[] _BBox;
        internal static int _MinZoom;
        internal static int _MaxZoom;
    }
}
