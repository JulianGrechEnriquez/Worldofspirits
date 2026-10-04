using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WorldOfSpirits.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeLabel;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button helpBackButton;
        private bool loading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedSettings()
        {
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Settings.MasterVolume", 1f));
            if (PlayerPrefs.HasKey("Settings.Fullscreen"))
                Screen.fullScreen = PlayerPrefs.GetInt("Settings.Fullscreen") != 0;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            UpdateVolumeLabel(AudioListener.volume);
            ShowHome();
        }

        public void Play()
        {
            if (loading) return;
            loading = true;
            playButton.interactable = false;
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync("Game");
        }

        public void ShowHome() => Show(homePanel, playButton);
        public void ShowSettings() => Show(settingsPanel, settingsBackButton);
        public void ShowHelp() => Show(helpPanel, helpBackButton);

        private void Show(GameObject panel, Button focus)
        {
            homePanel.SetActive(panel == homePanel);
            settingsPanel.SetActive(panel == settingsPanel);
            helpPanel.SetActive(panel == helpPanel);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(focus.gameObject);
        }

        public void SetVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Settings.MasterVolume", AudioListener.volume);
            UpdateVolumeLabel(AudioListener.volume);
        }

        private void UpdateVolumeLabel(float value) => volumeLabel.text = $"MASTER VOLUME   {Mathf.RoundToInt(value * 100)}%";

        public void SetFullscreen(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt("Settings.Fullscreen", value ? 1 : 0);
        }

        private void OnDisable() => PlayerPrefs.Save();

        public void Quit()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
