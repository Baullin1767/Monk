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

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            if (config == null) return;
            if (titleText != null) titleText.text = config.Title;
            if (bodyText != null) bodyText.text = config.Body;
        }

        private void BuildUI()
        {
            var titleTransform = transform.Find("Title");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<TextMeshProUGUI>();

            var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(transform, false);

            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(40f, 100f);
            scrollRect.offsetMax = new Vector2(-40f, -300f);

            var scrollImage = scrollGo.GetComponent<Image>();
            scrollImage.color = new Color(0f, 0f, 0f, 0f);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);

            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0f);

            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);

            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(0f, 0f);
            contentRect.offsetMax = new Vector2(0f, 0f);

            var contentLayout = contentGo.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(20, 20, 10, 10);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = contentGo.GetComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bodyGo = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyGo.transform.SetParent(contentGo.transform, false);

            bodyText = bodyGo.GetComponent<TextMeshProUGUI>();
            bodyText.fontSize = 26f;
            bodyText.color = Color.white;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.enableWordWrapping = true;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 20f;
        }
    }
}
