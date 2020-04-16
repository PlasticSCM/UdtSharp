using System.Diagnostics;
using System.Threading;

namespace UdtSharp
{
    public class Timer
    {
        ulong m_ullSchedTime;             // next schedulled time
        static ulong s_ullCPUFrequency = (ulong)(Stopwatch.Frequency / 1000000L);// CPU frequency : clock cycles per microsecond

        EventWaitHandle m_TickCond = new EventWaitHandle(false, EventResetMode.AutoReset);
        object m_TickLock = new object();

        static EventWaitHandle m_EventCond = new EventWaitHandle(false, EventResetMode.AutoReset);
        static object m_EventLock = new object();

        public Timer()
        {

        }

        public void Stop()
        {
            m_TickCond.Close();
        }

        public static ulong getCPUFrequency()
        {
            return s_ullCPUFrequency;
        }

        void sleep(ulong interval)
        {
            ulong t = getTime();

            // sleep next "interval" time
            sleepto(t + interval);
        }

        public void sleepto(ulong nexttime)
        {
            // Use class member such that the method can be interrupted by others
            m_ullSchedTime = nexttime;

            ulong t = getTime();

            while (t < m_ullSchedTime)
            {
                m_TickCond.WaitOne(1);

                t = getTime();
            }
        }

        public void interrupt()
        {
            // schedule the sleepto time to the current CCs, so that it will stop
            m_ullSchedTime = getTime();
            tick();
        }

        public void tick()
        {
            m_TickCond.Set();
        }

        public static ulong getTime()
        {
            long ticks = Stopwatch.GetTimestamp();
            ulong microseconds = (ulong)(ticks * 1000000 / Stopwatch.Frequency);
            return microseconds;
        }

        public static void triggerEvent()
        {
            m_EventCond.Set();
        }

        static void waitForEvent()
        {
            m_EventCond.WaitOne(1);
        }

        static void sleep()
        {
            Thread.Sleep(1);
        }
    }
}