using System.Collections.Generic;

namespace Reimu.Database.Models.Parts
{
    public class SelfRoleMenu
    {
        public ulong? Channel { get; set; }
        public ulong? Message { get; set; }
        public Dictionary<string, ulong> SelfRoles { get; set; } = new Dictionary<string, ulong>();
    }
}
