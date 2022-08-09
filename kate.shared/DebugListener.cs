using kate.shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Timer = System.Threading.Timer;

namespace kate.shared
{
    public delegate void DebugLogDelegate(DebugListener instance);
    public class DebugListener
    {
        public pTraceListener Listener;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        public DebugListener()
        {
            Listener = new pTraceListener(this);
            Initialize();
        }
        internal void Initialize()
        {
            updateTimer = new Timer(onTimerUpdate, autoEvent, 0, 250);

            #if NETFRAMEWORK
            Debug.Listeners.Remove("Default");
            Debug.Listeners.Add(Listener);
            #endif
        }

        private Timer updateTimer;

        public event DebugLogDelegate Updated;
        public void OnUpdate()
        {
            Updated?.Invoke(this);
        }

        public event VoidDelegate LogUpdate = null;
        public void OnLogUpdate()
        {
            if (LogUpdate != null)
            {
                LogUpdate?.Invoke();
            }
        }

        public List<string> LogLines = new List<string>();
        public void onTimerUpdate(Object stateInfo)
        {
            int previousCount = LogLines.Count;
            foreach (var s in Listener.GetPendingMessages())
            {
                LogLines.Add(s);
            }
            if (previousCount != LogLines.Count)
                OnUpdate();
        }

        public virtual bool AllowDebugBreak()
        {
            return false;
        }
    }
    public class pTraceListener : TraceListener
    {
        private DebugListener debugListener;
        public pTraceListener(DebugListener debugListener)
        {
            this.debugListener = debugListener;
        }

        public List<string> pendingLines = new List<string>();

        public override void Write(string message)
        {
            System.Console.WriteLine(message);
            lock (pendingLines)
                pendingLines.Add(message);
        }

        public override void WriteLine(string message)
        {
            System.Console.WriteLine(message);
            lock (pendingLines)
                pendingLines.Add(message);
        }


        public IEnumerable<string> GetPendingMessages()
        {
            lock (pendingLines)
            {
                List<string> pending = new List<string>(pendingLines);
                pendingLines.Clear();
                foreach (var l in pending)
                    Console.WriteLine($"[DEBUG] {l}");
                return pending;
            }
        }


        public override void Fail(string message)
        {
            System.Console.WriteLine(message);
            Fail(message, null);
        }

        public override void Fail(string message, string detailMessage)
        {
            System.Console.WriteLine(message + @"\n" + detailMessage);
            lock (pendingLines)
            {
                pendingLines.Add($@"ASSERT: {message} (hold shift to break)");

                if (detailMessage != null)
                    pendingLines.Add(detailMessage);


                if (Debugger.IsAttached && debugListener.AllowDebugBreak())
                    Debugger.Break();
            }
        }
    }
}

