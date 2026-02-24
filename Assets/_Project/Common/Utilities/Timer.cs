using System;

namespace Monk.Common
{
    public class Timer
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }
        public event Action OnCompleted;

        public Timer(float duration)
        {
            Duration = duration;
        }

        public void Start()
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void Reset()
        {
        }
    }
}
