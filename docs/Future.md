# Grid Placement

Change GridPositionTraits to use a placement strategy instead of hardcoding map modification rules.
Two strategies exist:

* SingleItemPlacementStrategy - Only one item exists in a cell at any given time
* StackedPlacementStrategy    - Items are stored via a single-linked list element TElement:= TPayload -> TElement

Need to change the map context interface to be able to handle multiple items per cell without
sacrificing performance.

# Senses

Implement sensory limits and sensory overload. People with bad senses (failing sight etc) cannot see in low 
intensity conditions. Could be solved with a `min-threashold` value. 

Strong signal sources blur out other senses and dominate the sensor readings (flash-bang grenades etc). 
Can be solved with a post-processing step that uses a local floodfill for any sense reading larger than 
`overload threshold`.

# Non-Grid Placements

Allow elements to be placed freely. Efficient queries requires some form of spatial indexing, similiar
to what is provided by physics engines. 