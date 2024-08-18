using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
	[Header("Audio Mixer"), Space]
	[SerializeField] private AudioMixer mixer;

	[Header("UI References"), Space]
	//[SerializeField] private Slider _musicSlider;
	[SerializeField] private Slider _soundsSlider;
	[SerializeField] private TMP_Dropdown _qualityDropdown;

	// Private fields
	private TextMeshProUGUI _musicText;
	private TextMeshProUGUI _soundsText;

	private void Awake()
	{
		//_musicText = _musicSlider.GetComponentInChildren<TextMeshProUGUI>();
		_soundsText = _soundsSlider.GetComponentInChildren<TextMeshProUGUI>();
	}

	private void Start()
	{
		ReloadUI();
	}

	#region Callback Method for UI elements.
	public void SetMusicVolume(float amount)
	{
		mixer.SetFloat("musicVol", GetVolume(amount));

		_musicText.text = $"MUSIC: {ConvertDecibelToText(amount)}";
		UserSettings.MusicVolume = amount;
	}

	public void SetSoundsVolume(float amount)
	{
		mixer.SetFloat("soundsVol", GetVolume(amount));

		_soundsText.text = $"SOUND: {ConvertDecibelToText(amount)}";
		UserSettings.SoundsVolume = amount;
	}

	public void SetQualityLevel(int index)
	{
		QualitySettings.SetQualityLevel(index);
		UserSettings.QualityLevel = index;
	}

	public void ResetToDefault()
	{
		UserSettings.ResetToDefault(UserSettings.SettingSection.All);
		ReloadUI();
	}
	#endregion

	private string ConvertDecibelToText(float amount)
	{
		return (amount * 100f).ToString("0");
	}

	private float GetVolume(float amount) => Mathf.Log10(amount) * 20f;

	private void ReloadUI()
	{
		//float musicVol = UserSettings.MusicVolume;
		float soundsVol = UserSettings.SoundsVolume;

		//_musicSlider.value = musicVol;
		_soundsSlider.value = soundsVol;
		_qualityDropdown.value = UserSettings.QualityLevel;
	}
}
