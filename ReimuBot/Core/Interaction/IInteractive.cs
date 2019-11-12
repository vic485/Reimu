using System.Threading.Tasks;

// TODO: From old code-base. Needs cleaning up
namespace Reimu.Core.Interaction
{
    public interface IInteractive<T>
    {
        Task<bool> JudgeAsync(BotContext context, T typeParam);
    }
}