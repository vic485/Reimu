using System.Collections.Generic;

namespace Reimu.Common.Data.Parts
{
    public class SelfRoleMenu
    {
        public ulong? Channel { get; set; }
        public ulong? Message { get; set; }
        public Dictionary<string, ulong> SelfRoles { get; set; } = new Dictionary<string, ulong>();
    }
}
