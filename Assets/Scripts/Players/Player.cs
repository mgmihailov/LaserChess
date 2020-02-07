using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    protected Phase phase;


    public delegate void Victory(Player player);
    public event Victory OnVictory;


    protected enum Phase
    {
        Idle = 0,
        Move,
        Attack,
        WaitTurn
    }


    public abstract void Initialize();

    public abstract void Reset();

    public abstract void Play();

    public abstract bool HasFinishedTurn();

    public abstract void AddPiece(GameObject piece);

    public abstract void OnPieceActionCompleted(Piece piece, Piece.ActionType action);

    public abstract void OnPieceDestroyed(GameObject piece);

    public void BroadcastVictory()
    {
        OnVictory(this);
    }
}
