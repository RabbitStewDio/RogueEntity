Commands and Action Handling
============================

The RogueEntity library is a simulation system at heart. The entity system
periodically runs a set of systems (functions) over the entity set. 
This system does not readily adapt itself to polling inputs or handling
UI flows.

In the sample architecture, a game is split between an I/O handling frontend
program that displays the current state and accepts inputs from the user,
and a simulation backend that processes the completed inputs.

User Input
----------

A game collects user input by polling controllers or by providing a graphical
user interface with buttons and dialogs. 

User inputs are expressed as commands. A command is an atomic action that
contains all relevant information to complete the activity within the game
world. 

Those commands are submitted to the game via an input queue. Actions should 
be processed in the order in which they have been submitted.


Command Processor
-----------------

During each iteration of the simulation, the game system processes the submitted
commands and translates them into events or state in the entity system. 

In the current sample architecture commands are translated into action components,
which are processed by their relevant action processing systems.



