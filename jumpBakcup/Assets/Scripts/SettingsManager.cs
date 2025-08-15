using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject startPanel;
    public Slider sensitivitySlider;

    private void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 50f); // Default value
        sensitivitySlider.value = savedSensitivity;

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        HelixRotator.sensitivity = savedSensitivity;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        startPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    private void OnSensitivityChanged(float value)
    {
        HelixRotator.sensitivity = value;
        PlayerPrefs.SetFloat("Sensitivity", value);
    }
}
