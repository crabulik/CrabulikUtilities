using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CrabulikUtilities.Profiler
{
    public static class Profiler
    {
        private static List<ProfilerStamp> _stampsList = new List<ProfilerStamp>();

        private static Dictionary<string, ProfilerTask> _tasks = new Dictionary<string, ProfilerTask>();

        public static void Reset()
        {
            _stampsList.Clear();
            if (_tasks.Count > 0)
            {
                foreach (var task in _tasks.Values)
                {
                    if ((task.Watch != null) && (task.Watch.IsRunning))
                    {
                        task.Watch.Stop();
                    }
                }
            }
        }

        public static void AddTask(string aKey, string aDescription)
        {
            ProfilerTask task;
            if (_tasks.TryGetValue(aKey, out task))
            {
                task.Watch.Stop();
                AddStamp(aKey, task.Description, task.Watch.ElapsedMilliseconds, task.Watch.Elapsed.Ticks);
            }
            else
            {
                task = new ProfilerTask
                {
                    Watch = new Stopwatch()
                };
                _tasks.Add(aKey, task);
            }
            task.Description = aDescription;
            task.Watch.Start();
        }

        public static void StopTask(string aKey)
        {
            ProfilerTask task;
            if (_tasks.TryGetValue(aKey, out task))
            {
                task.Watch.Stop();
                AddStamp(aKey, task.Description, task.Watch.ElapsedMilliseconds, task.Watch.Elapsed.Ticks);
                _tasks.Remove(aKey);
            }
        }

        public static string CreateTask(string aDescription)
        {
            var key = Guid.NewGuid().ToString();

            AddTask(key, aDescription);
            return key;
        }

        private static void AddStamp(string aKey, string aDescription, long aElapsedMilliseconds, long aElapsedTicks)
        {
            var item = new ProfilerStamp
            {
                Key = aKey,
                Description = aDescription,
                ElapsedMilliseconds = aElapsedMilliseconds,
                ElapsedTicks = aElapsedTicks
            };
            _stampsList.Add(item);
        }

        public static string Report(bool aNeedDetails = true)
        {
            return Report(aNeedDetails, new List<string>());
        }

        public static string Report(List<string> aDisplayDetailsList)
        {
            return Report(false, aDisplayDetailsList);
        }
        private static string Report(bool aNeedDetails, List<string> aDisplayList)
        {
            StringBuilder tmpReport = new StringBuilder("==================");
            tmpReport.AppendLine();
            tmpReport.AppendLine("Profiler Report");
            tmpReport.AppendLine("==================");
            if (_tasks.Count > 0)
            {
                tmpReport.AppendLine("==================");
                tmpReport.Append("Active tasks count: ");
                tmpReport.AppendLine(_tasks.Count.ToString());
                foreach (var item in _tasks)
                {
                    tmpReport.Append("   Key: ");
                    tmpReport.Append(item.Key);
                    tmpReport.Append(", Description: ");
                    tmpReport.Append(item.Value.Description);   
                    tmpReport.Append(", Watch State (ms): ");
                    tmpReport.Append(item.Value.Watch.ElapsedMilliseconds);
                    tmpReport.AppendLine();
                }
                tmpReport.AppendLine("==================");     
            }
            tmpReport.AppendLine("=======Stamps========");

            var list = (from p in _stampsList
                       group p by p.Key into g
                       select g).OrderBy(p => p.Key);
            foreach (var item in list)
            {
                tmpReport.Append(" Key: ");
                tmpReport.Append(item.Key);
                var time = new TimeSpan(item.ToList().Sum(stamp => stamp.ElapsedTicks));

                tmpReport.Append(", Total Time: ");
                tmpReport.Append(time.ToString(@"hh\:mm\:ss\.fff"));
                tmpReport.Append(", Min Time(ms): ");
                tmpReport.Append(item.ToList().Min(stamp => stamp.ElapsedMilliseconds));
                tmpReport.Append(", Max Time(ms): ");
                tmpReport.Append(item.ToList().Max(stamp => stamp.ElapsedMilliseconds));
                tmpReport.Append(", Avg Time(ms): ");
                tmpReport.Append(string.Format("{0:N2}", item.ToList().Average(stamp => stamp.ElapsedMilliseconds)));
                tmpReport.AppendLine();
                if ((aNeedDetails) || (aDisplayList.Exists(p => p == item.Key)))
                    foreach (var stamp in item.OrderByDescending(p => p.ElapsedMilliseconds).ToList())
                    {
                        tmpReport.Append("      Description: ");
                        tmpReport.Append(stamp.Description);
                        tmpReport.Append(", Elapsed(ms): ");
                        tmpReport.Append(stamp.ElapsedMilliseconds);
                        tmpReport.AppendLine();
                    }
            }
            return tmpReport.ToString();
        }
    }
}