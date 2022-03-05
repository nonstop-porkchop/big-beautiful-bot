using System;
using JetBrains.Annotations;
using ServiceStack.DataAnnotations;

namespace BBB.DataModel;

internal class WeightLogEntry
{
    [UsedImplicitly] [AutoIncrement] public long WeightLogEntryId { get; set; }
    [UsedImplicitly] public ulong UserId { get; set; }
    [UsedImplicitly] public double Weight { get; set; }
    [UsedImplicitly] public DateTime TimeStamp { get; set; }
}