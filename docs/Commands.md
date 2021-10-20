Commands and Action Handling
============================

The RogueEntity library is a simulation system at heart. The entity system
periodically runs a set of systems (functions) over the entity set. 
This system does not readily adapt itself to polling inputs or handling
UI flows.

In the default system architecture, a game is split between an I/O handling 
frontend program that displays the current state and accepts inputs from the user,
and a simulation backend that processes the completed inputs once a valid 
input gesture is recognized.

User Input
----------

A game collects user input by polling controllers or by providing a graphical
user interface with buttons and dialogs. Simple inputs require just a 
single button press to be valid. More complex inputs, like using an item
from the inventory on the game map might require multiple UI interactions
to collect all parameters before the system can execute them.

User inputs are expressed as commands. A command is an atomic action that
contains all relevant information to complete the activity within the game
world. 

Commands are submitted to the game via an CommandService. Command objects are 
usually stored as components on an entity and processed by systems (like any
other in-game state).

