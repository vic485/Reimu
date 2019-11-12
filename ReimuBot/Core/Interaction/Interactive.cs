using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reimu.Core.Interaction
{
    public class Interactive<T> : IInteractive<T>
    {
        List<IInteractive<T>> _interactiveList = new List<IInteractive<T>>();

        public Interactive<T> AddInteractive(IInteractive<T> interactive)
        {
            _interactiveList.Add(interactive);
            return this;
        }
        
        public async Task<bool> JudgeAsync(BotContext context, T typeParam)
        {
            foreach (var interactive in _interactiveList)
            {
                var result = await interactive.JudgeAsync(context, typeParam);
                if (!result)
                    return false;
            }

            return true;
        }
    }
}