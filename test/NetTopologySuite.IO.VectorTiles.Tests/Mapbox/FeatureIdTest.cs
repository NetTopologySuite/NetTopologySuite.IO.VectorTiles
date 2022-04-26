using NetTopologySuite.IO.VectorTiles.Mapbox;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Mapbox
{
    public class FeatureId
    {
        [Fact]
        private void TestFeatureId()
        {
            var featureS = new Features.Feature(new Geometries.Point(0, 0), new Features.AttributesTable(new Dictionary<string, object>()
            {
                {"id", 12345 }
            }));

            FeatureIdReadWriteTest(featureS, Convert.ToUInt64(featureS.Attributes["id"]), null);
        }

        [Fact]
        private void TestCustomFeatureId()
        {
            var featureS = new Features.Feature(new Geometries.Point(0, 0), new Features.AttributesTable(new Dictionary<string, object>()
            {
                {"myCustomId", 12345 }
            }));

            FeatureIdReadWriteTest(featureS, Convert.ToUInt64(featureS.Attributes["myCustomId"]), "myCustomId");
        }

        private void FeatureIdReadWriteTest(Features.Feature featureS, ulong expectedId, string? idAttributeName = null)
        {
            var vtS = new VectorTile { TileId = 0 };
            var lyrS = new Layer { Name = "test" };
            lyrS.Features.Add(featureS);
            vtS.Layers.Add(lyrS);

            using (var ms = new MemoryStream())
            {
                if (string.IsNullOrEmpty(idAttributeName))
                {
                    //No ID property specified when writing.
                    vtS.Write(ms);
                }
                else
                {
                    vtS.Write(ms, 4096, idAttributeName);
                }
                ms.Position = 0;

                //Get the raw Mapbox Tile Feature Id value and compare to expected.
                var tile = ProtoBuf.Serializer.Deserialize<NetTopologySuite.IO.VectorTiles.Mapbox.Tile>(ms);
                var id = tile.Layers[0].Features[0].Id;

                //Verify the id values match.
                Assert.Equal(expectedId, id);

                //Now test the Mapbox reader.

                if (string.IsNullOrEmpty(idAttributeName))
                {
                    idAttributeName = "id";
                }

                //Clear the keys and the feature's tags from the vector tiles. This will remove attribute values from the Mapbox feature.
                //This ensures we are testing the Mapbox Features ID value, and not features attributes.
                tile.Layers[0].Keys.Clear();
                tile.Layers[0].Features[0].Tags.Clear();
                
                using (var ms2 = new MemoryStream())
                {
                    //Serialize the modified tile.
                    ProtoBuf.Serializer.Serialize<NetTopologySuite.IO.VectorTiles.Mapbox.Tile>(ms2, tile);
                    ms2.Position = 0;

                    //Read the tile. Specify the ID attribute name we want the ID value to be stored.
                    //By default the reader won't capture the ID value unless specified what attribute to store it in. 
                    //The reason for this is that all Mapbox features have a default ID of 0, and it may be undesirable to capture this. This also ensures backwords compatibility.
                    var vtD = new MapboxTileReader().Read(ms2, new VectorTiles.Tiles.Tile(0), idAttributeName);

                    //Get the first feature. The ID value should be captured in it's attributes.
                    var f = vtD.Layers[0].Features[0];

                    //Compare with the expected ID.
                    Assert.Equal(expectedId, f.Attributes[idAttributeName]);
                }
            }
        }
    }
}
