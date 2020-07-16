using System.Collections.Generic;

namespace Reimu.Common.Data
{
    /// <summary>
    /// Storing statistics of the bot to show on the dashboard
    /// </summary>
    public class BotStatus : DatabaseItem
    {
        public Dictionary<int, string> ShardStatus = new Dictionary<int, string>();
    }
}
