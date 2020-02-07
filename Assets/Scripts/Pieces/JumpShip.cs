using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpShip : Piece
{
    private const int MOVE_RANGE = 1;
    private const int ATTACK_RANGE = 1;

    public override void Initialize(Vector2Int gridPosition, Ownership ownership)
    {
        this.gridPosition = gridPosition;
        health = 2;
        attackPower = 2;
        state = State.Idle;
        ownerType = ownership;
        doesAutoattack = true;

        Dictionary<Vector2Int, int> moveDirections = new Dictionary<Vector2Int, int>();
        moveDirections.Add(new Vector2Int(-2, 1), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(-1, 2), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(1, 2), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(2, 1), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(2, -1), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(1, -2), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(-1, -2), MOVE_RANGE);
        moveDirections.Add(new Vector2Int(-2, -1), MOVE_RANGE);

        movementBehavior = new DiscreteMovement(this, moveDirections);

        Dictionary<Vector2Int, int> attackDirections = new Dictionary<Vector2Int, int>();
        attackDirections.Add(new Vector2Int(-1, 0), ATTACK_RANGE);
        attackDirections.Add(new Vector2Int(0, 1), ATTACK_RANGE);
        attackDirections.Add(new Vector2Int(1, 0), ATTACK_RANGE);
        attackDirections.Add(new Vector2Int(0, -1), ATTACK_RANGE);

        attackBehavior = new MultiTargetAttack(this, attackDirections);

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
