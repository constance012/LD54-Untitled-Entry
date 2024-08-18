using UnityEngine;

/// <summary>
/// A static wrapper class for easily manipulating PlayerPref keys.
/// </summary>
public static class UserSettings
{
	#region Audio Settings
	public static float MasterVolume
	{
		get { return PlayerPrefs.GetFloat("MasterVolume", 1f); }
		set { PlayerPrefs.SetFloat("MasterVolume", value); }
	}

	public static float MusicVolume
	{
		get { return PlayerPrefs.GetFloat("MusicVolume", 1f); }
		set { PlayerPrefs.SetFloat("MusicVolume", value); }
	}

	public static float SoundsVolume
	{
		get { return PlayerPrefs.GetFloat("SoundsVolume", 1f); }
		set { PlayerPrefs.SetFloat("SoundsVolume", value); }
	}
	#endregion


	#region Graphics Settings
	public static int QualityLevel
	{
		get { return PlayerPrefs.GetInt("QualityLevel", 3); }
		set { PlayerPrefs.SetInt("QualityLevel", value); }
	}

	public static int ResolutionIndex
	{
		get { return PlayerPrefs.GetInt("ResolutionIndex", 7); }
		set { PlayerPrefs.SetInt("ResolutionIndex", value); }
	}

	public static bool IsFullscreen
	{
		get { return PlayerPrefs.GetInt("IsFullscreen", 0) == 1; }
		set { PlayerPrefs.SetInt("IsFullscreen", value ? 1 : 0); }
	}

	public static float TargetFramerate
	{
		get { return PlayerPrefs.GetFloat("TargetFramerate", 120f); }
		set { PlayerPrefs.SetFloat("TargetFramerate", value); }
	}

	public static bool UseVsync
	{
		get { return PlayerPrefs.GetInt("UseVsync", 0) == 1; }
		set { PlayerPrefs.SetInt("UseVsync", value ? 1 : 0); }
	}
	#endregion

	#region Controls Settings
	public static string SelectedKeyset
	{
		get { return PlayerPrefs.GetString("SelectedKeyset", "Default"); }
		set { PlayerPrefs.SetString("SelectedKeyset", value); }
	}
	#endregion

	/// <summary>
	/// Resets all the settings in the specified section to their default value.
	/// </summary>
	/// <param name="section"></param>
	public static void ResetToDefault(SettingSection section)
	{
		switch (section)
		{
			case SettingSection.Audio:
				MasterVolume = 1f;
				MusicVolume = 1f;
				SoundsVolume = 1f;
				break;

			case SettingSection.Graphics:
				QualityLevel = 3;
				ResolutionIndex = 7;
				IsFullscreen = false;
				TargetFramerate = 120f;
				UseVsync = false;
				break;

			case SettingSection.Controls:
				SelectedKeyset = "Default";
				break;

			case SettingSection.All:
				MasterVolume = 1f;
				MusicVolume = 1f;
				SoundsVolume = 1f;

				QualityLevel = 3;
				ResolutionIndex = 7;
				IsFullscreen = false;
				TargetFramerate = 120f;
				UseVsync = false;

				SelectedKeyset = "Default";
				break;
		}
	}

	/** <summary>
		Deletes the specified key by name, or you can optionally specified whether to delete all keys at once.
		<para />
		<c>WARNING: The method causes irreversible changes, use with your own risk.</c>
		</summary>
		<param name="keyName">The name of the key to be deleted.</param>
		<param name="deleteAll">Optional, set this to true in order to delete all keys at once.</param>
	**/
	public static void DeleteKey(string keyName, bool deleteAll = false)
	{
		if (deleteAll)
			PlayerPrefs.DeleteAll();
		else
			PlayerPrefs.DeleteKey(keyName);
	}

	public enum SettingSection { Audio, Graphics, Controls, All}
}
