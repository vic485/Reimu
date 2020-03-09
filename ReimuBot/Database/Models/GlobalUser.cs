using System;

namespace Reimu.Database.Models
{
    public class GlobalUser : DatabaseItem
    {
        public int Credits { get; set; }
        public DateTime CreditsCollected { get; set; } = DateTime.UtcNow;
    }
}
