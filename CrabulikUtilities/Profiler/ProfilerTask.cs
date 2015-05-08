using System.Diagnostics;

namespace CrabulikUtilities.Profiler
{
    public class ProfilerTask
    {
        public string Description { get; set; }

        public Stopwatch Watch = null;
    }
}