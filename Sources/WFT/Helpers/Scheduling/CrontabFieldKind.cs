using System;

namespace WFT.Helpers.Scheduling
{
    [Serializable]
    internal enum CrontabFieldKind
    {
        Minute = 0, // Keep in order of appearance in expression
        Hour,
        Day,
        Month,
        DayOfWeek
    }
}
