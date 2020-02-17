# LaserChess
A chess-like game with a laser twist created with Unity3D

## Rules

* The game is played on an 8x8 grid. 
* It is a 2 player game, with a human player and an AI player.
* Each player takes turns, starting with the human player.
* During a turn, a player may move any or all of his pieces.
* The AI pieces must follow certain behaviour rules which normally require them to move.

### The pieces

* Each piece exclusively occupies a single space on the grid.
* Pieces may not move through other pieces, unless it uses the knight move as in Chess.
* Pieces block shooting.
* Each piece has hit points (the amount of damage it can take before it is destroyed and removed from the grid).
* Each piece has an attack power, which is the amount of hitpoints taken from a target enemy piece in an attack.
* Each piece can move and then attack.

### The Human pieces

#### Grunt

* Moves 1 space orthogonally
* Shoots once, diagonally at any range.
* Attack power: 1
* Hitpoints: 2

#### Jumpship

* Moves like the knight in Chess
* Attacks all enemy pieces in the 4 orthogonally adjacent spaces simultaneously.
* Attack power: 2
* Hitpoints: 2

#### Tank

* Moves like the Queen in chess, up to a maximum of 3 spaces.
* Shoots once, orthogonally at any range.
* Attack power: 2
* Hitpoints: 4

### The AI pieces

#### Drone

* Moves forward 1 space from its side of the board (like a pawn, but never moves diagonally).
* Shoots once, diagonally at any range.
* Attack power: 1
* Hitpoints: 2

#### Behaviour:

* Drones move before any other AI piece.
* They must all move and attack if possible.
* They must shoot at a target if possible after attempting to move

#### Dreadnought

* Moves 1 space in any direction.
* Attacks all adjacent enemy units.
* Attack power: 2.
* Hitpoints: 5

#### Behaviour:

* Dreadnoughts move after all drones have moved.
* It must move 1 space, if possible, and must move towards the nearest enemy unit. It must try to  attack after moving.

#### Command Unit

* The Command Unit must move after Dreadnoughts have moved.
* It can only move 1space in two possible directions parallel to the AIs side of the board (i.e. it stays  the same distance from the enemy side of the board).
* It cannot shoot or attack.
* Attack power: 0
* Hitpoints: 5

#### Behaviour:

* It must avoid getting hit, if possible, so it must make the best move out of the three options available  (move one way, move the other way, or stay still).

### Victory determination

* The human player wins if all the Command Units are destroyed.
* The AI player wins if all human units are killed, or one of the drones reaches the 8th row.

## Folder structure

Assets/Scripts - Contains all the gameplay logic for the LaserChess game.
Assets/Prefabs - Contains mostly the different pieces as Prefabs, plus each piece's UI, with the buttons and the HP / AP info.
Assets/Sprites - Contains the icons used for the different buttons from the piece UI.
Assets/Materials - Contains the materials for each piece.
Assets/Scenes - Contains the single scene (so far) of the game.

## Legend

* Green sphere - Grunt
* Purple sphere - Jumpship
* Light-blue sphere - Tank
* Dark-blue sphere - Drone
* Orange sphere - Dreadnought
* Golden sphere - Command Unit

## Software used

* Unity 2019.3.0f6
* Visual Studio 2019 Community

## Ways to improve the game

- The code could be a bit more organized and data - more encapsulated.
- The AI Player's decision making could be improved:
  - The CommandUnit could start using MinMax algorithm to choose a move. Currently it just chooses a path where there's no direct threat from a piece controlled by the opponent.
- The AI Player's difficulty could also be made to scale. It could use a MinMax algorithm for all of it's moves with a different depth for each difficulty level.
- Additional user interface could be added:
  - A timer indicating the time elapsed since the match started.
  - A scoreboard keeping track of the result.

## Ways to extend the game

- More pieces could be added.
- Any player could play with any type of piece (not a fixed set of them).
- Players could choose the starting configuration of their pieces.
- Additional new mechanics could be added (i.e. taking control of enemy pieces, banishing an enemy piece from play for a few turns, etc.)
- Different boards sizes of boards could be added (i.e. a 16x16 board or a board in the shape of a triangle, with equal sides).
- Boards could provide terrain bonuses (i.e. there could be squares that provide jumpships +1 attack power when attacking from them).
