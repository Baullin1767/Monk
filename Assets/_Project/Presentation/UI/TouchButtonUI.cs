using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Monk.Presentation
{
    public class TouchButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnButtonPressed;
        public event Action OnButtonReleased;

        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }
    }
}
