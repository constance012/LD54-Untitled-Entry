using UnityEngine;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
	[Header("UI References"), Space]
	[SerializeField] private CanvasGroup gameOverScreen;
	[SerializeField] private CanvasGroup victoryScreen;
	[SerializeField] private HealthBar playerHPBar;

	public bool GameDone { get; private set; }

	private void Start()
	{
		InputManager.Instance.onBackToMenuAction += (sender, e) => ReturnToMenu();

		gameOverScreen.Toggle(false);
		victoryScreen.Toggle(false);
	}

	public void UpdateCurrentHealth(int currentHP)
	{
		playerHPBar.SetCurrentHealth(currentHP);
	}

	public void InitializeHealthBar(int initialHP)
	{
		playerHPBar.SetMaxHealth(initialHP);
	}

	/// <summary>
	/// Callback method for the retry button.
	/// </summary>
	public void RestartGame()
	{
		GameDone = false;

		LevelsManager.Instance.LoadSceneAsync("Scenes/Base Scene");
	}

	/// <summary>
	/// Callback method for the return to menu button.
	/// </summary>
	public void ReturnToMenu()
	{
		LevelsManager.Instance.LoadSceneAsync("Scenes/Menu");
	}

	public void ShowGameOverScreen()
	{
		GameDone = true;
		Inventory.Instance.Toggle(false);

		gameOverScreen.DOFade(1f, .75f)
					  .OnComplete(() => gameOverScreen.Toggle(true));
	}

	public void ShowVictoryScreen()
	{
		GameDone = true;
		Inventory.Instance.Toggle(false);

		victoryScreen.DOFade(1f, .75f)
					  .OnComplete(() => victoryScreen.Toggle(true));
	}
}
