Module Initialization phases:

            < 10_000 : Entity Registrations
    -10_000 -      0 : One Time Initialization
          0 - 10_000 : Preparation Events
     10_000 - 19_999 : Player/Creature action planing
     20_000 - 29_999 : Player/Creature action execution
     30_000 - 39_999 : Reserved as extension point.
     40_000 - 49_999 : Apply Status Updates
     50_000 - 59_999 : Sense Map and Light calculations
     60_000 - 99_999 : Reserved as extension point.
           > 100_000 : System Cleanup Events



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