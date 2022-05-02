using System;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable ParameterHidesMember

namespace Ryuu
{
    [Serializable]
    public class Timer : IDisposable
    {
        private MonoUpdater updater;
        [SerializeField] private float timeStamp;
        [SerializeField] private float timeSet;
        [SerializeField] private float time;
        [SerializeField] private bool isStop;
        public event Action OnStop;
        public float Time => time;
        public bool IsStop => isStop;

        public Timer(UpdateMode updateMode, Action onStop = null)
        {
            OnStop = onStop;
            updater = Object.FindObjectOfType<MonoUpdater>();
            updater.Subscribe(OnUpdate, updateMode);
        }

        public Timer SetUpdateMode(UpdateMode updateMode)
        {
            updater.Subscribe(OnUpdate, updateMode);
            return this;
        }

        public Timer SetTime(float timeSet)
        {
            this.timeSet = timeSet;
            return this;
        }

        public Timer SetCountDown()
        {
            timeStamp = UnityEngine.Time.time;
            time = 0;
            return this;
        }

        public Timer Start()
        {
            timeStamp = UnityEngine.Time.time + time;
            isStop = false;
            return this;
        }

        public void OnUpdate()
        {
            if (isStop)
            {
                return;
            }

            time = UnityEngine.Time.time - timeStamp;

            if (time < timeSet)
            {
                return;
            }

            isStop = true;
            OnStop?.Invoke();
        }

        public Timer Stop()
        {
            isStop = true;
            return this;
        }

        public static Timer Event(Action onStop, UpdateMode updateMode, float time, int repeat = 1)
        {
            Timer timer = new Timer(updateMode, onStop).SetTime(time).SetCountDown().Start();
            if (repeat == -1)
            {
                timer.OnStop += () => { timer.SetCountDown().Start(); };
            }
            else
            {
                timer.OnStop += () =>
                {
                    repeat--;
                    if (repeat == 0)
                    {
                        timer.Dispose();
                    }
                    else
                    {
                        timer.SetCountDown().Start();
                    }
                };
            }

            return timer;
        }

        public void Dispose()
        {
            updater.Unsubscribe(OnUpdate);
        }
    }
}