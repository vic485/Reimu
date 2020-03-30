using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reimu.Core.Interaction
{
    public class Interactive<T> : IInteractive<T>
    {
        private readonly List<IInteractive<T>> _interactives = new List<IInteractive<T>>();

        public Interactive<T> AddInteractive(IInteractive<T> interactive)
        {
            _interactives.Add(interactive);
            return this;
        }
        
        public async Task<bool> JudgeAsync(BotContext context, T typeParam)
        {
            foreach (var interactive in _interactives)
            {
                if (!await interactive.JudgeAsync(context, typeParam))
                    return false;
            }

            return true;
        }
    }
}
