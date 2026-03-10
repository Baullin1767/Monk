using Monk.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class TextWindowView : MonoBehaviour
    {
        [SerializeField] private TextContentConfig config;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private ScrollRect scrollView;

        private void Awake()
        {
            CacheReferences();
            ConfigureScrollView();
        }

        private void OnEnable()
        {
            ApplyConfig();
            RefreshLayoutAndPosition();
        }

        private void CacheReferences()
        {
            if (titleText == null)
            {
                titleText = FindTextByName("Title");
                titleText ??= FindTextByName("Title (1)");
                titleText ??= FindTextByName("Titletext");
            }

            if (scrollView == null)
            {
                scrollView = GetComponentInChildren<ScrollRect>(true);
            }

            if (bodyText == null)
            {
                bodyText = FindBodyText();
            }
        }

        private TextMeshProUGUI FindTextByName(string objectName)
        {
            var textTransform = transform.Find(objectName);
            return textTransform != null ? textTransform.GetComponent<TextMeshProUGUI>() : null;
        }

        private TextMeshProUGUI FindBodyText()
        {
            if (scrollView != null && scrollView.content != null)
            {
                var textInContent = scrollView.content.GetComponentInChildren<TextMeshProUGUI>(true);
                if (textInContent != null)
                {
                    return textInContent;
                }
            }

            var bodyTransform = transform.Find("ScrollView/Viewport/Content/BodyText");
            return bodyTransform != null ? bodyTransform.GetComponent<TextMeshProUGUI>() : null;
        }

        private void ConfigureScrollView()
        {
            if (scrollView == null)
            {
                return;
            }

            if (scrollView.viewport == null)
            {
                var viewport = scrollView.transform.Find("Viewport") as RectTransform;
                if (viewport != null)
                {
                    scrollView.viewport = viewport;
                }
            }

            if (scrollView.content == null && scrollView.viewport != null)
            {
                var content = scrollView.viewport.transform.Find("Content") as RectTransform;
                if (content != null)
                {
                    scrollView.content = content;
                }
            }

            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.movementType = ScrollRect.MovementType.Elastic;

            if (scrollView.scrollSensitivity <= 0f)
            {
                scrollView.scrollSensitivity = 20f;
            }
        }

        private void ApplyConfig()
        {
            if (config == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text = config.Title;
            }

            if (bodyText != null)
            {
                bodyText.text = config.Body;
                bodyText.enableWordWrapping = true;
                bodyText.raycastTarget = false;
            }
        }

        private void RefreshLayoutAndPosition()
        {
            if (scrollView == null || scrollView.content == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.content);
            scrollView.verticalNormalizedPosition = 1f;
        }
    }
}
