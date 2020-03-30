using System.Threading.Tasks;

namespace Reimu.Core.Interaction
{
    public interface IInteractive<T>
    {
        Task<bool> JudgeAsync(BotContext context, T typeParam);
    }
}
