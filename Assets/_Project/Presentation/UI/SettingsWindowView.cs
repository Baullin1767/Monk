using Monk.Common;
using Monk.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class SettingsWindowView : MonoBehaviour
    {
        [SerializeField] private Sprite buttonBG;

        private const float DefaultMusicVolume = 0.7f;
        private const float DefaultSFXVolume = 1f;

        private PlayerPrefsStorage storage;
        private Slider musicSlider;
        private Slider sfxSlider;
        private TextMeshProUGUI musicValueText;
        private TextMeshProUGUI sfxValueText;
        private AudioManager audioManager;

        private void Awake()
        {
            storage = new PlayerPrefsStorage();
            audioManager = FindFirstObjectByType<AudioManager>();
            BuildUI();
            LoadValues();
        }

        private void OnEnable()
        {
            LoadValues();
        }

        private void BuildUI()
        {
            var rowsGo = new GameObject("SettingsRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rowsGo.transform.SetParent(transform, false);

            var rowsRect = rowsGo.GetComponent<RectTransform>();
            rowsRect.anchorMin = new Vector2(0.5f, 1f);
            rowsRect.anchorMax = new Vector2(0.5f, 1f);
            rowsRect.pivot = new Vector2(0.5f, 1f);
            rowsRect.anchoredPosition = new Vector2(0f, -400f);
            rowsRect.sizeDelta = new Vector2(900f, 400f);

            var layout = rowsGo.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 24f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var rowsRoot = rowsGo.transform;

            BuildSliderRow(rowsRoot, "Music", out musicSlider, out musicValueText);
            BuildSliderRow(rowsRoot, "SFX", out sfxSlider, out sfxValueText);
            BuildResetButton(rowsRoot);

            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        private void BuildSliderRow(Transform parent, string label, out Slider slider, out TextMeshProUGUI valueText)
        {
            var row = new GameObject($"Row_{label}", typeof(RectTransform), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.minHeight = 80f;
            layoutElement.preferredHeight = 80f;

            // Label
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(row.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.2f, 1f);
            labelRect.offsetMin = new Vector2(20f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelGo.GetComponent<TextMeshProUGUI>();
            labelText.fontSize = 30f;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;
            labelText.text = label;

            // Slider
            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(row.transform, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.22f, 0.2f);
            sliderRect.anchorMax = new Vector2(0.78f, 0.8f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill area
            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillAreaGo.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5f, 5f);
            fillAreaRect.offsetMax = new Vector2(-5f, -5f);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.GetComponent<Image>();
            fillImage.color = new Color(0.9f, 0.75f, 0.3f, 1f);

            // Handle area
            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(sliderGo.transform, false);
            var handleAreaRect = handleAreaGo.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 0f);
            var handleImage = handleGo.GetComponent<Image>();
            handleImage.color = Color.white;

            slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;

            // Percentage text
            var valueGo = new GameObject("ValueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            valueGo.transform.SetParent(row.transform, false);
            var valueRect = valueGo.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.8f, 0f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = new Vector2(-20f, 0f);

            valueText = valueGo.GetComponent<TextMeshProUGUI>();
            valueText.fontSize = 28f;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = Color.white;
        }

        private void BuildResetButton(Transform parent)
        {
            var row = new GameObject("Row_Reset", typeof(RectTransform), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.minHeight = 80f;
            layoutElement.preferredHeight = 80f;

            var buttonGo = new GameObject("ResetButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(row.transform, false);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(260f, 70f);

            var buttonImage = buttonGo.GetComponent<Image>();
            buttonImage.sprite = buttonBG;

            var button = buttonGo.GetComponent<Button>();
            button.onClick.AddListener(ResetToDefaults);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelGo.GetComponent<TextMeshProUGUI>();
            labelText.fontSize = 30f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.25f, 0.15f, 0.05f, 1f);
            labelText.text = "RESET";
        }

        private void LoadValues()
        {
            if (musicSlider == null) return;

            var musicVol = storage.GetFloat(Constants.PrefsKeys.MusicVolume, DefaultMusicVolume);
            var sfxVol = storage.GetFloat(Constants.PrefsKeys.SFXVolume, DefaultSFXVolume);

            musicSlider.SetValueWithoutNotify(musicVol);
            sfxSlider.SetValueWithoutNotify(sfxVol);

            UpdateValueText(musicValueText, musicVol);
            UpdateValueText(sfxValueText, sfxVol);

            ApplyVolumes(musicVol, sfxVol);
        }

        private void OnMusicVolumeChanged(float value)
        {
            storage.SetFloat(Constants.PrefsKeys.MusicVolume, value);
            storage.Save();
            UpdateValueText(musicValueText, value);

            if (audioManager != null)
                audioManager.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            storage.SetFloat(Constants.PrefsKeys.SFXVolume, value);
            storage.Save();
            UpdateValueText(sfxValueText, value);

            if (audioManager != null)
                audioManager.SetSFXVolume(value);
        }

        private void ResetToDefaults()
        {
            musicSlider.value = DefaultMusicVolume;
            sfxSlider.value = DefaultSFXVolume;
        }

        private void ApplyVolumes(float music, float sfx)
        {
            if (audioManager == null) return;
            audioManager.SetMusicVolume(music);
            audioManager.SetSFXVolume(sfx);
        }

        private static void UpdateValueText(TextMeshProUGUI text, float value)
        {
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
}
