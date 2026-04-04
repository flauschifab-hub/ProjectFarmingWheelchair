// dont optimize this shit please it barely works
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsPanel;
    public GameObject generalPanel;
    public GameObject audioPanel;
    public GameObject videoPanel;
    
    [Header("Buttons")]
    public Button generalButton;
    public Button audioButton;
    public Button videoButton;
    public Button exitButton;
    public Button backButton;
    
    [Header("General Settings")]
    public TMP_Dropdown languageDropdown;
    public Slider mouseSensitivitySlider;
    public TMP_Text mouseSensitivityValueText;
    public TMP_InputField mouseSensitivityInputField; 
    
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public TMP_Text volumeValueText;
    public TMP_InputField volumeInputField; 
    
    [Header("Video Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public TMP_Dropdown qualityDropdown;
    
[Header("Wheelchair Controller")]
    public WheelChairController wheelchairController;

    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.Escape;



    private const float DEFAULT_LOOK_SENSITIVITY = 2f; 
    private const float MIN_SLIDER_VALUE = 0.1f;
    private const float MAX_SLIDER_VALUE = 10f;


    private bool isOpen = false;
    private Resolution[] resolutions;
    private bool isToggleCooldown = false;
    private GameObject currentActivePanel = null;

    void Awake()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void Start()
    {
        HideAllSubPanels();
        
        Debug.Log("SettingsMenu Start() called");
        Debug.Log("Exit button assigned: " + (exitButton != null));
        if (exitButton != null)
        {
            Debug.Log("Exit button name: " + exitButton.name);
            Debug.Log("Exit button interactable: " + exitButton.interactable);
        Debug.Log("Exit button active in hierarchy: " + exitButton.gameObject.activeInHierarchy);
        }        
        SetupButtonListeners();
        
        InitializeVideoSettings();
        InitializeAudioSettings();
        InitializeGeneralSettings();
        
        LoadSettings();
        
        SetMaxResolution();
    }
    float SliderValueToSensitivity(float sliderValue)
    {
        return DEFAULT_LOOK_SENSITIVITY * sliderValue;
    }

    float SensitivityToSliderValue(float sensitivity)
    {
        return sensitivity / DEFAULT_LOOK_SENSITIVITY;
    }

    void SetMaxResolution()
    {
        Resolution[] allResolutions = Screen.resolutions;
        
        if (allResolutions.Length > 0)
        {
            Resolution maxResolution = allResolutions[0];
            int maxPixelCount = maxResolution.width * maxResolution.height;
            
            foreach (Resolution res in allResolutions)
            {
                int pixelCount = res.width * res.height;
                if (pixelCount > maxPixelCount)
                {
                    maxPixelCount = pixelCount;
                    maxResolution = res;
                }
            }
            
            Screen.SetResolution(maxResolution.width, maxResolution.height, Screen.fullScreen);
            
            Debug.Log($"Set to max resolution: {maxResolution.width} x {maxResolution.height}");
            
            if (resolutionDropdown != null && resolutions != null)
            {
                for (int i = 0; i < resolutions.Length; i++)
                {
                    if (resolutions[i].width == maxResolution.width && 
                        resolutions[i].height == maxResolution.height)
                    {
                        resolutionDropdown.value = i;
                        resolutionDropdown.RefreshShownValue();
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No resolutions found, using default resolution");
        }
    }

    void SetupButtonListeners()
    {
        if (generalButton != null)
        {
            generalButton.onClick.RemoveAllListeners();
            generalButton.onClick.AddListener(ShowGeneralPanel);
            Debug.Log("General button listener set");
        }
            
        if (audioButton != null)
        {
            audioButton.onClick.RemoveAllListeners();
            audioButton.onClick.AddListener(ShowAudioPanel);
            Debug.Log("Audio button listener set");
        }
            
        if (videoButton != null)
        {
            videoButton.onClick.RemoveAllListeners();
            videoButton.onClick.AddListener(ShowVideoPanel);
            Debug.Log("Video button listener set");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            
            exitButton.onClick.AddListener(ExitMenu);
            
            exitButton.onClick.AddListener(() => {
                Debug.Log("Exit button lambda triggered");
                ExitMenu();
            });
            
            Debug.Log("Exit button listeners added. Total listeners: " + exitButton.onClick.GetPersistentEventCount());
            
            exitButton.interactable = true;
            
            Debug.Log("Exit button setup complete. You can also call PublicExitMenu() from inspector");
        }
        else
        {
            Debug.LogError("Exit button is NULL!");
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
            Debug.Log("Back button listener set");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !isToggleCooldown)
        {
            Debug.Log("ESC pressed - Current menu state: " + (isOpen ? "Open" : "Closed"));
            ToggleMenu();
            
            isToggleCooldown = true;
            StartCoroutine(ResetToggleCooldown());
        }

        if (!isOpen && settingsPanel != null && settingsPanel.activeSelf)
        {
            Debug.LogWarning("Menu state mismatch detected - forcing close");
            settingsPanel.SetActive(false);
            HideAllSubPanels();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
                Debug.Log("Clicked on UI element: " + (clickedObject != null ? clickedObject.name : "unknown"));
                
                if (clickedObject != null && clickedObject.name == "ExitSettings")
                {
                    Debug.Log("Exit button clicked - manually triggering exit");
                    ForceCloseMenu();
                }
            }
        }
    }

    System.Collections.IEnumerator ResetToggleCooldown()
    {
        yield return null;
        isToggleCooldown = false;
    }

    public void PublicExitMenu()
    {
        Debug.Log("PublicExitMenu() called from inspector");
        ForceCloseMenu();
    }

    void ExitMenu()
    {
        Debug.Log("ExitMenu() method called directly");
        ForceCloseMenu();
    }

    void ForceCloseMenu()
    {
        Debug.Log("=== ForceCloseMenu() started ===");
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("Settings panel deactivated");
        }
        
        HideAllSubPanels();
        
        isOpen = false;
        
        if (wheelchairController != null)
        {
            wheelchairController.enabled = true;
            wheelchairController.ReenableControl();
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Time.timeScale = 1f;
        
        SaveSettings();
        
        Debug.Log("=== ForceCloseMenu() completed ===");
        Debug.Log("Menu closed. Settings panel active: " + (settingsPanel != null ? settingsPanel.activeSelf.ToString() : "null"));
    }

    public void EmergencyClose()
    {
        Debug.Log("EMERGENCY CLOSE CALLED");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        HideAllSubPanels();
        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void HideAllSubPanels()
    {
        if (generalPanel != null) generalPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (videoPanel != null) videoPanel.SetActive(false);
        currentActivePanel = null;
    }

    void ShowMainSettings()
    {
        HideAllSubPanels();
        
        if (generalButton != null) generalButton.gameObject.SetActive(true);
        if (audioButton != null) audioButton.gameObject.SetActive(true);
        if (videoButton != null) videoButton.gameObject.SetActive(true);
        if (exitButton != null) 
        {
            exitButton.gameObject.SetActive(true);
            exitButton.interactable = true; 
        }
        if (backButton != null) backButton.gameObject.SetActive(false);
    }

    void GoBack()
    {
        if (currentActivePanel != null)
        {
            ShowMainSettings();
        }
    }

    void ShowGeneralPanel()
    {
        HideAllSubPanels();
        if (generalPanel != null)
        {
            generalPanel.SetActive(true);
            currentActivePanel = generalPanel;
        }
        HideMainButtons();
        if (backButton != null) backButton.gameObject.SetActive(true);
    }

    void ShowAudioPanel()
    {
        HideAllSubPanels();
        if (audioPanel != null)
        {
            audioPanel.SetActive(true);
            currentActivePanel = audioPanel;
        }
        HideMainButtons();
        if (backButton != null) backButton.gameObject.SetActive(true);
    }

    void ShowVideoPanel()
    {
        HideAllSubPanels();
        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
            currentActivePanel = videoPanel;
        }
        HideMainButtons();
        if (backButton != null) backButton.gameObject.SetActive(true);
    }

    void HideMainButtons()
    {
        if (generalButton != null) generalButton.gameObject.SetActive(false);
        if (audioButton != null) audioButton.gameObject.SetActive(false);
        if (videoButton != null) videoButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);
    }

    void ToggleMenu()
    {
        if (isOpen)
        {
            ResumeGame();
        }
        else
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        settingsPanel.SetActive(true);
        ShowMainSettings();
        isOpen = true;

        if (wheelchairController != null) wheelchairController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        HideAllSubPanels();
        isOpen = false;

        if (wheelchairController != null) wheelchairController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        
        SaveSettings();
    }

    #region Video Settings
    void InitializeVideoSettings()
    {
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions.Select(resolution => new Resolution { 
                width = resolution.width, 
                height = resolution.height 
            }).Distinct().ToArray();
            
            resolutionDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width && 
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
        
        if (windowModeDropdown != null)
        {
            windowModeDropdown.ClearOptions();
            windowModeDropdown.AddOptions(new List<string> { "Fullscreen", "Windowed", "Borderless" });
            windowModeDropdown.value = (int)Screen.fullScreenMode;
            
            windowModeDropdown.onValueChanged.RemoveAllListeners();
            windowModeDropdown.onValueChanged.AddListener(SetWindowMode);
        }
        
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    void SetResolution(int index)
    {
        if (resolutions != null && index < resolutions.Length)
        {
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    void SetWindowMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            case 2: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
        }
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
    #endregion

    #region Audio Settings
    void InitializeAudioSettings()
    {
        if (masterVolumeSlider != null)
        {
            float currentVolume = AudioListener.volume;
            masterVolumeSlider.value = currentVolume;
            
            if (volumeValueText != null)
                volumeValueText.text = Mathf.RoundToInt(currentVolume * 100).ToString();
            
            if (volumeInputField != null)
                volumeInputField.text = Mathf.RoundToInt(currentVolume * 100).ToString();
            
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
            if (volumeInputField != null)
            {
                volumeInputField.onValueChanged.RemoveAllListeners();
                volumeInputField.onValueChanged.AddListener(OnVolumeInputChanged);
                
                volumeInputField.onEndEdit.RemoveAllListeners();
                volumeInputField.onEndEdit.AddListener(OnVolumeInputEndEdit);
            }
        }
    }

    void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100).ToString();
        
        if (volumeInputField != null)
            volumeInputField.text = Mathf.RoundToInt(value * 100).ToString();
    }

    void OnVolumeInputChanged(string input)
    {
        // This exists for when the Plyer is typing
    }

    void OnVolumeInputEndEdit(string input)
    {
        if (float.TryParse(input, out float volumePercent))
        {
            volumePercent = Mathf.Clamp(volumePercent, 0f, 100f);
            
            float volumeValue = volumePercent / 100f;
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = volumeValue;
            
            AudioListener.volume = volumeValue;
            
            if (volumeValueText != null)
                volumeValueText.text = volumePercent.ToString();
            
            if (volumeInputField != null)
                volumeInputField.text = volumePercent.ToString();
        }
        else
        {
            // Failsafe weil Erik ja unbedingt es irgendwie geschafft hat den scheiß kaputt zu machen
            if (volumeInputField != null)
                volumeInputField.text = Mathf.RoundToInt(AudioListener.volume * 100).ToString();
        }
    }
    #endregion

    #region General Settings
    void InitializeGeneralSettings()
    {
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Spanish", "French", "German", "Japanese" });
            languageDropdown.value = 0;
            
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(SetLanguage);
        }
        
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = MIN_SLIDER_VALUE;
            mouseSensitivitySlider.maxValue = MAX_SLIDER_VALUE;
            
            float currentSliderValue = GetCurrentSliderValue();
            
            mouseSensitivitySlider.value = currentSliderValue;
            
            if (mouseSensitivityValueText != null)
                mouseSensitivityValueText.text = currentSliderValue.ToString("F1") + "x";
            
            if (mouseSensitivityInputField != null)
                mouseSensitivityInputField.text = currentSliderValue.ToString("F1");
            
            mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivityFromSlider);
            
            // Setup input field listeners since it dosent work otherwise, trust me I tried future me just leave this as is
            if (mouseSensitivityInputField != null)
            {
                mouseSensitivityInputField.onValueChanged.RemoveAllListeners();
                mouseSensitivityInputField.onValueChanged.AddListener(OnMouseSensitivityInputChanged);
                
                mouseSensitivityInputField.onEndEdit.RemoveAllListeners();
                mouseSensitivityInputField.onEndEdit.AddListener(OnMouseSensitivityInputEndEdit);
            }
        }
    }

    float GetCurrentSliderValue()
    {
        if (wheelchairController != null)
        {
            return SensitivityToSliderValue(wheelchairController.lookSensitivity);
        }
        return 1f;
    }

    void SetLanguage(int index)
    {
        Debug.Log("Language changed to index: " + index);
    }

    void SetMouseSensitivityFromSlider(float sliderValue)
    {
        sliderValue = Mathf.Clamp(sliderValue, MIN_SLIDER_VALUE, MAX_SLIDER_VALUE);
        
        if (mouseSensitivityValueText != null)
            mouseSensitivityValueText.text = sliderValue.ToString("F1") + "x";
        
        if (mouseSensitivityInputField != null)
            mouseSensitivityInputField.text = sliderValue.ToString("F1");
        
        float actualSensitivity = SliderValueToSensitivity(sliderValue);
        
        if (wheelchairController != null)
        {
            wheelchairController.lookSensitivity = actualSensitivity;
            Debug.Log($"Mouse sensitivity set to: {wheelchairController.lookSensitivity} (slider: {sliderValue}x)");
        }
        else
        {
            Debug.LogWarning("WheelchairController not assigned, can't set mouse sensitivity");
        }
    }

    void OnMouseSensitivityInputChanged(string input)
    {
        // This is called while typing for the buttons in the inspector idk dosent tho ts dosent work
    }

    void OnMouseSensitivityInputEndEdit(string input)
    {
        if (float.TryParse(input, out float sliderValue))
        {
            sliderValue = Mathf.Clamp(sliderValue, MIN_SLIDER_VALUE, MAX_SLIDER_VALUE);
            
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = sliderValue;
        }
        else
        {
            float currentValue = mouseSensitivitySlider != null ? mouseSensitivitySlider.value : 1f;
            if (mouseSensitivityInputField != null)
                mouseSensitivityInputField.text = currentValue.ToString("F1");
        }
    }
    #endregion

    #region Save/Load Settings
    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        
        if (wheelchairController != null)
        {
            PlayerPrefs.SetFloat("WheelchairLookSensitivity", wheelchairController.lookSensitivity);
        }
        float sliderValue = mouseSensitivitySlider != null ? mouseSensitivitySlider.value : 1f;
        PlayerPrefs.SetFloat("MouseSensitivitySlider", sliderValue);
        
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Language", languageDropdown != null ? languageDropdown.value : 0);
        PlayerPrefs.SetInt("WindowMode", (int)Screen.fullScreenMode);
        
        if (resolutions != null && resolutionDropdown != null)
        {
            PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        }
        
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        if (masterVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);
        }
        
        if (wheelchairController != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("WheelchairLookSensitivity", DEFAULT_LOOK_SENSITIVITY);
            wheelchairController.lookSensitivity = savedSensitivity;
        }
        if (mouseSensitivitySlider != null)
        {
            float savedSliderValue = PlayerPrefs.GetFloat("MouseSensitivitySlider", 1f);
            mouseSensitivitySlider.value = savedSliderValue;
            SetMouseSensitivityFromSlider(savedSliderValue);
        }
        
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality);
        if (qualityDropdown != null) qualityDropdown.value = savedQuality;
        
        if (languageDropdown != null)
        {
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
        }
        
        if (windowModeDropdown != null)
        {
            windowModeDropdown.value = PlayerPrefs.GetInt("WindowMode", (int)FullScreenMode.ExclusiveFullScreen);
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", 0);
        }
    }
    #endregion
}