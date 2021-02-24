# Player Representation and starting a new game

## Player Structure

In RogueEntity several concepts represent the player configuration.

A player is the direct representation of a human or non-human actor that
controls some entities by sending commands and who receives status updates
from the game to a dedicated client UI.

A player profile is an optional data structure that hold any data associated with
a single player. A player profile is expected to be stored outside of a 
current game and to potentially survive a game. You usually record data like
scores, player names and profile (description, identifying information for
highscore lists or chats etc.)

A player observer is an optional entity that represents a player's attention in 
the game world. In dynamic worlds, observers should be used to mark areas that 
the player is active in. An observer can be equivalent to the the player's avatar, 
or can be a camera or other marker object that marks the player's current view(s).

