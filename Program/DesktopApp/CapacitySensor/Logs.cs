using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapacitySensor
{
    public static class Logs
    {
        public enum Type
        {
            Info,
            Warning,
            Error,
        }

        public enum From
        {
            User,
            PC,
            Device,
        }

        public struct Log
        {
            public From from;
            public Type type;
            public string time;
            public string message;

            public Log(From from, Type type, string message)
            {
                this.from = from;
                this.type = type;
                this.message = message;
                this.time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture);
            }
        }

        public static List<Log> List = new List<Log>();

        public static void Create(From from, Type type, string message)
        {
            message = message.Replace("\r", "").Replace("\n", "");
            Log log = new Log(from, type, message);
            List.Add(log);
            MainForm.Instance.AppendLog(log);
        }
    }
}
