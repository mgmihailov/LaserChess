using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightData
{
    public HighlightData(Vector2Int gridPos, Color highlightColor, Color originalColor)
    {
        this.gridPos = gridPos;
        this.highlightColor = highlightColor;
        this.originalColor = originalColor;
    }

    public Vector2Int gridPos;
    public Color highlightColor;
    public Color originalColor;
}

public class Board : MonoBehaviour
{
    public static Board Instance;

    public static int Rows = 8;

    public static int Columns = 8;

    private List<Square> squares;

    // All the currently highlighted squares of the board.
    private List<HighlightData> highlights;

    public void Initialize()
    {
        if (!Instance)
        {
            Instance = this;
        }

        squares = new List<Square>();
        highlights = new List<HighlightData>();

        for (int i = 0; i < Rows * Columns; ++i)
        {
            Square square = transform.GetChild(i).GetComponent<Square>();
            square.GridPosition = new Vector2Int(i / Columns, i % Columns);
            square.Piece = null;
            squares.Add(square);
        }
    }

    public void Reset()
    {
        foreach (var square in squares)
        {
            if (square.Piece)
            {
                Destroy(square.Piece.gameObject);
            }
        }
        highlights.Clear();
    }

    public void AddPiece(GameObject piece, Vector2Int pos)
    {
        squares[pos.x * Columns + pos.y].Piece = piece.GetComponent<Piece>();
    }

    public void RemovePiece(Vector2Int pos)
    {
        squares[pos.x * Columns + pos.y].Piece = null;
    }

    public Square GetSquare(Vector2Int pos)
    {
        return squares[pos.x * Columns + pos.y];
    }

    public void UpdatePiecePosition(Vector2Int from, Vector2Int to)
    {
        squares[to.x * Columns + to.y].Piece = squares[from.x * Columns + from.y].Piece;
        squares[from.x * Columns + from.y].Piece = null;
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return transform.GetChild(gridPosition.x * Columns + gridPosition.y).position;
    }

    public Dictionary<Vector2Int, Square> GetAllPieces(Piece.Ownership filter)
    {
        Dictionary<Vector2Int, Square> allPieces = new Dictionary<Vector2Int, Square>();
        foreach (var square in squares)
        {
            Piece piece = square.Piece;
            if (piece && (piece.OwnerType & filter) != 0)
            {
                allPieces.Add(square.GridPosition, square);
            }
        }
        return allPieces;
    }

    public void HighlightSquares(List<HighlightData> squares)
    {
        foreach (HighlightData data in squares)
        {
            int idx = highlights.FindIndex(x => x.gridPos == data.gridPos);
            if (idx == -1)
            {
                highlights.Add(data);
            }
            else
            {
                highlights[idx].highlightColor = Color.Lerp(highlights[idx].highlightColor, data.highlightColor, 0.5f);
            }
        }
    }

    public void RemoveHighlightedSquares()
    {
        foreach (HighlightData data in highlights)
        {
            Transform square = transform.GetChild(data.gridPos.x * Columns + data.gridPos.y);
            Material material = square.GetComponent<MeshRenderer>().material;
            material.SetColor("_Color", data.originalColor);
        }
        highlights.Clear();
    }

    public void FilterPaths(List<List<Vector2Int>> paths, Piece.Ownership filter)
    {
        for (int i = paths.Count - 1; i >= 0; --i)
        {
            for (int j = 0; j < paths[i].Count; ++j)
            {
                int idx = paths[i][j].x * Columns + paths[i][j].y;
                if (squares[idx].Piece)
                {
                    Piece piece = squares[idx].Piece;
                    if ((piece.OwnerType & filter) != 0)
                    {
                        // If this is the beginning of the path, we should remove the entire path
                        // straight away.
                        if (j == 0)
                        {
                            paths.RemoveAt(i);
                        }
                        else
                        {
                            // Otherwise trim the rest of the path.
                            paths[i].RemoveRange(j, paths[i].Count - j);
                        }
                        break;
                    }
                }
            }
        }
        paths.RemoveAll(x => x.Count == 0);
    }

    public void FilterTargets(List<List<Vector2Int>> paths, Piece.Ownership filter)
    {
        for (int i = paths.Count - 1; i >= 0; --i)
        {
            bool shouldDropPath = false;
            for (int j = 0; j < paths[i].Count;)
            {
                int idx = paths[i][j].x * Columns + paths[i][j].y;
                Piece piece = squares[idx].Piece;
                // If the first piece we encounter along the attack path is
                // owned by the current player, she can't shoot through it
                // so we drop this path entirely.
                if (piece && (piece.OwnerType & filter) == 0)
                {
                    shouldDropPath = true;
                    break;
                }

                // If the first piece we encounter along the attack path is
                // owned by the opponent, leave only this square and
                // remove the others from the path.
                if (piece && (piece.OwnerType & filter) != 0)
                {
                    paths[i].RemoveAll(x => x != paths[i][j]);
                    shouldDropPath = false;
                    break;
                }

                ++j;
                shouldDropPath = j == paths[i].Count;
            }

            if (shouldDropPath)
            {
                paths.RemoveAt(i);
            }
        }
        paths.RemoveAll(x => x.Count == 0);
    }

    // Return the first encountered piece conforming to "filter" that stands on "path".
    public Piece IsPathBlocked(List<Vector2Int> path, Piece.Ownership filter)
    {
        foreach (var pos in path)
        {
            int idx = pos.x * Columns + pos.y;
            if (squares[idx].Piece)
            {
                Piece piece = squares[idx].Piece;
                if ((piece.OwnerType & filter) != 0)
                {
                    return piece;
                }
            }
        }

        return null;
    }

    public float GetMinDistBetweenSquares()
    {
        return Vector3.Distance(transform.GetChild(0).position
            , transform.GetChild(1 * Columns + 1).position);
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < highlights.Count; ++i)
        {
            Transform square = transform.GetChild(highlights[i].gridPos.x * Columns + highlights[i].gridPos.y);
            MeshRenderer renderer = square.GetComponent<MeshRenderer>();
            renderer.material.SetColor("_Color", highlights[i].highlightColor);
        }
    }
}
