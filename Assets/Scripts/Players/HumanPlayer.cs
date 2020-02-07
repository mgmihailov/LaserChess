using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HumanPlayer : Player
{
    public UserInterface UserInterface;

    private Camera mainCamera;

    private GameObject selectedPiece;

    private List<GameObject> ownedPieces;

    private List<Vector2Int> availableMoveSquares;

    private List<Vector2Int> availableAttackSquares;

    private bool hasRequestedAction;


    public override void Initialize()
    {
        mainCamera = Camera.main;

        phase = Phase.WaitTurn;

        ownedPieces = new List<GameObject>();
        availableMoveSquares = new List<Vector2Int>();
        availableAttackSquares = new List<Vector2Int>();

        UserInterface.AddOnEndTurnButtonClickListener(OnUIEndTurnButtonClick);
    }

    public override void Reset()
    {
        ownedPieces.Clear();
        DeselectPiece();
    }

    public override void Play()
    {
        foreach (var piece in ownedPieces)
        {
            piece.GetComponent<Piece>().ResetState();
        }
        UserInterface.ShowEndTurnButton();
        phase = Phase.Idle;
    }

    public override bool HasFinishedTurn()
    {
        return phase == Phase.WaitTurn;
    }

    public override void AddPiece(GameObject piece)
    {
        Piece pieceComp = piece.GetComponent<Piece>();
        pieceComp.OnDestroyed += OnPieceDestroyed;
        pieceComp.OnActionCompleted += OnPieceActionCompleted;
        pieceComp.pieceUi.AddOnMoveListener(OnUIMoveButtonClicked);
        pieceComp.pieceUi.AddOnAttackListener(OnUIAttackButtonClicked);
        pieceComp.pieceUi.AddOnCancelActionListener(OnUICancelButtonClicked);
        pieceComp.pieceUi.SetEventCamera(mainCamera);
        ownedPieces.Add(piece);
    }
    public override void OnPieceActionCompleted(Piece piece, Piece.ActionType action)
    {
        phase = Phase.Idle;

        // Check if the victory condition for Human player
        // is true:
        // - There shouldn't be any command units left
        //   for the AI player
        if (action == Piece.ActionType.Attack)
        {
            Dictionary<Vector2Int, Square> aIPieces = Board.Instance.GetAllPieces(Piece.Ownership.AI);
            bool hasWon = true;
            foreach (var aIPiece in aIPieces)
            {
                if (aIPiece.Value.Piece.GetType().Name == "CommandUnit")
                {
                    hasWon = false;
                    break;
                }
            }

            if (hasWon)
            {
                phase = Phase.WaitTurn;
                BroadcastVictory();
            }
        }
    }

    public override void OnPieceDestroyed(GameObject piece)
    {
        Vector2Int gridPosition = piece.GetComponent<Piece>().GetGridPos();
        Board.Instance.RemovePiece(gridPosition);
        ownedPieces.Remove(piece);
    }

    public void OnUIMoveButtonClicked()
    {
        phase = Phase.Move;
        ShowPieceMovePaths();
        hasRequestedAction = true;
    }

    public void OnUIAttackButtonClicked()
    {
        phase = Phase.Attack;
        ShowPieceAttackTargets();
        if (selectedPiece.GetComponent<Piece>().AutoAttack &&
            availableAttackSquares.Count != 0)
        {
            AutoAttackWithSelectedPiece();
        }
        hasRequestedAction = true;
    }

    public void OnUICancelButtonClicked()
    {
        phase = Phase.Idle;
        ClearPieceMovePaths();
        ClearPieceAttackTargets();
        hasRequestedAction = true;
    }

    public void OnUIEndTurnButtonClick()
    {
        EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        if (phase == Phase.WaitTurn)
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseClick();
        }
    }

    private void SelectPiece(GameObject piece)
    {
        if (selectedPiece != piece)
        {
            DeselectPiece();
            selectedPiece = piece;
            Behaviour halo = selectedPiece.GetComponent("Halo") as Behaviour;
            halo.enabled = true;

            selectedPiece.GetComponent<Piece>().pieceUi.Show();

            if (selectedPiece.GetComponent<Piece>().OwnerType == Piece.Ownership.AI)
            {
                ShowPieceMovePaths();
                ShowPieceAttackTargets();
            }
        }
    }

    private void DeselectPiece()
    {
        if (selectedPiece)
        {
            ClearPieceMovePaths();
            ClearPieceAttackTargets();

            Behaviour halo = selectedPiece.GetComponent("Halo") as Behaviour;
            halo.enabled = false;

            selectedPiece.GetComponent<Piece>().pieceUi.Hide();

            selectedPiece = null;
        }
    }

    private void ShowPieceMovePaths()
    {
        if (selectedPiece)
        {
            Piece pieceComp = selectedPiece.GetComponent<Piece>();

            var paths = pieceComp.GetPossiblePaths();

            if (pieceComp.OwnerType == Piece.Ownership.Human)
            {
                Board.Instance.FilterPaths(paths, Piece.Ownership.FilterAllPieces);
            }
            
            List<HighlightData> squaresToHighlight = new List<HighlightData>();
            foreach (var path in paths)
            {
                foreach (var coords in path)
                {
                    Transform currentSquare = Board.Instance.gameObject.transform.GetChild(coords.x * Board.Columns + coords.y);
                    squaresToHighlight.Add(new HighlightData(coords
                        , Color.green, currentSquare.GetComponent<MeshRenderer>().material.GetColor("_Color"))
                    );

                    if (pieceComp.OwnerType == Piece.Ownership.Human)
                    {
                        availableMoveSquares.Add(coords);
                    }
                }
            }
            Board.Instance.HighlightSquares(squaresToHighlight);
        }
    }

    private void ClearPieceMovePaths()
    {
        availableMoveSquares.Clear();
        Board.Instance.RemoveHighlightedSquares();
    }

    private void ShowPieceAttackTargets()
    {
        if (selectedPiece)
        {
            Piece pieceComp = selectedPiece.GetComponent<Piece>();

            var paths = pieceComp.GetPossibleAttacks();

            if (pieceComp.OwnerType == Piece.Ownership.Human)
            {
                Board.Instance.FilterTargets(paths, Piece.Ownership.FilterAllPieces ^ pieceComp.OwnerType);
            }

            List<HighlightData> squaresToHighlight = new List<HighlightData>();
            foreach (var path in paths)
            {
                foreach (var coords in path)
                {
                    Transform currentSquare = Board.Instance.gameObject.transform.GetChild(coords.x * Board.Columns + coords.y);
                    squaresToHighlight.Add(new HighlightData(coords
                        , Color.red, currentSquare.GetComponent<MeshRenderer>().material.GetColor("_Color")));

                    if (pieceComp.OwnerType == Piece.Ownership.Human)
                    {
                        availableAttackSquares.Add(coords);
                    }
                }
            }
            Board.Instance.HighlightSquares(squaresToHighlight);
        }
    }

    private void ClearPieceAttackTargets()
    {
        availableAttackSquares.Clear();
        Board.Instance.RemoveHighlightedSquares();
    }

    private void AttemptMove(Square square)
    {
        if (selectedPiece &&
            selectedPiece.GetComponent<Piece>().OwnerType == Piece.Ownership.Human)
        {
            foreach (var sqr in availableMoveSquares)
            {
                if (sqr == square.GridPosition)
                {
                    MoveSelectedPiece(square.GridPosition);
                    ClearPieceMovePaths();
                    DeselectPiece();
                    break;
                }
            }
        }
    }

    private void AttemptAttack(Piece piece)
    {
        if (selectedPiece)
        {
            bool isValidAttack = false;
            foreach (var sqr in availableAttackSquares)
            {
                Vector3 position = Board.Instance.GridToWorldPosition(sqr);
                if (position.x == piece.transform.position.x &&
                    position.z == piece.transform.position.z)
                {
                    isValidAttack = true;
                    break;
                }
            }

            if (isValidAttack)
            {
                AttackWithSelectedPiece(piece.GetGridPos());
                ClearPieceAttackTargets();
                DeselectPiece();
            }
        }
    }

    private void MoveSelectedPiece(Vector2Int destination)
    {
        if (selectedPiece)
        {
            Piece pieceComp = selectedPiece.GetComponent<Piece>();

            pieceComp.SetMoveDest(destination);
        }
    }

    private void AttackWithSelectedPiece(Vector2Int attackTarget)
    {
        if (selectedPiece)
        {
            List<Vector2Int> targets = new List<Vector2Int>();
            targets.Add(attackTarget);
            selectedPiece.GetComponent<Piece>().SetAttackDests(targets);
            DeselectPiece();
        }
    }

    private void AutoAttackWithSelectedPiece()
    {
        if (selectedPiece)
        {
            List<Vector2Int> attackDestinations = new List<Vector2Int>();
            foreach (var destination in availableAttackSquares)
            {
                attackDestinations.Add(destination);
            }
            selectedPiece.GetComponent<Piece>().SetAttackDests(attackDestinations);
            DeselectPiece();
        }
    }

    private void HandleMouseClick()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        // If we've clicked on the UI, we don't want to do anything here.
        // If the player has clicked on a button, the assigned handlers
        // will take care of that.
        if (results.Count != 0 || hasRequestedAction)
        {
            if (hasRequestedAction)
            {
                hasRequestedAction = false;
            }
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            Piece pieceComp = hitInfo.collider.gameObject.GetComponent<Piece>();
            if (pieceComp)
            {
                if (phase == Phase.Idle)
                {
                    SelectPiece(pieceComp.gameObject);
                }
                else if (phase == Phase.Attack && pieceComp.gameObject != selectedPiece)
                {
                    AttemptAttack(pieceComp);
                }
            }
            else if (hitInfo.collider.gameObject.tag == "Square")
            {
                if (phase == Phase.Idle)
                {
                    Square square = hitInfo.collider.gameObject.GetComponent<Square>();
                    if (square.Piece)
                    {
                        SelectPiece(square.Piece.gameObject);
                    }
                    else
                    {
                        DeselectPiece();
                    }
                }
                else if (phase == Phase.Move)
                {
                    AttemptMove(hitInfo.collider.gameObject.GetComponent<Square>());
                }
                else if (phase == Phase.Attack)
                {
                    Square square = hitInfo.collider.gameObject.GetComponent<Square>();

                    if (square.Piece)
                    {
                        AttemptAttack(square.Piece);
                    }
                }
            }
            else
            {
                if (phase == Phase.Idle)
                {
                    DeselectPiece();
                }
            }
        }
        else
        {
            DeselectPiece();
        }
    }

    private void EndTurn()
    {
        if (phase == Phase.Idle)
        {
            phase = Phase.WaitTurn;
            DeselectPiece();
        }
    }
}
