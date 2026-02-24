using UnityEngine;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "TextContentConfig", menuName = "Monk/Configs/Text Content Config")]
    public class TextContentConfig : ScriptableObject
    {
        [SerializeField] private string title;
        [TextArea(5, 20)]
        [SerializeField] private string body;

        public string Title => title;
        public string Body => body;
    }
}
