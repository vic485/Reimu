using System;

namespace Reimu.Database.Models
{
    public class GlobalUser : DatabaseItem
    {
        public int Points { get; set; }
        public DateTime LastMessage { get; set; } = DateTime.MinValue;
        public int Yen { get; set; }
        public int Streak { get; set; }
        public DateTime LastClaimed { get; set; } = DateTime.MinValue;
    }
}