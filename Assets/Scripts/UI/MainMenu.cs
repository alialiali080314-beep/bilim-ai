using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sensitivitySlider)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 2f);
            sensitivitySlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat("Sensitivity", v));
        }

        if (volumeSlider)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            volumeSlider.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("Volume", v);
            });
            AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
        }

        if (qualityDropdown)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(QualitySettings.SetQualityLevel);
        }
    }

    public void StartGame()       => SceneManager.LoadScene("GameScene");
    public void OpenSettings()    { mainPanel?.SetActive(false); settingsPanel?.SetActive(true); }
    public void CloseSettings()   { settingsPanel?.SetActive(false); mainPanel?.SetActive(true); }
    public void SaveAndBack()     { PlayerPrefs.Save(); CloseSettings(); }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
