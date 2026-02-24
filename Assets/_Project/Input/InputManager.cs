using UnityEngine;
using Monk.Core;
using Monk.Common;

namespace Monk.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private DesktopInputProvider desktopInputProvider;
        [SerializeField] private MobileInputProvider mobileInputProvider;
        [SerializeField] private bool useMobileInputInEditor;

        private IInputProvider activeProvider;

        public IInputProvider ActiveProvider => activeProvider;

        private void Awake()
        {
            DetectPlatform();
            ServiceLocator.Register(activeProvider);
        }

        private void DetectPlatform()
        {
            if (desktopInputProvider == null)
            {
                desktopInputProvider = GetComponent<DesktopInputProvider>();
            }

            if (mobileInputProvider == null)
            {
                mobileInputProvider = GetComponent<MobileInputProvider>();
            }

            var shouldUseMobile = Application.isMobilePlatform || (Application.isEditor && useMobileInputInEditor);
            if (shouldUseMobile)
            {
                if (mobileInputProvider == null)
                {
                    mobileInputProvider = gameObject.AddComponent<MobileInputProvider>();
                }

                activeProvider = mobileInputProvider;
            }
            else
            {
                if (desktopInputProvider == null)
                {
                    desktopInputProvider = gameObject.AddComponent<DesktopInputProvider>();
                }

                activeProvider = desktopInputProvider;
            }

            if (desktopInputProvider != null)
            {
                desktopInputProvider.enabled = activeProvider == desktopInputProvider;
            }

            if (mobileInputProvider != null)
            {
                mobileInputProvider.enabled = activeProvider == mobileInputProvider;
            }
        }
    }
}
