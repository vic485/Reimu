using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reimu.Scheduler
{
    public class SchedulerService
    {
        // TODO: Some way to cleanup past timers?
        private List<Timer> _timers = new List<Timer>();

        public Task Schedule(TimeSpan fromNow, Action todo)
        {
            var timer = new Timer(x => todo.Invoke(), null, fromNow, TimeSpan.FromMilliseconds(-1));
            _timers.Add(timer);
            return Task.CompletedTask;
        }
    }
}