using System.Collections.Generic;
using Reimu.Database.Models.Parts;

namespace Reimu.Database.Models
{
    /// <summary>
    /// Guild specific configuration
    /// </summary>
    public class GuildConfig : DatabaseItem
    {
        /// <summary>
        /// Command prefix for the guild
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Id of role given to users on join
        /// </summary>
        public ulong JoinRole { get; set; }

        /// <summary>
        /// Percent chance of replies to messages
        /// </summary>
        public int FunnyBusiness { get; set; } = 20;

        /// <summary>
        /// Id of role given when users pass a gateway (e.g. accepting rules)
        /// </summary>
        public ulong VerificationRole { get; set; }

        /// <summary>
        /// Message to be reacted to for gateway passing
        /// </summary>
        public ulong VerificationMessage { get; set; }

        /// <summary>
        /// Id of channel to send user join messages to
        /// </summary>
        //public ulong JoinChannel { get; set; }

        //public List<string> JoinMessages { get; set; } = new List<string>(5);

        /// <summary>
        /// Settings for when a user joins the guild
        /// </summary>
        public UserJoinLeaveSettings JoinSettings { get; set; } = new UserJoinLeaveSettings();

        /// <summary>
        /// Id of channel to send user leave messages to
        /// </summary>
        //public ulong LeaveChannel { get; set; }

        //public List<string> LeaveMessages { get; set; } = new List<string>(5);

        /// <summary>
        /// Settings for when a user leaves the guild
        /// </summary>
        public UserJoinLeaveSettings LeaveSettings { get; set; } = new UserJoinLeaveSettings();

        public GuildXpSettings XpSettings { get; set; } = new GuildXpSettings();

        /// <summary>
        /// Moderation settings and data
        /// </summary>
        public GuildModeration Moderation { get; set; } = new GuildModeration();

        /// <summary>
        /// The default listing of the guild's self-assignable roles
        /// </summary>
        public SelfRoleMenu DefaultRoleMenu
        {
            get
            {
                if (!SelfroleMenus.ContainsKey("default"))
                    SelfroleMenus.Add("default", new SelfRoleMenu());

                return SelfroleMenus["default"];
            }
        }

        /// <summary>
        /// Collection of self-assignable role menus
        /// </summary>
        public Dictionary<string, SelfRoleMenu> SelfroleMenus { get; set; } = new Dictionary<string, SelfRoleMenu>();

        /// <summary>
        /// Guild specific profile data for users
        /// </summary>
        public Dictionary<ulong, GuildProfile> UserProfiles { get; set; } = new Dictionary<ulong, GuildProfile>();
    }
}
