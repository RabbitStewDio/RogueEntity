# Maps

RogueEntity provides two separate map systems. All map systems provide
a 3D view organized in horizontal layers. The Z-Axis describes the height
layer while the X and Y coordinates the describe the position within the
layer.

For each map cell, multiple data layers can exist. This allows you to
specify more than one type of data for each cell. Typical role playing
games would define layers like "Floor", "Structures/Walls", "Items" and 
"Actors".

All provided map implementations are tiled maps without predefined map
boundaries. Maps can grow or shrink to accommodate the current data stored
within them.

A ```GridMap``` stores elements in a regular grid. Only one element can 
occupy a grid cell for each layer. Elements moving on a grid map will
appear to jump from grid cell to grid cell.

The ```ContinuousMap``` system allows elements to be placed at any coordinate
ignoring any cell boundaries. This allows entities to overlap and to move
smoothly between positions etc.

It is possible to use different map implementations for each map layer.
It is not possible to use multiple map implementation for the same map
layer.

## Map Layers

Map layers are enumeration values that are usually produced by a 
MapLayerRegistry. Up to 255 unique map layers can exist at any point in time. 
Each map layer has a byte-id that identifies the layer within the system. 

## Placing entities

To query, place, swap or remove items from the map RogueEntity provides
a ```IItemPlacementService```. This interface is suitable for querying
single items for a given map location. 

As grid maps cannot have more than one data element in each cell, it
might be necessary to query the map for suitable empty cells in a given
radius around a target location. Use an ```IItemPlacementLocationService```
to find cells that can accept a given item. This service will correctly
handle stacking for bulk items.

To search a map area for a given item or items with a given criteria,
you can use a ```ISpatialQueryLookup``` to perform efficient lookups
on the map data over reference entities.

Any entity that has a ```GridPositionTrait``` will be stored in a 
grid map. Entities that have a ```ContinousPositionTrait``` will use
a ContinuousMap. If the game does not manually define maps for used layers, 
the initialization system will declare maps for each entity layer 
encountered.

To access grid maps for scanning a map area, you can use a ```IGridMapContext```
to access the map data of any map layer as 2D array.

However, to correctly handle entity movement, stacking and other special
cases (like checking whether a cell is actually occupied before writing
data) it is recommended to use the ```IItemPlacementService``` mentioned
above.