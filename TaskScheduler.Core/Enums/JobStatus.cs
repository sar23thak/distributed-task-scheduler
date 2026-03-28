using System;
using System.Collections.Generic;
using System.Text;

namespace TaskScheduler.Core.Enums
{
    public enum JobStatus
    {
        Pending=0,
        Running=1,
        Completed=2,
        Failed=3,
        DeadLetter=4
    }
}
