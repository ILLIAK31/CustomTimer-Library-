using System;
using System.Threading.Tasks;
using System.Threading;

namespace ClassTimer
{
    public class CustomTimer
    {
        private readonly object lockObject = new object();
        private Timer timer;
        private bool isRunning;
        private int interval;
        public event Action TimeElapsed;
        public event Action<Exception> OnError;

        public CustomTimer(int interval)
        {
            this.interval = interval;
            isRunning = false;
        }
        public void Start(int time)
        {
            lock (lockObject)
            {
                if (isRunning)
                    return;

                timer = new Timer(TimerCallback, null, 0, interval);
                isRunning = true;
            }
            Thread.Sleep(time);
        }
        public void Stop()
        {
            lock (lockObject)
            {
                if (!isRunning)
                    return;

                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                isRunning = false;
            }

        }
        private void TimerCallback(object state)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    TimeElapsed?.Invoke();
                });
            }
            catch (Exception ex)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    OnError?.Invoke(ex);
                });
            }
        }
    }
}
