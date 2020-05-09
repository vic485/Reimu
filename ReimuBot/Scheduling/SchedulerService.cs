using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Discord.WebSocket;
using Reimu.Common.Logging;
using Reimu.Database;
using Reimu.Database.Models;

namespace Reimu.Scheduling
{
    public class SchedulerService
    {
        private DiscordShardedClient _client;
        private DatabaseHandler _database;

        private Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();

        public SchedulerService(DiscordShardedClient client, DatabaseHandler database)
        {
            _client = client;
            _database = database;
        }

        public void SetupBaseTasks()
        {
            // Create a cleanup service to once a week
            _timers.Add("db-cleanup",
                new Timer(async _ => await DbCleanup(), null, DateTime.UtcNow.GetNextScheduleTime() - DateTime.UtcNow,
                    TimeSpan.FromDays(7)));
        }

        private async Task RemoveTimer(string id)
        {
            await _timers[id].DisposeAsync();
            _timers.Remove(id);
        }

        private Task DbCleanup()
        {
            Logger.LogInfo("Beginning database cleanup.");
            var guilds = _database.GetAll<GuildConfig>("guild-");

            foreach (var guild in guilds)
            {
                if (_client.GetGuild(ulong.Parse(guild.Id.Replace("guild-", ""))) != null)
                    continue;

                Logger.LogInfo($"Removing data {guild.Id}.");
                _database.Remove(guild.Id);
            }

            var users = _database.GetAll<UserData>("user-");
            foreach (var user in users)
            {
                if (_client.GetUser(ulong.Parse(user.Id.Replace("user-", ""))) != null)
                    continue;

                Logger.LogInfo($"Removing data {user.Id}.");
            }

            return Task.CompletedTask;
        }
    }
}
