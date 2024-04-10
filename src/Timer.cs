using GTA;
using System;

namespace EasyScript
{
    internal class Timer : IUpdate
    {
        public bool IsActive { get; private set; } = true;
        public float Interval;
        public float Elapsed => _currentTime;
        public bool IsLooping;
        public bool ResetWhenTimeReached = true;
        public event EventHandler OnTimeReached;
        private float _currentTime;

        public Timer()
        {
            ResetWhenTimeReached = false;
            IsActive = false;
        }

        public Timer(float timeSeconds, EventHandler onTimeReached = null, bool isLooping = false)
        {
            Interval = timeSeconds;
            OnTimeReached = onTimeReached;
            IsLooping = isLooping;
        }


        public void Start()
        {
            IsActive = true;
        }

        public void Stop()
        {
            IsActive = false;
        }

        /// <summary>
        /// Invert the current active state of the timer.
        /// </summary>
        public void ToggleTimerActive()
        {
            IsActive = !IsActive;
        }

        public void Reset()
        {
            _currentTime = 0f;
        }

        public void Update()
        {
            if (!IsActive) return;

            _currentTime += Game.LastFrameTime;

            if (ResetWhenTimeReached && _currentTime >= Interval)
            {
                Reset();
                OnTimeReached?.Invoke(this, EventArgs.Empty);
                if (!IsLooping)
                {
                    IsActive = false;
                }
            }
        }
    }
}
