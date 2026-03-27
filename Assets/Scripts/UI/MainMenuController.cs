using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: UIDocument nao encontrado");
            return;
        }

        root = uiDocument.rootVisualElement;
        RegisterButtonCallbacks();
    }

    private void RegisterButtonCallbacks()
    {
        // Main Menu Section
        root.Q<Button>("PlayButton").clicked += OnPlayButtonClicked;
        root.Q<Button>("OptionsButton").clicked += OnOptionsButtonClicked;
        root.Q<Button>("QuitButton").clicked += OnQuitButtonClicked;

        // Levels Section
        root.Q<Button>("BackFromLevelsButton").clicked += OnBackFromLevelsClicked;
        PopulateLevelButtons();

        // Options Section
        root.Q<Button>("BackFromOptionsButton").clicked += OnBackFromOptionsClicked;
        root.Q<Button>("KeysButton").clicked += OnKeysButtonClicked;
        root.Q<Button>("VolumeButton").clicked += OnVolumeButtonClicked;

        // Volume Sliders
        var musicSlider = root.Q<SliderInt>("MusicVolumeSlider");
        var sfxSlider = root.Q<SliderInt>("SfxVolumeSlider");

        if (musicSlider != null)
        {
            musicSlider.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
            Debug.Log("MainMenuController: Slider de música registrado com sucesso");
        }
        else
        {
            Debug.LogWarning("MainMenuController: Slider de música não encontrado no UXML");
        }

        if (sfxSlider != null)
        {
            sfxSlider.RegisterValueChangedCallback(evt => OnSfxVolumeChanged(evt.newValue));
            Debug.Log("MainMenuController: Slider de SFX registrado com sucesso");
        }
        else
        {
            Debug.LogWarning("MainMenuController: Slider de SFX não encontrado no UXML");
        }
    }

    private void PopulateLevelButtons()
    {
        if (levelManager == null)
        {
            Debug.LogWarning("MainMenuController: LevelManager nao atribuido. Usando botoes placeholder.");
            return;
        }

        var levelsGrid = root.Q("LevelsGrid");
        var levelCount = levelManager.GetLevelCount();

        // Remove botoes placeholder se houver mais niveis
        if (levelCount > 0)
        {
            levelsGrid.Clear();

            for (int i = 0; i < levelCount; i++)
            {
                var levelData = levelManager.GetLevel(i);
                if (levelData == null)
                    continue;

                var button = new Button(() => OnLevelClicked(levelData))
                {
                    text = levelData.levelName,
                    name = $"Level_{i}"
                };

                button.style.minWidth = 120;
                button.style.paddingLeft = 15;
                button.style.paddingRight = 15;
                button.style.paddingTop = 15;
                button.style.paddingBottom = 15;
                button.style.fontSize = 16;
                button.style.unityFontStyleAndWeight = FontStyle.Bold;
                button.style.backgroundColor = new Color(50f / 255f, 150f / 255f, 200f / 255f);
                button.style.color = Color.white;
                button.style.borderBottomLeftRadius = 8;
                button.style.borderBottomRightRadius = 8;
                button.style.borderTopLeftRadius = 8;
                button.style.borderTopRightRadius = 8;
                button.style.flexGrow = 0;
                button.style.flexShrink = 0;

                levelsGrid.Add(button);
            }
        }
    }

    // ============ MAIN MENU NAVIGATION ============

    private void OnPlayButtonClicked()
    {
        ShowSection("LevelsSection");
    }

    private void OnOptionsButtonClicked()
    {
        ShowSection("OptionsSection");
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ============ LEVELS SECTION ============

    private void OnBackFromLevelsClicked()
    {
        ShowSection("MainMenuSection");
    }

    private void OnLevelClicked(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("MainMenuController: LevelData invalido");
            return;
        }

        GameSession.Instance.SelectLevel(levelData);
    }

    // ============ OPTIONS SECTION ============

    private void OnBackFromOptionsClicked()
    {
        ShowSection("MainMenuSection");
    }

    private void OnKeysButtonClicked()
    {
        var details = root.Q("KeysDetails");
        if (details != null)
        {
            details.style.display = details.style.display == DisplayStyle.None
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void OnVolumeButtonClicked()
    {
        var details = root.Q("VolumeDetails");
        if (details != null)
        {
            details.style.display = details.style.display == DisplayStyle.None
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void OnMusicVolumeChanged(int value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value / 100f);
            Debug.Log($"MainMenuController: Volume de música ajustado para {value}%");
        }
        else
        {
            Debug.LogWarning("MainMenuController: AudioManager não disponível. Volume de música não será alterado.");
        }

        var label = root.Q<Label>("MusicVolumeLabel");
        if (label != null)
            label.text = $"{value}%";
    }

    private void OnSfxVolumeChanged(int value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value / 100f);
            Debug.Log($"MainMenuController: Volume de SFX ajustado para {value}%");
        }
        else
        {
            Debug.LogWarning("MainMenuController: AudioManager não disponível. Volume de SFX não será alterado.");
        }

        var label = root.Q<Label>("SfxVolumeLabel");
        if (label != null)
            label.text = $"{value}%";
    }

    // ============ UTILITY ============

    private void ShowSection(string sectionName)
    {
        var mainSection = root.Q("MainMenuSection");
        var levelsSection = root.Q("LevelsSection");
        var optionsSection = root.Q("OptionsSection");

        mainSection.style.display = DisplayStyle.None;
        levelsSection.style.display = DisplayStyle.None;
        optionsSection.style.display = DisplayStyle.None;

        var targetSection = root.Q(sectionName);
        if (targetSection != null)
            targetSection.style.display = DisplayStyle.Flex;
    }
}
