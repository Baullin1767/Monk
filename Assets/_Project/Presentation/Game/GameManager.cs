using System;
using UnityEngine;
using Monk.Core;

namespace Monk.Presentation
{
    public class GameManager : MonoBehaviour
    {
        public event Action<GameState> OnGameStateChanged;

        public GameState CurrentState { get; private set; }

        public void StartGame()
        {
        }

        public void PauseGame()
        {
        }

        public void ResumeGame()
        {
        }

        public void GameOver()
        {
        }

        public void LevelComplete()
        {
        }
    }
}
