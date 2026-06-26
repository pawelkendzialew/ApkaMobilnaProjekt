using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiAppMobile.Services.Timer
{
    public class WorkTimerService : IWorkTimerService
    {
        private readonly Dictionary<int, DateTime> _startTimes = new();

        public void StartTimer(int orderId)
        {
            if (_startTimes.ContainsKey(orderId))
                return;

            _startTimes[orderId] = DateTime.Now;
        }

        public void StopTimer(int orderId)
        {
            if (_startTimes.ContainsKey(orderId))
                _startTimes.Remove(orderId);
        }

        public TimeSpan GetElapsedTime(int orderId)
        {
            if (!_startTimes.ContainsKey(orderId))
                return TimeSpan.Zero;

            return DateTime.Now - _startTimes[orderId];
        }

        public bool IsRunning(int orderId)
        {
            return _startTimes.ContainsKey(orderId);
        }
    }
}
