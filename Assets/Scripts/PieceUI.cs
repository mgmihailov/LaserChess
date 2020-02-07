using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PieceUI : MonoBehaviour
{
    public Button MoveButton;

    public Button AttackButton;

    public Button CancelButton;

    public Text HealthInfo;

    public Text AttackPowerInfo;

    private UnityAction OnMoveButtonClickActions;

    private UnityAction OnAttackButtonClickActions;

    private UnityAction OnCancelButtonClickActions;

    private State initialState;


    public void OnMoveButtonClick()
    {
        OnMoveButtonClickActions();

        ShowCancelButton();
    }

    public void OnAttackButtonClick()
    {
        OnAttackButtonClickActions();

        ShowCancelButton();
    }

    public void OnCancelButtonClick()
    {
        OnCancelButtonClickActions();

        HideCancelButton();
    }


    public void Initialize()
    {
        initialState.moveButtonEnabled = MoveButton.transform.parent.gameObject.activeSelf;
        initialState.attackButtonEnabled = AttackButton.transform.parent.gameObject.activeSelf;
        initialState.cancelButtonEnabled = CancelButton.transform.parent.gameObject.activeSelf;
        initialState.healthInfoEnabled = HealthInfo.transform.parent.gameObject.activeSelf;
        initialState.attackInfoEnabled = AttackPowerInfo.transform.parent.gameObject.activeSelf;

        MoveButton.onClick.AddListener(OnMoveButtonClick);
        AttackButton.onClick.AddListener(OnAttackButtonClick);
        CancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    public void Show()
    {
        bool moveButtonActive = !initialState.moveButtonEnabled ?
            initialState.moveButtonEnabled : CanPieceMove();
        bool attackButtonActive = !initialState.attackButtonEnabled ?
            initialState.attackButtonEnabled : CanPieceAttack();
        MoveButton.transform.parent.gameObject.SetActive(moveButtonActive);
        AttackButton.transform.parent.gameObject.SetActive(attackButtonActive);
        CancelButton.transform.parent.gameObject.SetActive(false);
        HealthInfo.transform.parent.gameObject.SetActive(true);
        AttackPowerInfo.transform.parent.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetHealthInfo(int health)
    {
        HealthInfo.text = $"HP: {health}";
    }

    public void SetAttackPowerInfo(int attackPower)
    {
        AttackPowerInfo.text = $"AP: {attackPower}";
    }

    public void SetEventCamera(Camera camera)
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = camera;
    }

    public void AddOnMoveListener(UnityAction onMoveCallback)
    {
        OnMoveButtonClickActions += onMoveCallback;
    }

    public void RemoveOnMoveListener(UnityAction onMoveCallback)
    {
        OnMoveButtonClickActions -= onMoveCallback;
    }

    public void AddOnAttackListener(UnityAction onAttackCallback)
    {
        OnAttackButtonClickActions += onAttackCallback;
    }

    public void RemoveOnAttackListener(UnityAction onAttackCallback)
    {
        OnAttackButtonClickActions -= onAttackCallback;
    }

    public void AddOnCancelActionListener(UnityAction onCancelCallback)
    {
        OnCancelButtonClickActions += onCancelCallback;
    }

    public void RemoveOnCancelActionListener(UnityAction onCancelCallback)
    {
        OnCancelButtonClickActions -= onCancelCallback;
    }

    private void ShowCancelButton()
    {
        MoveButton.transform.parent.gameObject.SetActive(false);
        AttackButton.transform.parent.gameObject.SetActive(false);
        HealthInfo.transform.parent.gameObject.SetActive(false);
        AttackPowerInfo.transform.parent.gameObject.SetActive(false);

        CancelButton.transform.parent.gameObject.SetActive(true);
    }

    private void HideCancelButton()
    {
        bool moveButtonActive = !initialState.moveButtonEnabled ?
            initialState.moveButtonEnabled : CanPieceMove();
        bool attackButtonActive = !initialState.attackButtonEnabled ?
            initialState.attackButtonEnabled : CanPieceAttack();
        MoveButton.transform.parent.gameObject.SetActive(moveButtonActive);
        AttackButton.transform.parent.gameObject.SetActive(attackButtonActive);
        HealthInfo.transform.parent.gameObject.SetActive(true);
        AttackPowerInfo.transform.parent.gameObject.SetActive(true);

        CancelButton.transform.parent.gameObject.SetActive(false);
    }

    private bool CanPieceMove()
    {
        Piece pieceComp = transform.parent.gameObject.GetComponent<Piece>();
        return pieceComp.CanMove();
    }

    private bool CanPieceAttack()
    {
        Piece pieceComp = transform.parent.gameObject.GetComponent<Piece>();
        return pieceComp.CanAttack();
    }

    protected struct State
    {
        public bool attackButtonEnabled;
        public bool moveButtonEnabled;
        public bool cancelButtonEnabled;
        public bool healthInfoEnabled;
        public bool attackInfoEnabled;
    }
}
