using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    // The time in seconds that the AI player will
    // take between moves. This way the AI player's
    // actions are more visible for the human player.
    public float MoveDelay;

    private float lastCommandEndTime;

    private List<GameObject> drones = new List<GameObject>();

    private List<GameObject> dreadnoughts = new List<GameObject>();

    private List<GameObject> commandUnits = new List<GameObject>();

    private Queue<Command> pieceCommands = new Queue<Command>();

    public override void Initialize()
    {
        phase = Phase.WaitTurn;
    }

    public override void Reset()
    {
        drones.Clear();
        dreadnoughts.Clear();
        commandUnits.Clear();
        pieceCommands.Clear();
    }

    public override void Play()
    {
        phase = Phase.Idle;
        lastCommandEndTime = Time.time;
        ResetAllPieces();
        ScheduleDronesActions();
        ScheduleDreadnoughtsActions();
        ScheduleCommandUnitsActions();
    }

    public override bool HasFinishedTurn()
    {
        bool hasFinishedTurn = true;

        foreach (var commandUnit in commandUnits)
        {
            if (commandUnit.GetComponent<Piece>().GetState() != Piece.State.Done)
            {
                hasFinishedTurn = false;
                break;
            }
        }

        if (hasFinishedTurn)
        {
            phase = Phase.WaitTurn;
        }
        return hasFinishedTurn;
    }

    public override void AddPiece(GameObject piece)
    {
        Piece pieceComp = piece.GetComponent<Piece>();
        pieceComp.OnDestroyed += OnPieceDestroyed;
        pieceComp.OnActionCompleted += OnPieceActionCompleted;
        string pieceType = piece.GetComponent<Piece>().GetType().Name;
        switch (pieceType)
        {
            case "Drone":
                {
                    drones.Add(piece);
                }
                break;
            case "Dreadnought":
                {
                    dreadnoughts.Add(piece);
                }
                break;
            case "CommandUnit":
                {
                    commandUnits.Add(piece);
                }
                break;
        }
    }

    public override void OnPieceActionCompleted(Piece piece, Piece.ActionType action)
    {
        lastCommandEndTime = Time.time;
        phase = Phase.Idle;

        // Check the if the victory condition is true:
        // - Either a drone owned by the AI player has
        //   reached the last row of the board.
        // - Or the AI player has destroyed all of the
        //   Human player's pieces
        bool hasWon = false;
        if (action == Piece.ActionType.Move)
        {
            if (piece.GetType().Name == "Drone" &&
            piece.GetGridPos().x == Board.Rows - 1)
            {
                hasWon = true;
            }
        }
        else if (action == Piece.ActionType.Attack)
        {
            Dictionary<Vector2Int, Square> humanPieces = Board.Instance.GetAllPieces(Piece.Ownership.Human);

            if (humanPieces.Count == 0)
            {
                hasWon = true;
            }
        }

        if (hasWon)
        {
            phase = Phase.WaitTurn;
            BroadcastVictory();
        }
    }

    public override void OnPieceDestroyed(GameObject piece)
    {
        Piece pieceComp = piece.GetComponent<Piece>();
        Board.Instance.RemovePiece(pieceComp.GetGridPos());
        string pieceType = pieceComp.GetType().Name;
        switch(pieceType)
        {
            case "Drone":
                {
                    drones.Remove(piece);
                }
                break;
            case "Dreadnought":
                {
                    dreadnoughts.Remove(piece);
                }
                break;
            case "CommandUnit":
                {
                    commandUnits.Remove(piece);
                }
                break;
        }
    }

    private void ResetAllPieces()
    {
        foreach (var drone in drones)
        {
            drone.GetComponent<Piece>().ResetState();
        }

        foreach (var dreadnought in dreadnoughts)
        {
            dreadnought.GetComponent<Piece>().ResetState();
        }

        foreach (var commandUnit in commandUnits)
        {
            commandUnit.GetComponent<Piece>().ResetState();
        }
    }

    private void ScheduleDronesActions()
    {
        foreach (var drone in drones)
        {
            pieceCommands.Enqueue(new MoveDroneCommand(this, drone));
            pieceCommands.Enqueue(new AttackWithDroneCommand(this, drone));
        }
    }

    private void ScheduleDreadnoughtsActions()
    {
        foreach (var dreadnought in dreadnoughts)
        {
            pieceCommands.Enqueue(new MoveDreadnoughtCommand(this, dreadnought));
            pieceCommands.Enqueue(new AttackWithDreadnoughtCommand(this, dreadnought));
        }
    }

    private void ScheduleCommandUnitsActions()
    {
        foreach (var commandUnit in commandUnits)
        {
            pieceCommands.Enqueue(new MoveCommandUnitCommand(this, commandUnit));
        }
    }

    private Square FindClosestFreeSquare(Piece piece)
    {
        Dictionary<Vector2Int, Square> humanPieces = Board.Instance.GetAllPieces(Piece.Ownership.FilterHumanPieces);
        // Find the closest piece of the opponent.
        int shortestDistance = int.MaxValue;
        Square closestPieceSquare = null;
        foreach (var square in humanPieces)
        {
            int distance = (int)Vector3.Distance(piece.gameObject.transform.position
                , square.Value.Piece.gameObject.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPieceSquare = square.Value;
            }
        }

        // The closest piece controlled by the opponent is on a neighbouring
        // square - no need to move.
        if (shortestDistance == 1)
        {
            return null;
        }

        // Now find a neighbor of the closest square with an opponent piece,
        // which is the closest to the attacking piece.
        shortestDistance = int.MaxValue;
        int startRow = Math.Max(0, closestPieceSquare.GridPosition.x - 1);
        int endRow = Math.Min(Board.Rows - 1, closestPieceSquare.GridPosition.x + 1);
        int startCol = Math.Max(0, closestPieceSquare.GridPosition.y - 1);
        int endCol = Math.Min(Board.Columns - 1, closestPieceSquare.GridPosition.y + 1);
        for (int i = startRow; i <= endRow; ++i)
        {
            for (int j = startCol; j <= endCol; ++j)
            {
                if (i == closestPieceSquare.GridPosition.x &&
                    j == closestPieceSquare.GridPosition.y)
                {
                    continue;
                }

                Square neighbor = Board.Instance.GetSquare(new Vector2Int(i, j));
                int distance = (int)Vector3.Distance(piece.gameObject.transform.position
                    , neighbor.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPieceSquare = neighbor;
                }
            }
        }

        return closestPieceSquare;
    }

    private int AStarHeuristic(Vector2Int start, Vector2Int current)
    {
        return Math.Max(Math.Abs(current.x - start.x), Math.Abs(current.y - start.y));
    }

    private Vector2Int GetFirstStep(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        Vector2Int result = current;

        while (cameFrom.ContainsKey(current))
        {
            result = current;
            current = cameFrom[current];
        }
        return result;
    }

    private Vector2Int FindShortestPath(Vector2Int start, Vector2Int goal)
    {

        // Get all the squares which are occupied by a piece as they are not considered passable.
        Dictionary<Vector2Int, Square> obstacles = Board.Instance.GetAllPieces(Piece.Ownership.FilterAllPieces);

        // For square n, cameFrom[n] is the square immediately preceding it on the cheapest path from start to n currently known.
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        // The set of discovered squares that may need to be (re-)expanded.
        // Initially, only the start square is known.
        List<Node> openSet = new List<Node>();

        // For node n, n.gScore is the cost of the cheapest path from start to n currently known.
        // For node n, n.fScore = n.gScore + heuristic(start, n).
        List<Node> nodes = new List<Node>();
        for (int i = 0; i < Board.Rows; ++i)
        {
            for (int j = 0; j < Board.Columns; ++j)
            {
                Node node = new Node(new Vector2Int(i, j));
                if (i == start.x && j == start.y)
                {
                    node.gScore = 0;
                    node.fScore = AStarHeuristic(start, goal);
                    openSet.Add(node);
                }
                nodes.Add(node);
            }
        }

        while (openSet.Count != 0)
        {
            // Find the minimum fScore of all elements in the open set.
            var lowestFScore = openSet.Min(x => x.fScore);
            // Then, get the corresponding node that has it.
            Node current = openSet.First(x => x.fScore == lowestFScore);
            if (current.position == goal)
            {
                // Get the first step from the shortest path.
                // This is where we would like to move next.
                return GetFirstStep(cameFrom, current.position);
            }

            openSet.Remove(current);
            // Go through all adjacent grid positions
            int startRow = Math.Max(current.position.x - 1, 0);
            int startCol = Math.Max(current.position.y - 1, 0);
            int endRow = Math.Min(current.position.x + 1, Board.Rows - 1);
            int endCol = Math.Min(current.position.y + 1, Board.Columns - 1);
            for (int i = startRow; i <= endRow; ++i)
            {
                for (int j = startCol; j <= endCol; ++j)
                {
                    Node neighbor = nodes[i * Board.Columns + j];
                    // Skip current square and any square that already has a piece on it.
                    if (obstacles.ContainsKey(neighbor.position))
                    {
                        continue;
                    }
                    // tentativeGScore is the distance from "start" to the neighbor through current.
                    float tentativeGScore = current.gScore + Vector2Int.Distance(current.position, neighbor.position);
                    if (tentativeGScore < neighbor.gScore)
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        cameFrom[neighbor.position] = current.position;
                        neighbor.gScore = tentativeGScore;
                        neighbor.fScore = neighbor.gScore + AStarHeuristic(neighbor.position, goal);
                        nodes[i * Board.Columns + j] = neighbor;
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // Open set is empty but "goal" was never reached.
        return new Vector2Int(-1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        // There's currently a pending move / attack - don't do anything until it's done.
        if (phase != Phase.Idle)
        {
            return;
        }

        // Move / attack commands should be executed sequentially because the player
        // uses the board's current state to determine the destination / targets.
        float timeSinceLastCommand = Time.time - lastCommandEndTime;
        if (pieceCommands.Count != 0 &&
            timeSinceLastCommand > MoveDelay)
        {
            Command cmd = pieceCommands.Dequeue();
            cmd.Execute();
        }
    }


    private struct Node
    {
        public Vector2Int position;

        public float fScore;

        public float gScore;


        public Node(Vector2Int position)
        {
            this.position = position;
            fScore = float.MaxValue;
            gScore = float.MaxValue;
        }
    }


    public abstract class Command
    {
        protected AIPlayer player;

        protected GameObject piece;

        public Command(AIPlayer player, GameObject piece)
        {
            this.player = player;
            this.piece = piece;
        }

        public abstract void Execute();
    }

    private class MoveDroneCommand : Command
    {
        public MoveDroneCommand(AIPlayer player, GameObject piece) : base(player, piece) { }


        public override void Execute()
        {
            Piece pieceComp = piece.GetComponent<Piece>();

            List<List<Vector2Int>> possibleMoves = pieceComp.GetPossiblePaths();
            Board.Instance.FilterPaths(possibleMoves, Piece.Ownership.FilterAllPieces);
            
            if (possibleMoves.Count != 0 && possibleMoves[0].Count != 0)
            {
                // Prevent player from doing anything while there's a pending move.
                player.phase = Phase.Move;

                pieceComp.SetMoveDest(possibleMoves[0][0]);
            }
            else
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
            }
        }
    }

    private class AttackWithDroneCommand : Command
    {
        public AttackWithDroneCommand(AIPlayer player, GameObject piece) : base(player, piece) { }


        public override void Execute()
        {
            Piece pieceComp = piece.GetComponent<Piece>();

            List<List<Vector2Int>> possibleAttacks = pieceComp.GetPossibleAttacks();
            Board.Instance.FilterTargets(possibleAttacks, Piece.Ownership.FilterHumanPieces);

            if (possibleAttacks.Count != 0)
            {
                List<Vector2Int> attackTargets = new List<Vector2Int>();
                attackTargets.Add(possibleAttacks[UnityEngine.Random.Range(0, possibleAttacks.Count)][0]);

                // Prevent player from doing anything while there's an attack in progress.
                player.phase = Phase.Attack;

                pieceComp.SetAttackDests(attackTargets);
            }
            else
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
            }
        }
    }

    private class MoveDreadnoughtCommand : Command
    {
        public MoveDreadnoughtCommand(AIPlayer player, GameObject piece) : base(player, piece) { }


        public override void Execute()
        {
            Piece pieceComp = piece.GetComponent<Piece>();

            Square closestPieceSquare = player.FindClosestFreeSquare(pieceComp);

            if (!closestPieceSquare)
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
                return;
            }

            // Find the first step along the shortest path from the
            // dreadnought to the closest piece of the opponent and move there.
            Vector2Int destination = player.FindShortestPath(pieceComp.GetGridPos(), closestPieceSquare.GridPosition);

            if (destination.x != -1 && destination.y != -1)
            {
                // Prevent player from doing anything while there's a pending move.
                player.phase = Phase.Move;

                pieceComp.SetMoveDest(destination);
            }
            else
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
            }
        }
    }

    private class AttackWithDreadnoughtCommand : Command
    {
        public AttackWithDreadnoughtCommand(AIPlayer player, GameObject piece) : base(player, piece) { }


        public override void Execute()
        {
            Piece pieceComp = piece.GetComponent<Piece>();

            Dictionary<Vector2Int, Square> humanPieces = Board.Instance.GetAllPieces(Piece.Ownership.FilterHumanPieces);
            Dictionary<Vector2Int, Square> aIPieces = Board.Instance.GetAllPieces(Piece.Ownership.FilterAIPieces);
            List<List<Vector2Int>> attackPaths = pieceComp.GetPossibleAttacks();
            for (int i = attackPaths.Count - 1; i >= 0; --i)
            {
                // Remove squares on which there is no opponent piece to attack
                // or which are already occupied by the a piece of the current player herself.
                if (!humanPieces.ContainsKey(attackPaths[i][0]) ||
                    aIPieces.ContainsKey(attackPaths[i][0]))
                {
                    attackPaths.RemoveAt(i);
                }
            }

            // Convert any reamining gridPositions to world positions and set them as attack
            // destinations for the dreadnought.
            if (attackPaths.Count != 0)
            {
                List<Vector2Int> attackDestinations = new List<Vector2Int>();
                for (int i = 0; i < attackPaths.Count; ++i)
                {
                    attackDestinations.Add(attackPaths[i][0]);
                }

                // Prevent player from doing anything while there's an attack in progress.
                player.phase = Phase.Attack;

                pieceComp.SetAttackDests(attackDestinations);
            }
            else
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
            }
        }
    }

    private class MoveCommandUnitCommand : Command
    {
        public MoveCommandUnitCommand(AIPlayer player, GameObject piece) : base(player, piece) { }


        public override void Execute()
        {
            Piece pieceComp = piece.GetComponent<Piece>();

            Dictionary<Vector2Int, Square> humanPieces = Board.Instance.GetAllPieces(Piece.Ownership.FilterHumanPieces);
            List<List<Vector2Int>> attackPaths = new List<List<Vector2Int>>();
            foreach (var piece in humanPieces)
            {
                Piece currentPieceComp = piece.Value.Piece;
                List<List<Vector2Int>> currentAttackPaths = currentPieceComp.GetPossibleAttacks();
                attackPaths.AddRange(currentAttackPaths);
            }

            // We need to filter the attack paths otherwise the command unit might deside
            // it's threatened when it actually isn't. For example, the command unit might be
            // on the attack path of an enemy unit but that attack path is blocked by another
            // piece, hence there's no real thread for the command unit.
            for (int i = attackPaths.Count - 1; i >= 0; --i)
            {
                Piece blockingPiece = Board.Instance.IsPathBlocked(attackPaths[i], Piece.Ownership.FilterAllPieces);
                if (blockingPiece && blockingPiece != piece)
                {
                    attackPaths.RemoveAt(i);
                }
            }

            List<List<Vector2Int>> possibleMoves = pieceComp.GetPossiblePaths();
            Board.Instance.FilterPaths(possibleMoves, Piece.Ownership.FilterAllPieces);
            foreach (var path in attackPaths)
            {
                for (int j = 0; j < possibleMoves.Count;)
                {
                    // Remove any possible moves that will put the
                    // command unit in harm's way.
                    if (path.Contains(possibleMoves[j][0]))
                    {
                        possibleMoves.RemoveAt(j);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }

            // If there are any moves left, just pick the first available.
            // If there aren't - the command unit just stays where it is.
            if (possibleMoves.Count != 0)
            {
                // Prevent player from doing anything while there's a pending move.
                player.phase = Phase.Move;

                pieceComp.SetMoveDest(possibleMoves[UnityEngine.Random.Range(0, possibleMoves.Count)][0]);
            }
            else
            {
                pieceComp.SkipAction();
                player.phase = Phase.Idle;
            }
        }
    }
}
