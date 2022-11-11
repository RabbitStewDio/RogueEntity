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


Mechanics
---------

Reference entities are movable if they declare a ``MovementCost`` trait. All movement has some
form of cost. Usually movement is either restrained by a maximum velocity (ie an actor
can move a certain distance at any turn, where turn represents a time unit) or a direct
energy cost (an actor spends movement points to move in the world).

An actors available movement points or energy is constrained by the terrain they
try to pass through. It is easier to travel on an paved road than in a swamp. Terrain
cost is represented as a cost modifier that can be applied to any entity in the world.
Both bulk and reference actors can define a movement cost modifier by declaring a 
``RelativeMovementCostModifier``. Generally this modifier will be applied to environment 
entities, like floor times, not moveable actors like monsters.

The movement system assumes that all tiles that are 'walkable' can be entered from
any tile. This allows movement to transition between movement modes. Imagine a walkable
tile next to a water tile. A character, that can both walk and swim, can transition from
the walking cell to the swiming cell and vice versa. In terms of pathfinding, the
algorithms only check inbound connections and ignore outbound movement connection.

Movement cost modifiers describe how the fixed movement cost (time if velocity, points
if movement points) is altered based on the physical properies of the tile. The movement
costs you define for an entity declare the cost modifier when entering a given tile. 

Thus: Moving from a paved road with a cost factor of ``1`` into a swamp (cost factor ``2``)
will spend 2 times the time or points (cost of swamp tile). Moving from the swamp tile to 
another swamp tile will also cost 2 times the time or points, while leavign the swamp via a 
road tile (of cost factor 1) will make the character move at a cost 1.

Traps that increase movement cost (ie. spider webs) affect the character when entering
the trap, but leave the character unimpeded when leaving the trap.

Movement costs and traversability are recorded separately for each movement mode.


Movement Planing
----------------

The RogueEntity library provides three modes of movement planing:

1. PathFinding:

   Attempts to navigate from a given position to another set of known positions.
   Each target position has the same relative importance. This will always find
   the nearest target.

2. GoalFinding: 

   Attempts to find a navigatable path to multiple targets. Each target can have
   a variable importance. Goal finding attempts to balance distance and goal
   importance and thus may select a target further away than the nearest target
   if the goal's importance outweights the movement cost.

3. GoalAvoidance:

   Attempts to move *away* from a given set of goals. Also known as flee maps. 
