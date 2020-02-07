using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Piece : MonoBehaviour
{
    public PieceUI pieceUi;

    protected Vector2Int gridPosition;

    protected int health;

    protected int attackPower;

    protected Ownership ownerType;

    protected State state;

    protected MovementBehavior movementBehavior;

    protected AttackBehavior attackBehavior;

    protected bool canAttack = true;

    protected bool doesAutoattack = false;


    public delegate void ActionCompleted(Piece piece, ActionType action);
    public event ActionCompleted OnActionCompleted;

    public delegate void Destroyed(GameObject piece);
    public event Destroyed OnDestroyed;

    public delegate void Attack(Piece piece, Vector2Int targetDestination);
    public static event Attack OnAttack;


    public enum State
    {
        Idle = 0,
        Moving,
        Moved,
        Attacking,
        Done
    }

    public enum Ownership
    {
        Human = 0x1,
        AI = 0x2,

        // Filters used when we care only for
        // pieces with specific ownership
        FilterHumanPieces = Human,
        FilterAIPieces = AI,
        FilterAllPieces = FilterHumanPieces | FilterAIPieces,
    }

    public enum ActionType
    {
        Move = 0,
        Attack
    }


    public Ownership OwnerType
    {
        get
        {
            return ownerType;
        }
    }

    public bool AutoAttack
    {
        get
        {
            return doesAutoattack;
        }
    }

    public int AttackPower
    {
        get
        {
            return attackPower;
        }
    }


    public abstract void Initialize(Vector2Int gridPosition, Ownership ownership);

    public bool CanMove()
    {
        List<List<Vector2Int>> possibleMoves = GetPossiblePaths();
        Board.Instance.FilterPaths(possibleMoves, Piece.Ownership.FilterHumanPieces);
        return possibleMoves.Count != 0 && state == State.Idle;
    }

    public List<List<Vector2Int>> GetPossiblePaths()
    {
        return movementBehavior.GetPossiblePaths();
    }

    public void SetMoveDest(Vector2Int destination)
    {
        if (state == State.Idle)
        {
            movementBehavior.SetMoveDest(destination);
            SetState(State.Moving);
        }
    }

    protected void PerformMove()
    {
        if (state == State.Moving && movementBehavior.PerformMove())
        {
            SetState(State.Moved);
            if (!canAttack)
            {
                SetState(State.Done);
            }
            OnActionCompleted(this, ActionType.Move);
        }
    }

    public bool CanAttack()
    {
        if (!this.canAttack)
        {
            return false;
        }

        List<List<Vector2Int>> possibleAttacks = GetPossibleAttacks();
        Board.Instance.FilterTargets(possibleAttacks, Piece.Ownership.FilterAllPieces ^ ownerType);
        bool canAttack = state == State.Idle || state == State.Moved;
        canAttack = canAttack && possibleAttacks.Count != 0;
        return canAttack;
    }

    public List<List<Vector2Int>> GetPossibleAttacks()
    {
        return attackBehavior.GetPossibleAttacks();
    }

    public void SetAttackDests(List<Vector2Int> destinations)
    {
        if (state == State.Idle ||
            state == State.Moved)
        {
            attackBehavior.SetAttackDests(destinations);
            SetState(State.Attacking);
        }
    }

    protected void PerformAttack()
    {
        if (state == State.Attacking && attackBehavior.PerformAttack())
        {
            SetState(State.Done);
            OnActionCompleted(this, ActionType.Attack);
        }
    }

    public Vector2Int GetGridPos()
    {
        return gridPosition;
    }

    public void SetGridPos(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public State GetState()
    {
        return state;
    }

    public void SetState(State newState)
    {
        switch (newState)
        {
            case State.Idle:
                if (state == State.Done)
                {
                    state = newState;
                }
                break;
            case State.Moving:
                if (state == State.Idle)
                {
                    state = newState;
                }
                break;
            case State.Moved:
                if (state == State.Moving)
                {
                    state = newState;
                }
                break;
            case State.Attacking:
                if (state == State.Idle ||
                    state == State.Moved)
                {
                    state = newState;
                }
                break;
            case State.Done:
                if (state == State.Idle ||
                    state == State.Attacking ||
                    state == State.Moved)
                {
                    state = newState;
                }
                break;
        }
    }

    public void SkipAction()
    {
        switch (state)
        {
            case State.Idle:
                {
                    if (canAttack)
                    {
                        state = State.Moved;
                    }
                    else
                    {
                        state = State.Done;
                    }
                }
                break;
            case State.Moved:
                {
                    state = State.Done;
                }
                break;
        }
    }

    public void ResetState()
    {
        state = State.Idle;
    }

    public void ReceiveDamage(int dmg)
    {
        health = System.Math.Max(0, health - dmg);
        pieceUi.SetHealthInfo(health);
        if (health == 0)
        {
            OnDestroyed(gameObject);
            Destroy(gameObject);
        }
    }

    public void BroadcastAttack(Vector2Int targetDestination)
    {
        OnAttack(this, targetDestination);
    }

    public abstract class MovementBehavior
    {
        // Movement speed in units per second.
        public float moveSpeed = 20.0f;

        // The lenght of the journey from starting point to end point
        protected float journeyLength;

        // Start time of the movement in seconds
        protected float moveStartTime;

        // Keys are the possible directions in which a piece can move.
        // The values are the maximum lengths of the paths it can move
        // along in each direction.
        protected Dictionary<Vector2Int, int> moveDirections;

        protected List<Vector3> worldDestinations;

        protected Vector3 previousWorldPosition;

        protected Vector2Int moveDestination;

        protected Vector2Int lastIdlePosition;

        protected Piece piece;


        public MovementBehavior(Piece piece)
        {
            this.piece = piece;
        }


        public virtual List<List<Vector2Int>> GetPossiblePaths()
        {
            List<List<Vector2Int>> paths = new List<List<Vector2Int>>(moveDirections.Count);
            foreach (var pair in moveDirections)
            {
                Vector2Int pos = piece.gridPosition;
                paths.Add(new List<Vector2Int>(pair.Value));
                for (int i = 0; i < pair.Value; ++i)
                {
                    pos += pair.Key;
                    if (pos.x >= 0 && pos.x < Board.Columns && pos.y >= 0 && pos.y < Board.Rows)
                    {
                        paths[paths.Count - 1].Add(pos);
                    }
                }
            }
            paths.RemoveAll(x => x.Count == 0);
            return paths;
        }

        public abstract void SetMoveDest(Vector2Int destination);

        public abstract bool PerformMove();
    }

    protected class ContinuousMovement : MovementBehavior
    {
        public ContinuousMovement(Piece piece, Dictionary<Vector2Int, int> directions)
            : base(piece)
        {
            lastIdlePosition = piece.gridPosition;
            moveDestination = piece.gridPosition;
            moveDirections = directions;
            previousWorldPosition = piece.gameObject.transform.position;
            worldDestinations = new List<Vector3>();

        }

        public override void SetMoveDest(Vector2Int destination)
        {
            previousWorldPosition = piece.gameObject.transform.position;
            moveDestination = destination;
            Vector3 worldDestination = Board.Instance.GridToWorldPosition(destination);
            worldDestination.y = piece.gameObject.transform.position.y;
            worldDestinations.Add(worldDestination);
            moveStartTime = Time.time;
            journeyLength = Vector3.Distance(piece.transform.position, worldDestinations[0]);
        }

        public override bool PerformMove()
        {
            float distCovered = (Time.time - moveStartTime) * moveSpeed;
            bool isAtDestination = false;
            if (distCovered < journeyLength)
            {
                float fractionOfJourney = distCovered / journeyLength;
                piece.gameObject.transform.position = Vector3.Lerp(previousWorldPosition, worldDestinations[0], fractionOfJourney);
            }
            else
            {
                isAtDestination = true;
                piece.SetGridPos(moveDestination);
                Board.Instance.UpdatePiecePosition(lastIdlePosition, moveDestination);
                piece.gameObject.transform.position = worldDestinations[0];
                previousWorldPosition = piece.gameObject.transform.position;
                lastIdlePosition = moveDestination;
                worldDestinations.Clear();
            }

            return isAtDestination;
        }
    }

    protected class DiscreteMovement : MovementBehavior
    {
        private int currentWorldDest;

        public DiscreteMovement(Piece piece, Dictionary<Vector2Int, int> directions)
            : base(piece)
        {
            lastIdlePosition = piece.gridPosition;
            moveDestination = piece.gridPosition;
            moveDirections = directions;
            currentWorldDest = 0;
            worldDestinations = new List<Vector3>();
            previousWorldPosition = piece.gameObject.transform.position;
        }

        public override void SetMoveDest(Vector2Int destination)
        {
            moveDestination = destination;
            previousWorldPosition = piece.gameObject.transform.position;

            MeshRenderer mesh = piece.GetComponent<MeshRenderer>();
            float height = mesh.bounds.size.y;
            height += mesh.bounds.extents.y;

            // The piece should move upwards (up to 1.5 times it's full height).
            worldDestinations.Add(piece.gameObject.transform.position + new Vector3(0, height, 0));

            Vector3 worldDestination = Board.Instance.GridToWorldPosition(destination);
            // Then it should move to the destination, but still at the 1.5-its-height level.
            worldDestinations.Add(worldDestination + new Vector3(0, height, 0));

            // Finally, it lands on the board, at the destination
            worldDestinations.Add(worldDestination + new Vector3(0, piece.gameObject.transform.position.y, 0));

            moveStartTime = Time.time;
            journeyLength = Vector3.Distance(piece.gameObject.transform.position, worldDestinations[0]);
        }

        public override bool PerformMove()
        {
            float distCovered = (Time.time - moveStartTime) * moveSpeed;
            bool isAtDestination = false;
            if (distCovered < journeyLength)
            {
                float fractionOfJourney = distCovered / journeyLength;
                piece.gameObject.transform.position = Vector3.Lerp(previousWorldPosition
                    , worldDestinations[currentWorldDest]
                    , fractionOfJourney);
            }
            else if (currentWorldDest < worldDestinations.Count - 1)
            {
                ++currentWorldDest;
                moveStartTime = Time.time;
                journeyLength = Vector3.Distance(piece.gameObject.transform.position
                    , worldDestinations[currentWorldDest]);
                previousWorldPosition = piece.gameObject.transform.position;
            }
            else
            {
                isAtDestination = true;

                piece.SetGridPos(moveDestination);
                Board.Instance.UpdatePiecePosition(lastIdlePosition, moveDestination);

                piece.gameObject.transform.position = worldDestinations[currentWorldDest];
                previousWorldPosition = piece.gameObject.transform.position;
                lastIdlePosition = moveDestination;
                currentWorldDest = 0;
                worldDestinations.Clear();
            }

            return isAtDestination;
        }
    }

    public abstract class AttackBehavior
    {
        // Start time of the attack in seconds
        protected float attackStartTime;

        // Duration of the attack in seconds
        protected float attackDuration = 0.5f;

        protected Dictionary<Vector2Int, int> attackDirections;

        protected List<Vector2Int> attackDestinations;

        protected List<Vector3> worldDestinations;

        protected Piece piece;


        public AttackBehavior(Piece piece)
        {
            this.piece = piece;
        }


        public virtual List<List<Vector2Int>> GetPossibleAttacks()
        {
            List<List<Vector2Int>> paths = new List<List<Vector2Int>>(attackDirections.Count);
            foreach (var pair in attackDirections)
            {
                paths.Add(new List<Vector2Int>());
                Vector2Int pos = piece.gridPosition;
                for (int i = 0; i < pair.Value; ++i)
                {
                    pos = pos + pair.Key;
                    if (pos.x < 0 || pos.x >= Board.Rows || pos.y < 0 || pos.y >= Board.Columns)
                    {
                        break;
                    }

                    paths[paths.Count - 1].Add(pos);
                }
            }
            paths.RemoveAll(x => x.Count == 0);
            return paths;
        }

        public abstract void SetAttackDests(List<Vector2Int> destinations);

        public abstract bool PerformAttack();
    }

    protected class SingleTargetAttack : AttackBehavior
    {
        public SingleTargetAttack(Piece piece, Dictionary<Vector2Int, int> attackDirections)
            : base(piece)
        {
            this.attackDirections = attackDirections;
            attackDestinations = new List<Vector2Int>();
            worldDestinations = new List<Vector3>();
        }

        public override void SetAttackDests(List<Vector2Int> destinations)
        {
            attackDestinations = destinations;
            for (int i = 0; i < destinations.Count; ++i)
            {
                Vector3 worldDestination = Board.Instance.GridToWorldPosition(destinations[i]);
                worldDestination.y = piece.gameObject.transform.position.y;
                worldDestinations.Add(worldDestination);
            }
            attackStartTime = Time.time;
            LineRenderer lineRenderer = piece.GetComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.3f;
        }

        public override bool PerformAttack()
        {
            bool isDoneAttacking = false;
            float timeElapsed = Time.time - attackStartTime;
            LineRenderer lineRenderer = piece.GetComponent<LineRenderer>();
            if (timeElapsed < attackDuration)
            {
                Ray ray = new Ray(piece.gameObject.transform.position
                    , (worldDestinations[0] - piece.gameObject.transform.position).normalized);
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, worldDestinations[0]);
            }
            else
            {
                piece.BroadcastAttack(attackDestinations[0]);
                lineRenderer.enabled = false;
                isDoneAttacking = true;
                attackDestinations.Clear();
                worldDestinations.Clear();
            }

            return isDoneAttacking;
        }
    }

    protected class MultiTargetAttack : AttackBehavior
    {
        private int currentAttackDest;

        public MultiTargetAttack(Piece piece, Dictionary<Vector2Int, int> attackDirections)
            : base(piece)
        {
            this.attackDirections = attackDirections;
            currentAttackDest = 0;
            attackDestinations = new List<Vector2Int>();
            worldDestinations = new List<Vector3>();
        }

        public override void SetAttackDests(List<Vector2Int> destinations)
        {
            attackDestinations = destinations;
            for (int i = 0; i < destinations.Count; ++i)
            {
                Vector3 worldDestination = Board.Instance.GridToWorldPosition(destinations[i]);
                worldDestination.y = piece.gameObject.transform.position.y;
                worldDestinations.Add(worldDestination);
            }
            attackStartTime = Time.time;
            LineRenderer lineRenderer = piece.GetComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.5f;
        }

        public override bool PerformAttack()
        {
            bool isDoneAttacking = false;
            float timeElapsed = Time.time - attackStartTime;
            LineRenderer lineRenderer = piece.GetComponent<LineRenderer>();
            if (timeElapsed < attackDuration)
            {
                Ray ray = new Ray(piece.gameObject.transform.position
                    , (worldDestinations[currentAttackDest] - piece.gameObject.transform.position).normalized);
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, worldDestinations[currentAttackDest]);
            }
            else if (currentAttackDest < attackDestinations.Count - 1)
            {
                piece.BroadcastAttack(attackDestinations[currentAttackDest]);
                ++currentAttackDest;
                attackStartTime = Time.time;
            }
            else
            {
                piece.BroadcastAttack(attackDestinations[attackDestinations.Count - 1]);
                lineRenderer.enabled = false;
                isDoneAttacking = true;
                currentAttackDest = 0;
                worldDestinations.Clear();
                attackDestinations.Clear();
            }

            return isDoneAttacking;
        }
    }

    protected class NoAttack : AttackBehavior
    {
        public NoAttack(Piece piece) : base(piece) { }

        public override List<List<Vector2Int>> GetPossibleAttacks()
        {
            return new List<List<Vector2Int>>();
        }

        public override void SetAttackDests(List<Vector2Int> destinations) { }

        public override bool PerformAttack() { return true; }
    }
}
