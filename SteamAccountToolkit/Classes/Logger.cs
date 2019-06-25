using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Data;
using System.Windows.Interop;

namespace SteamAccountToolkit.Classes
{
    public class Logger
    {
        public class LogItem
        {

            public string Type { get; set; }
            public string Message { get; set; }
            public ObservableCollection<string> Category { get; } = new ObservableCollection<string>();
            public StackTrace StackInfo { get; set; }
            public Thread CurrentThread { get; set; }
            public string LoggedAt { get; set; }

            public string ThreadId => CurrentThread.ManagedThreadId.ToString();
        }

        public ObservableCollection<LogItem> LogCollection;

        public Logger()
        {
            LogCollection = new ObservableCollection<LogItem>();
        }

        public Logger(ref ObservableCollection<LogItem> logCollection)
        {
            LogCollection = logCollection;
        }

        private readonly Mutex _m = new Mutex(false, $"Logger{Environment.ProcessorCount}{DateTime.Now.Ticks}");

        private void Log(string message, string type, List<string> categories = null, StackTrace st = null, Exception ex = null)
        {
            _m.WaitOne(10000);

            var item = new LogItem
            {
                Message = message,
                Type = type,
                CurrentThread = Thread.CurrentThread,
                StackInfo = st ?? new StackTrace(),
                LoggedAt = DateTime.Now.TimeOfDay.ToString()
            };

            item.Category.Add(type);
            item.Category.Add("Any");
            categories?.ForEach(x => item.Category.Add(x));

            Utils.InvokeDispatcherIfRequired(() =>
            {
                LogCollection.Add(item);
            });
            
            _m.ReleaseMutex();
        }

        public void Error(string msg, Exception ex = null) => Error(msg, null, ex);
        public void Error(Tuple<string, List<string>> tArgs, Exception ex = null) => Error(tArgs.Item1, tArgs.Item2, ex);

        public void Error(string msg, List<string> categories, Exception ex) => Log(msg, "Error", categories,
            (ex == null) ? new StackTrace() : new StackTrace(ex), ex);

        public void Warning(string msg) => Warning(msg, null);
        public void Warning(Tuple<string, List<string>> tArgs) => Warning(tArgs.Item1, tArgs.Item2);
        public void Warning(string msg, List<string> categories) => Log(msg, "Warning", categories);

        public void Info(string msg) => Info(new Tuple<string, List<string>>(msg, null));
        public void Info(Tuple<string, List<string>> tArgs) => Info(tArgs.Item1, tArgs.Item2);
        public void Info(string msg, List<string> categories) => Log(msg, "Info", categories);
    }
}
