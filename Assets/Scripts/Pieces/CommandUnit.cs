using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandUnit : Piece
{
    public override void Initialize(Vector2Int gridPosition, Ownership ownership)
    {
        this.gridPosition = gridPosition;
        health = 5;
        attackPower = 0;
        state = State.Idle;
        ownerType = ownership;

        Dictionary<Vector2Int, int> moveDirections = new Dictionary<Vector2Int, int>();
        moveDirections.Add(new Vector2Int(0, -1), 1);
        moveDirections.Add(new Vector2Int(0, 1), 1);

        movementBehavior = new ContinuousMovement(this, moveDirections);

        attackBehavior = new NoAttack(this);
        canAttack = false;

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
