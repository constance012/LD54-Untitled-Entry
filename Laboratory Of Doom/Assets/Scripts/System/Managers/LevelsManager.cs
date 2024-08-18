using CSTGames.DataPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : Singleton<LevelsManager>
{
	public int CurrentLevelIndex { get; set; } = -1;
	public Scene CurrentScene { get; private set; }

	private void Start()
	{
		CurrentScene = SceneManager.GetActiveScene();
		CurrentLevelIndex = CurrentScene.buildIndex;

		Scene firstLevel = SceneManager.GetSceneByBuildIndex(CurrentLevelIndex + 1);

		// Load the first level.
		if (CurrentScene != firstLevel)
			LoadLevel(LevelNavigation.Initialize);
	}

	public void LoadSceneAsync(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName);
	}
	
	public void LoadSceneAsync(int buildIndex)
	{
		SceneManager.LoadSceneAsync(buildIndex);
	}

	public void LoadNextLevel()
	{
		Debug.Log("Entering the next level...");
		LoadLevel(LevelNavigation.Next);
	}

	public void LoadPreviousLevel()
	{
		Debug.Log("Entering the previous level...");
		LoadLevel(LevelNavigation.Previous);
	}

	private void LoadLevel(LevelNavigation navigation)
	{
		switch (navigation)
		{
			case LevelNavigation.Next:
				GameDataManager.Instance.SaveGame();
				
				CurrentLevelIndex++;
				SceneManager.UnloadSceneAsync(CurrentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.Previous:
				GameDataManager.Instance.SaveGame();
				
				CurrentLevelIndex--;
				SceneManager.UnloadSceneAsync(CurrentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.Restart:
				SceneManager.UnloadSceneAsync(CurrentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.Initialize:
				CurrentLevelIndex++;
				break;
		}

		CurrentScene = SceneManager.GetSceneByBuildIndex(CurrentLevelIndex);

		if (!CurrentScene.isLoaded)
		{
			LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
			AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(CurrentLevelIndex, param);

			loadSceneOp.completed += OnLevelCompletedLoading;
		}
		else
			OnLevelCompletedLoading(null);
	}

	/// <summary>
	/// Callback method when the new level level has been asynchronously loaded.
	/// </summary>
	/// <param name="obj"></param>
	private void OnLevelCompletedLoading(AsyncOperation obj)
	{
		CurrentScene = SceneManager.GetSceneByBuildIndex(CurrentLevelIndex);
		SceneManager.SetActiveScene(CurrentScene);

		//// Set the event camera for the level's world canvas.
		//Canvas enemiesWorldCanvas = GameObject.FindWithTag("Enemies World Canvas").GetComponent<Canvas>();
		//enemiesWorldCanvas.worldCamera = Camera.main;
	}

	private enum LevelNavigation { Initialize, Previous, Next, Restart }
}
