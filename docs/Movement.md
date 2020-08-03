Movement
========

This library provides facilities for the four common movement modes usually
found in Role-Playing Games. The default system assumes that each map contains
at least a ground and an item layer. For voxel based games, the ground layer 
could even be the item layer of the next lower level.

* Walking: Ground movement on top of solid materials. Requires that there are no items blocking the movement.
* Swimming: Movement through liquids. Requires that no items block the path and that the ground layer is liquid.
* Flying: Movement through air. Requires that there are no items blocking the movement.
* Ethereal: Passing through walls and solid items just like a ghost would.

Movement cost is based on an assumed global standard velocity. Actors and items contribute
modifiers to this velocity. Movement cost is defined as a percentage of the standard velocity
in a range of [0 to 500%]. Any movement with a cost larger than 500% is assumed blocked.

Item and environmental movement costs are precomputed to speed up pathfinding operations.

