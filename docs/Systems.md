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
