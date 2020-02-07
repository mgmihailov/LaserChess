using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drone : Piece
{
    private const int MOVE_RANGE = 1;


    public override void Initialize(Vector2Int gridPosition, Ownership ownership)
    {
        this.gridPosition = gridPosition;
        health = 2;
        attackPower = 1;
        state = State.Idle;
        ownerType = ownership;

        Dictionary<Vector2Int, int> moveDirections = new Dictionary<Vector2Int, int>();
        moveDirections.Add(new Vector2Int(1, 0), MOVE_RANGE);

        movementBehavior = new ContinuousMovement(this, moveDirections);

        Dictionary<Vector2Int, int> attackDirections = new Dictionary<Vector2Int, int>();
        int attackRange = System.Math.Min(Board.Rows, Board.Columns);
        attackDirections.Add(new Vector2Int(-1, 1), attackRange);
        attackDirections.Add(new Vector2Int(1, -1), attackRange);
        attackDirections.Add(new Vector2Int(-1, -1), attackRange);
        attackDirections.Add(new Vector2Int(1, 1), attackRange);

        attackBehavior = new SingleTargetAttack(this, attackDirections);

        pieceUi.HealthInfo.text = $"HP: {health}";
        pieceUi.AttackPowerInfo.text = $"AP: {attackPower}";
    }

    // Update is called once per frame
    void Update()
    {
        PerformMove();
        PerformAttack();
    }
}
