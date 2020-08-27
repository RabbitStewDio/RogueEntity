Module Initialization phases:

            < 10_000 : Entity Registrations
    -10_000 -      0 : One Time Initialization
          0 - 10_000 : Preparation Events
     10_000 - 19_999 : Player/Creature action planing
     20_000 - 29_999 : Player/Creature action execution
     30_000 - 39_999 : Reserved.
     40_000 - 49_999 : Apply Status Updates
     50_000 - 59_999 : Sense Map and Light calculations
           > 100_000 : System Cleanup Events

