# Entity System Reference

## System initialization

A game is a collection of entity types and their associated behaviours.
Entity types are identified by their 'ItemDeclarationId'. An item declaration
is equivalent to a class in object oriented programming and describes the
potential behaviours a given entity supports. 

Behaviours are registered as traits on each item declaraion. Traits can
either be stateless (are independent of the entity instance) or stateful.

The RogueEntity system supports two fundamental entity types. ReferenceEntities
are stateful objects that are stored in an EntityRegistry. Each ReferenceEntity
is uniquely identified by its EntityKey. Only reference entities can be used
in entity systems.

Reference entities are used for actors and items that require complex behaviours
or that can exist only once in the game world. All actors must be ReferenceEntities.

A BulkEntity is a stateless entity and is not uniquely identified in the system.
For BulkEntities the EntityKey encodes the type information and reserves 16 bit
of shared state data to be used by entity traits to encode state information.
BulkEntities exist solely within the map or other entities. 

BulkEntities with a Stackable trait can merge into larger stacks. This way
a dragon's thousands of coins can form an orderly pile of a single entity 
encoding a item count.

BulkEntities should be used when the exact instance of an item is not important
and where many of such entities exist. Bulk entities should be used for modelling
gold coins and other commodity items. 

## System Execution Order

Module Initialization phases:

     before    -10_000 : Entity Registrations
    -10_000 to       0 : One Time Initialization
          0 to  10_000 : Preparation Events
     10_000 to  19_999 : Player/Creature action planing
     20_000 to  29_999 : Player/Creature action execution
     30_000 to  34_999 : Reserved as extension point.
     35_000 to  39_999 : Apply Status Updates
     40_000 to  44_999 : Reserved as extension point.
     45_000 to  49_999 : Content/Map Processing (Chunk loading etc.)
     50_000 to  59_999 : Sense Map and Light calculations
     60_000 to  79_999 : Reserved as extension point.
     80_000 to  84_999 : GUI and service updates
     85_000 to  99_999 : Reserved as extension point.
    100_000 and beyond : System Cleanup Events

Commands:

     20_000  - Clear movement Intent Markers (Late step)
     21_000  - Handle Movement Commands


Chunks/Maps:
 
     45_000  - Spawn Player Entities
     45_500  - Process Map-Loading commands, schedule map loading triggers
     47_500  * Mark observed map chunks as used.  
     48_000  * Unload chunks.                     
     48_500  - Load newly observed chunks.        
     49_000  - Collect possible spawn points
     49_500  - Place players in newly loaded chunks

Senses:

     50_000  - Prepare Sense Sources
     50_000  - Prepare Sense Receptors
     51_000  - Update Resistance Data

     55_000  - Collect Processable Sense Receptors; updates local position
     56_000  - Compute Sense Receptor Field-of-View
     57_000  - Collect Processable Sense Sources; updates local position
     57_500  - Collect Sense Sources For Receptor Processing; mark sources observed                         [if *not* using global Sense Map]
     57_500  - Collect Sense Sources For Global Sense Map; mark all sources observed                        [if using global Sense Map]
     58_000  - Process Sense Sources Field-of-View; defines sense source strength for each reachable cell
     58_500  - Copy Local Sense Source Data To Receptor                                                     [if *not* using global Sense Map]
     58_500  - Copy Sense Sources To Global Sense Map                                                       [if using global Sense Map]
     59_000  - Copy Global Sense Map Data To Receptor                                                       [if using global Sense Map]
     59_000  - Clean-up Sense Source Temporary Data
     59_500  - Clean-up Sense Receptor Temporary Data

Player Management:

     80_000  - Collect Players for PlayerManager (Late Step)
     81_000  - Collect Player Observers for PlayerManager (Late Step)
