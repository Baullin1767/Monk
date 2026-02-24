using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Monk.Input
{
    public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnButtonPressed;
        public event Action OnButtonReleased;

        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            OnButtonPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            OnButtonReleased?.Invoke();
        }
    }
}
