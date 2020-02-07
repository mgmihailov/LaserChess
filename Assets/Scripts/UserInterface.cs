using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UserInterface : MonoBehaviour
{
    public GameObject MainMenu;

    public Text Title;

    public Button NextLevelButton;

    public Button RestartButton;

    public Button QuitButton;

    public Button EndTurnButton;

    private UnityAction onNextLevelButtonClickActions;

    private UnityAction onRestartButtonClickActions;

    private UnityAction onQuitButtonClickActions;

    private UnityAction onEndTurnButtonClickActions;

    private HumanPlayer player;


    public void Initialize(HumanPlayer player)
    {
        this.player = player;

        NextLevelButton.onClick.AddListener(OnNextLevelButtonClick);
        RestartButton.onClick.AddListener(OnRestartButtonClick);
        QuitButton.onClick.AddListener(OnQuitButtonClick);
        EndTurnButton.onClick.AddListener(OnEndTurnButtonClick);
    }

    public void OnNextLevelButtonClick()
    {
        onNextLevelButtonClickActions();

        HideMenu();
    }

    public void OnRestartButtonClick()
    {
        onRestartButtonClickActions();

        HideMenu();
    }

    public void OnQuitButtonClick()
    {
        onQuitButtonClickActions();

        HideMenu();
    }

    public void OnEndTurnButtonClick()
    {
        onEndTurnButtonClickActions();

        HideEndTurnButton();
    }

    public void ShowEndLevelMenu(string winner)
    {
        Title.text = $"{winner} wins!";
        NextLevelButton.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(false);
        QuitButton.gameObject.SetActive(true);

        MainMenu.SetActive(true);
    }

    public void ShowEndGameMenu()
    {
        Title.text = "GAME OVER";
        NextLevelButton.gameObject.SetActive(false);
        QuitButton.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);

        MainMenu.SetActive(true);
    }

    private void HideMenu()
    {
        MainMenu.SetActive(false);
    }

    public void ShowEndTurnButton()
    {
        HideMenu();

        EndTurnButton.gameObject.SetActive(true);
    }

    private void HideEndTurnButton()
    {
        EndTurnButton.gameObject.SetActive(false);
    }

    public void AddOnNextLevelButtonClickListener(UnityAction callback)
    {
        onNextLevelButtonClickActions += callback;
    }

    public void RemoveOnNextLevelButtonClickListener(UnityAction callback)
    {
        onNextLevelButtonClickActions -= callback;
    }

    public void AddOnRestartButtonClickListener(UnityAction callback)
    {
        onRestartButtonClickActions += callback;
    }

    public void RemoveOnRestartButtonClickListener(UnityAction callback)
    {
        onRestartButtonClickActions -= callback;
    }

    public void AddOnQuitButtonClickListener(UnityAction callback)
    {
        onQuitButtonClickActions += callback;
    }

    public void RemoveOnQuitButtonClickListener(UnityAction callback)
    {
        onQuitButtonClickActions -= callback;
    }

    public void AddOnEndTurnButtonClickListener(UnityAction callback)
    {
        onEndTurnButtonClickActions += callback;
    }

    public void RemoveOnEndTurnButtonClickListener(UnityAction callback)
    {
        onEndTurnButtonClickActions -= callback;
    }
}
