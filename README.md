# NetTopologySuite.IO.VectorTiles

[![release](https://github.com/NetTopologySuite/NetTopologySuite.IO.VectorTiles/actions/workflows/release.yml/badge.svg)](https://github.com/NetTopologySuite/NetTopologySuite.IO.VectorTiles/actions/workflows/release.yml)

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/NetTopologySuite/NetTopologySuite.IO.VectorTiles/blob/develop/LICENSE.md)

A package that can be used to read or generate vector tiles using NTS.

## Getting started

A NuGet-package is hosted on [Github Packages](https://github.com/orgs/NetTopologySuite/packages?repo_name=NetTopologySuite.IO.VectorTiles) 

### Create a vector tile

The following shows how to create an individual vector tile, pass in a Feature from NTS, and write the tile as a file.

```csharp
//Define which tile you want to create.
var tileDefinition = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom);

//Create a vector tile instance and pass om the tile ID from the tile definition above.
var vt = new VectorTile { TileId = tileDefinition.id };

//Create one or more layers. Ideally one layer per dataset.
var lyr = new Layer { Name = "layer1" };

//Add your NTS feature(s) to the layer. Loop through all your features and add them to the tile.
lyr.Features.Add(myFeature);

//Add the layer to the vector tile. 
vt.Layers.Add(lyr);

//Output the tile to a stream. 
using (var fs = new FileStream(filePath, FileMode.Create))
{
    //Write the tile to the stream.
    vt.Write(fs, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent);
}
```

`MapboxTileWriter.Write` method takes two arguments controlling the output of features.

Argument | Default | Meaning
--- | --- | ---
minLinealExtent | 1 pixel |This applies to features with lineal geometries. If their scaled geometries for this tile have a bounding box with both edge lengths less than this value will not be written
minPolygonalExtent | 2 pixel<sup>2</sup> | This applies to polygonal geometries. If their scaled geometries for this tile have a bounding box with an area less than this value will not be exported.

Both constraints apply to parts of geometries, too. Holes in polygons or parts of multi-geometries not meeting the requirement will be omitted.

### Read a vector tile

The following shows how to read an individual vector tile and access the underlying NTS Features.

```csharp
//Create a MapboxTileReader.
var reader = new MapboxTileReader();

//Define which tile you want to read. You may be able to extract the x/y/zoom info from the file path of the tile. 
var tileDefinition = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom);

//Open a vector tile file as a stream.
using (var fs = new FileStream(filePath, FileMode.Open))
{
    //Read the vector tile.
    var vt = reader.Read(fs, tileDefinition);

    //Loop through each layer.
    foreach(var l in vt.Layers)
    {
        //Access the features of the layer and do something with them. 
        var features = l.Features;
    }
}
```

### Create tile ranges

More often than not you want to create a collection of tiles over a bounding box area and across multiple zoom levels. The `TileRange` class makes it easy to get the ID's of all tiles within an area for a specific zoom level. You can create a tile range for each zoom level.

```csharp
//Create a tile range for a bounding box and specific zoom level.
var tr = new NetTopologySuite.IO.VectorTiles.Tiles.TileRange(west, south, east, north, zoom);

//You can then enumerate over each tile within the range. 

//Enumerate using a loop. Parallel for loops can be used too, just be sure where ever you are reading your source data from can handle multiple parallel requests.
//Smaller datasets can be stored in memory, but for larger datasets that are stored in a database, you will want to throttle the number of parallel threads.
foreach(var tile in tr)
{
    //Read or write tiles.
}

//Alternatively make use the built in `EnumerateInCenterFirst` if you want to access tiles in a spiral pattern move out from the center. 
//This is useful if you are creating a dynamic rendering of the tiles.
var centerFirstRange = tr.EnumerateInCenterFirst();
foreach(var tile in centerFirstRange)
{
    //Read or write tiles.
}
```

### Feature IDs

GeoJSON features and features in vector tiles can have an ID property. This is for many things within an app, such as;

- Using the ID of an feature to query a database and retrieve addition details that shouldn't be stored in the tiles themselves. Generally you only want the data needed for visualization in the tiles.
- Using the ID in combination with the feature state capability in some SDKs to join external data to tiles, or create hover over effects.

NTS features however don't have an ID property. To address this, you can pass an ID value into the attributes table of a feature. The `MapboxTileWriter` class in this library by default will automatically look at the attribute table for an attribute called `id` that is an `integer` or `ulong` number. Alternatively, when writing a tile you can specify an alternative attribute name to retrieve an ID value from. The following shows how to pass an ID from an NTS feature into it's equivalent vector tile feature when writing a vector tile.

```csharp
//Create a feature.
var myFeature = new Feature(new Point(0, 0), new AttributesTable(new Dictionary<string, object>()
{
    { "id", 12345 },
    { "alternateId", 10101 },
    { "someOtherValue", "Hello World" }
}));

//Define which tile you want to create.
var tileDefinition = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom);

//Create a vector tile.
var vt = new VectorTile { TileId = tileDefinition.id };

//Create a layer. 
var lyr = new Layer { Name = "layer1" };

//Add your feature to the layer. Loop through all your features and add them to the tile.
lyr.Features.Add(myFeature);

//Add the layer to the vector tile. 
vt.Layers.Add(lyr);

//Output the tile to a stream. 
using (var fs = new FileStream(filePath, FileMode.Create))
{
    //Write the tile to the stream. This will automatically look for an "id" attribute that is a ulong or integer value as set the tile's feature ID to it.
    vt.Write(fs, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent);

    //Alternatively, pass in a different attribute name to have the vector tiles feature use that ID value.
    //vt.Write(fs, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent, 4096, "alternateId");
}
```

Similarly, the `MapboxTileReader` class will allows you to specify an attribute name to capture the ID value in the NTS feature. If you don't specify an attribute name, the ID value won't be captured in the attribute table. This is done for backwards compatibility. The following shows how to capture a vector tile features ID and store it using a custom attribute in the attribute table.

```csharp
//Create a MapboxTileReader.
var reader = new MapboxTileReader();

//Define which tile you want to read. You may be able to extract the x/y/zoom info from the file path of the tile. 
var tileDefinition = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom);

//Open a vector tile file as a stream.
using (var fs = new FileStream(filePath, FileMode.Open))
{
    //Read the vector tile.
    //By default ID values won't be extracted from the vector tile feature, by passing in the name of an attribute to store the ID in, the ID will be added to the attribute table. 
    //You can give this any name. 
    var vt = reader.Read(fs, tileDefinition, "id");

    //Loop through each layer.
    foreach(var l in vt.Layers)
    {
        //Access the features of the layer and do something with them. 
        var features = l.Features;
    }
}
```
