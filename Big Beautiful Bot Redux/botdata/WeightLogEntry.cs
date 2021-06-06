using System;
using ServiceStack.DataAnnotations;

namespace BBB.botdata
{
    internal class WeightLogEntry
    {
        [AutoIncrement] public long WeightLogEntryId { get; set; }
        public ulong UserId { get; set; }
        public double Weight { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}