using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.VectorTiles.Tilers;
using NetTopologySuite.IO.VectorTiles.Tiles;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Tilers
{
    public class LineStringTilerTests
    {
        [Fact]
        public void LineStringTiler_Tiles_LineStringInOneTile_Zoom5_ContainingTile()
        {
            var lineString = new LineString(new []
            {
                new Coordinate(4.712533950805664,
                    51.257456746633515
                ),
                new Coordinate( 4.78729248046875,
                    51.221722572383214),
            });

            var tiles = lineString.Tiles(5);
            Assert.Equal(new []
            {
                Tile.CreateAroundLocationId(51.257456746633515, 4.712533950805664, 5)
            }, tiles);
        }
        
        [Fact]
        public void LineStringTiler_Tiles_LineStringInMultipleTiles_Zoom16_ContainingTile()
        {
            var lineString = new LineString(new []
            {
                new Coordinate(4.712533950805664,
                    51.257456746633515
                ),
                new Coordinate( 4.78729248046875,
                    51.221722572383214),
            });

            var tiles = lineString.Tiles(16);
            Assert.Equal(new []
            {
                Tile.CreateAroundLocationId(51.257456746633515, 4.712533950805664, 16),
                Tile.CreateAroundLocationId(51.221722572383214, 4.787292480468750, 16)
            }, tiles);
        }
        
        [Fact]
        public void LineStringTiler_Cut_LineString_CutAt21_ShouldBeCutIn0OrMore()
        {
            var lineString = new LineString(new []
            {
                new Coordinate(4.3898534774780273, 51.186054229736328),
                new Coordinate(4.3878788948059082, 51.183872222900391),
                new Coordinate(4.3827724456787109, 51.181339263916016),
                new Coordinate(4.3851327896118164, 51.183269500732422),
                new Coordinate(4.3826436996459961, 51.181392669677734),
                new Coordinate(4.3882365226745605, 51.183692932128906),
                new Coordinate(4.3782811164855957, 51.179370880126953),
                new Coordinate(4.3905258178710938, 51.186126708984375),
                new Coordinate(4.3895831108093262, 51.188743591308594),
                new Coordinate(4.3895268440246582, 51.190467834472656),
                new Coordinate(4.3747630119323730, 51.177448272705078),
                new Coordinate(4.3956923484802246, 51.191341400146484),
                new Coordinate(4.3995976448059082, 51.191967010498047),
                new Coordinate(4.3775515556335449, 51.176448822021484),
                new Coordinate(4.3815684318542480, 51.172584533691406),
                new Coordinate(4.4039058685302734, 51.195121765136719),
                new Coordinate(4.4066252708435059, 51.1970100402832),
                new Coordinate(4.3830404281616211, 51.170722961425781),
                new Coordinate(4.3858428001403809, 51.170459747314453),
                new Coordinate(4.4076418876647949, 51.198310852050781),
                new Coordinate(4.3949790000915527, 51.169757843017578),
                new Coordinate(4.4061141014099121, 51.201618194580078),
                new Coordinate(4.4045424461364746, 51.204551696777344),
                new Coordinate(4.3981246948242188, 51.170070648193359),
                new Coordinate(4.4035434722900391, 51.2076530456543),
                new Coordinate(4.4008555412292480, 51.170303344726562),
                new Coordinate(4.4060611724853516, 51.171669006347656),
                new Coordinate(4.4071893692016600, 51.170017242431641),
                new Coordinate(4.4021854400634766, 51.209747314453125),
                new Coordinate(4.4011135101318359, 51.211509704589844),
                new Coordinate(4.4056010246276855, 51.168380737304688),
                new Coordinate(4.4100561141967773, 51.162185668945312),
                new Coordinate(4.3976798057556152, 51.212928771972656),
                new Coordinate(4.3992562294006348, 51.216156005859375),
                new Coordinate(4.4012904167175293, 51.218742370605469),
                new Coordinate(4.4120979309082031, 51.15960693359375),
                new Coordinate(4.4115667343139648, 51.157341003417969)
            });

            foreach (var tile in lineString.Tiles(21))
            {
                var tilePolygon = (new Tile(tile)).ToPolygon();
                var cutLineStrings = tilePolygon.Cut(lineString);
                Assert.NotNull(cutLineStrings);
                foreach (var cutLineString in cutLineStrings)
                {
                    Assert.NotNull(cutLineString);
                }
            }
        }
        
        [Fact]
        public void LineStringTiler_Cut_LineString_CutAt22_ShouldBeCutIn0OrMore()
        {
            var lineString = new LineString(new []
            {
                new Coordinate(4.3898534774780273, 51.186054229736328),
                new Coordinate(4.3878788948059082, 51.183872222900391),
                new Coordinate(4.3827724456787109, 51.181339263916016),
                new Coordinate(4.3851327896118164, 51.183269500732422),
                new Coordinate(4.3826436996459961, 51.181392669677734),
                new Coordinate(4.3882365226745605, 51.183692932128906),
                new Coordinate(4.3782811164855957, 51.179370880126953),
                new Coordinate(4.3905258178710938, 51.186126708984375),
                new Coordinate(4.3895831108093262, 51.188743591308594),
                new Coordinate(4.3895268440246582, 51.190467834472656),
                new Coordinate(4.3747630119323730, 51.177448272705078),
                new Coordinate(4.3956923484802246, 51.191341400146484),
                new Coordinate(4.3995976448059082, 51.191967010498047),
                new Coordinate(4.3775515556335449, 51.176448822021484),
                new Coordinate(4.3815684318542480, 51.172584533691406),
                new Coordinate(4.4039058685302734, 51.195121765136719),
                new Coordinate(4.4066252708435059, 51.1970100402832),
                new Coordinate(4.3830404281616211, 51.170722961425781),
                new Coordinate(4.3858428001403809, 51.170459747314453),
                new Coordinate(4.4076418876647949, 51.198310852050781),
                new Coordinate(4.3949790000915527, 51.169757843017578),
                new Coordinate(4.4061141014099121, 51.201618194580078),
                new Coordinate(4.4045424461364746, 51.204551696777344),
                new Coordinate(4.3981246948242188, 51.170070648193359),
                new Coordinate(4.4035434722900391, 51.2076530456543),
                new Coordinate(4.4008555412292480, 51.170303344726562),
                new Coordinate(4.4060611724853516, 51.171669006347656),
                new Coordinate(4.4071893692016600, 51.170017242431641),
                new Coordinate(4.4021854400634766, 51.209747314453125),
                new Coordinate(4.4011135101318359, 51.211509704589844),
                new Coordinate(4.4056010246276855, 51.168380737304688),
                new Coordinate(4.4100561141967773, 51.162185668945312),
                new Coordinate(4.3976798057556152, 51.212928771972656),
                new Coordinate(4.3992562294006348, 51.216156005859375),
                new Coordinate(4.4012904167175293, 51.218742370605469),
                new Coordinate(4.4120979309082031, 51.15960693359375),
                new Coordinate(4.4115667343139648, 51.157341003417969)
            });

            foreach (var tile in lineString.Tiles(22))
            {
                var tilePolygon = (new Tile(tile)).ToPolygon();
                var cutLineStrings = tilePolygon.Cut(lineString);
                Assert.NotNull(cutLineStrings);
                foreach (var cutLineString in cutLineStrings)
                {
                    Assert.NotNull(cutLineString);
                }
            }
        }
    }
}