using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reimu.Scheduler
{
    public class SchedulerService
    {
        // TODO: Some way to cleanup past timers?
        private Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();

        public Task Schedule(string id, TimeSpan fromNow, Action todo)
        {
            var timer = new Timer(async x =>
            {
                await Task.Run(todo.Invoke);
                await RemoveTimer(id);
            }, null, fromNow, TimeSpan.FromMilliseconds(-1));
            _timers.Add(id, timer);
            return Task.CompletedTask;
        }

        private async Task RemoveTimer(string id)
        {
            await _timers[id].DisposeAsync();
            _timers.Remove(id);
        }
    }
}