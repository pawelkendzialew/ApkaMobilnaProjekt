using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiAppMobile.Services.Timer
{
    public interface IWorkTimerService
    {
        void StartTimer(int orderId);
        void StopTimer(int orderId);
        TimeSpan GetElapsedTime(int orderId);
        bool IsRunning(int orderId);
    }
}
