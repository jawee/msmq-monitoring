using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;

namespace QueueMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = @"C:\TMP\Output\queue-monitoring.txt";
            var dict = new Dictionary<string, int>();
            if (File.Exists(filePath))
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(filePath));
            }

            var queuePaths = ConfigurationManager.AppSettings["queues"].Split(',');

            foreach(var queue in queuePaths)
            {
                if(!dict.ContainsKey(queue))
                {
                    dict.Add(queue, GetCount(queue));
                } else
                {
                    var dictValue = dict[queue];
                    var curValue = GetCount(queue);

                    if (curValue > dictValue && dictValue > 0)
                    {
                        //Create error
                        using(var eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "jawee.QueueMonitor";
                            eventLog.WriteEntry($"Queue {queue} is possibly blocked", EventLogEntryType.Warning);
                        }
                        Console.WriteLine("ERROR. Create Alert");
                    }

                    dict[queue] = curValue;
                }
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(dict));
            Console.WriteLine("New Dictionary written to file");
            Console.ReadLine();
        }

        public static int GetCount(string path)
        {
            var queue = new MessageQueue(path);
            var enumerator = queue.GetMessageEnumerator2();

            var count = 0;
            while (enumerator.MoveNext())
            {
                count++;
            }

            return count;
        }
    }
}
