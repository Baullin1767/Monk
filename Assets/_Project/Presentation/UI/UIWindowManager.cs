using UnityEngine;

namespace Monk.Presentation
{
    public class UIWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject storeWindow;
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private GameObject aboutWindow;
        [SerializeField] private GameObject privacyPolicyWindow;
        [SerializeField] private GameObject termsWindow;
        [SerializeField] private MainMenuStoreWindowView mainMenuStoreWindowView;
        [SerializeField] private SettingsWindowView settingsWindowView;
        [SerializeField] private TextWindowView aboutWindowView;
        [SerializeField] private TextWindowView termsWindowView;
        [SerializeField] private TextWindowView privacyPolicyWindowView;

        private GameObject activeWindow;
        

        private void Start()
        {
            HideAllWindows();
            ShowMainMenu();
        }

        private void HideAllWindows()
        {
            mainMenuPanel.SetActive(false);
            storeWindow.SetActive(false);
            settingsWindow.SetActive(false);
            aboutWindow.SetActive(false);
            termsWindow.SetActive(false);
            privacyPolicyWindow.SetActive(false);
        }

        public void ShowMainMenu()
        {
            CloseActiveWindow();
            mainMenuPanel.SetActive(true);
        }

        public void OpenWindow(GameObject window)
        {
            mainMenuPanel.SetActive(false);
            CloseActiveWindow();
            window.SetActive(true);
            activeWindow = window;
        }

        public void CloseActiveWindow()
        {
            if (activeWindow != null)
                activeWindow.SetActive(false);
            activeWindow = null;
        }

        public void OpenStore() => OpenWindow(storeWindow);
        public void OpenSettings() => OpenWindow(settingsWindow);
        public void OpenAbout() => OpenWindow(aboutWindow);
        public void OpenPrivacyPolicy() => OpenWindow(privacyPolicyWindow);
        public void OpenTerms() => OpenWindow(termsWindow);
        public void OnBackClicked() => ShowMainMenu();
    }
}
