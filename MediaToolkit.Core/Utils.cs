﻿using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{

    public abstract class StatCounter
    {
        public abstract string GetReport();
        public abstract void Reset();
    }

    public class Statistic
    {

        public readonly static List<StatCounter> Stats = new List<StatCounter>();

        public static object syncRoot = new object();
        public static void RegisterCounter(StatCounter counter)
        {
            lock (syncRoot)
            {
                Stats.Add(counter);
            }
        }

        public static void UnregisterCounter(StatCounter counter)
        {
            lock (syncRoot)
            {
                Stats.Remove(counter);
            }
        }

        public static string GetReport()
        {
            string report = "";
            lock (syncRoot)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var stat in Stats)
                {
                    sb.AppendLine(stat.GetReport());
                }

                report = sb.ToString();

            }

            return report;
        }

        private static PerfCounter perfCounter = new PerfCounter();
        public static PerfCounter PerfCounter
        {
            get
            {
                if (perfCounter == null)
                {
                    perfCounter = new PerfCounter();
                }
                return perfCounter;
            }
        }
    }




    public class PerfCounter : StatCounter, IDisposable
    {
        public PerfCounter()
        {
            _PerfCounter();
        }

        private void _PerfCounter()
        {
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Disposed += Timer_Disposed;
            timer.Start();

        }

        public short CPU { get; private set; }

        private System.Timers.Timer timer = new System.Timers.Timer();
        private CPUCounter cpuCounter = new CPUCounter();

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.CPU = cpuCounter.GetUsage();
        }

        private void Timer_Disposed(object sender, EventArgs e)
        {
            cpuCounter?.Dispose();
        }

        public override string GetReport()
        {
            string cpuUsage = "";
            if (CPU >= 0 && CPU <= 100)
            {
                cpuUsage = "CPU=" + CPU + "%";
            }
            else
            {
                cpuUsage = "CPU=--%";
            }
            return cpuUsage;
        }

        public override void Reset()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }

        class CPUCounter : IDisposable
        {

            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysKernel;
            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysUser;

            private TimeSpan prevProcTotal;

            private short CPUUsage;
            //DateTime LastRun;

            private long lastTimestamp;

            private long runCount;

            private Process currentProcess;

            public CPUCounter()
            {
                CPUUsage = -1;
                lastTimestamp = 0;

                prevSysUser.dwHighDateTime = prevSysUser.dwLowDateTime = 0;
                prevSysKernel.dwHighDateTime = prevSysKernel.dwLowDateTime = 0;
                prevProcTotal = TimeSpan.MinValue;
                runCount = 0;

                currentProcess = Process.GetCurrentProcess();
            }

            public short GetUsage()
            {
                if (disposed)
                {
                    return 0;
                }

                short CPUCopy = CPUUsage;
                if (Interlocked.Increment(ref runCount) == 1)
                {
                    if (!EnoughTimePassed)
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    System.Runtime.InteropServices.ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                    if (!Kernel32.GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    TimeSpan procTime = currentProcess.TotalProcessorTime;

                    if (prevProcTotal != TimeSpan.MinValue)
                    {
                        ulong sysKernelDiff = SubtractTimes(sysKernel, prevSysKernel);
                        ulong sysUserDiff = SubtractTimes(sysUser, prevSysUser);
                        ulong sysTotal = sysKernelDiff + sysUserDiff;

                        long procTotal = procTime.Ticks - prevProcTotal.Ticks;
                        // long procTotal = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                        if (sysTotal > 0)
                        {
                            CPUUsage = (short)((100.0 * procTotal) / sysTotal);
                        }
                    }
   
                    prevProcTotal = procTime;
                    prevSysKernel = sysKernel;
                    prevSysUser = sysUser;

                    lastTimestamp = Stopwatch.GetTimestamp();

                    CPUCopy = CPUUsage;
                }
                Interlocked.Decrement(ref runCount);

                return CPUCopy;

            }

            private ulong SubtractTimes(System.Runtime.InteropServices.ComTypes.FILETIME a, System.Runtime.InteropServices.ComTypes.FILETIME b)
            {
                ulong aInt = ((ulong)(a.dwHighDateTime << 32)) | (ulong)a.dwLowDateTime;
                ulong bInt = ((ulong)(b.dwHighDateTime << 32)) | (ulong)b.dwLowDateTime;

                return aInt - bInt;
            }

            private bool EnoughTimePassed
            {
                get
                {
                    const int minimumElapsedMS = 250;

                    long ticks = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                    TimeSpan sinceLast = new TimeSpan(ticks);

                    return sinceLast.TotalMilliseconds > minimumElapsedMS;
                }
            }

            private volatile bool disposed = false;
            public void Dispose()
            {
                disposed = true;

                if (currentProcess != null)
                {
                    currentProcess.Dispose();
                    currentProcess = null;
                }
            }
        }

    }

    public class RngProvider
    {
        private static System.Security.Cryptography.RNGCryptoServiceProvider provider =
            new System.Security.Cryptography.RNGCryptoServiceProvider();

        public static uint GetRandomNumber()
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            provider.GetNonZeroBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }


    public class MediaTimer
    {
        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;

        public static double GetRelativeTimeMilliseconds()
        {
            return (Ticks / (double)TicksPerMillisecond);
        }

        public static double GetRelativeTime()
        {
            return (Ticks / (double)TicksPerSecond);
        }

        public static long Ticks
        {
            get
            {
                return (long)(Stopwatch.GetTimestamp() * TicksPerSecond / (double)Stopwatch.Frequency);
                //return DateTime.Now.Ticks;
                //return NativeMethods.timeGetTime() * TicksPerMillisecond;
            }
        }

        public static DateTime GetDateTimeFromNtpTimestamp(ulong ntmTimestamp)
        {
            uint TimestampMSW = (uint)(ntmTimestamp >> 32);
            uint TimestampLSW = (uint)(ntmTimestamp & 0x00000000ffffffff);

            return GetDateTimeFromNtpTimestamp(TimestampMSW, TimestampLSW);
        }

        public static DateTime GetDateTimeFromNtpTimestamp(uint TimestampMSW, uint TimestampLSW)
        {
            /*
            Timestamp, MSW: 3670566484 (0xdac86654)
            Timestamp, LSW: 3876982392 (0xe7160e78)
            [MSW and LSW as NTP timestamp: Apr 25, 2016 09:48:04.902680000 UTC]
             * */

            DateTime ntpDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            uint ntpTimeMilliseconds = (uint)(Math.Round((double)TimestampLSW / (double)uint.MaxValue, 3) * 1000);
            return ntpDateTime
                .AddSeconds(TimestampMSW)
                .AddMilliseconds(ntpTimeMilliseconds);
        }

        private DateTime startDateTime;
        private long startTimestamp;

        private bool isRunning = false;

        public void Start(DateTime dateTime)
        {
            if (isRunning == false)
            {
                startDateTime = dateTime;
                startTimestamp = Stopwatch.GetTimestamp();

                isRunning = true;
            }
        }

        public DateTime Now
        {
            get
            {
                DateTime dateTime = DateTime.MinValue;
                if (isRunning)
                {
                    dateTime = startDateTime.AddTicks(ElapsedTicks);
                }

                return dateTime;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.Zero;
                if (isRunning)
                {
                    timeSpan = new TimeSpan(ElapsedTicks);
                }
                return timeSpan;
            }
        }

        public long ElapsedTicks
        {
            get
            {
                long ticks = 0;
                if (isRunning)
                {
                    ticks = (long)((Stopwatch.GetTimestamp() - startTimestamp) * TicksPerSecond / (double)Stopwatch.Frequency);

                    if (ticks < 0)
                    {
                        Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ticks " + ticks);
                    }
                }
                return ticks;
            }
        }

        public void Stop()
        {

            if (isRunning)
            {
                isRunning = false;
            }

        }
    }

    public class StringHelper
    {
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException("decimalPlaces");
            }

            if (value > long.MinValue && value < long.MaxValue)
            {
                if (value < 0)
                {
                    return "-" + SizeSuffix(-value);
                }

                if (value == 0)
                {
                    return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
                }

                // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
                int mag = (int)Math.Log(value, 1024);

                // 1L << (mag * 10) == 2 ^ (10 * mag) 
                // [i.e. the number of bytes in the unit corresponding to mag]
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                // make adjustment when the value is large enough that
                // it would round up to 1000 or more
                if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
                {
                    mag += 1;
                    adjustedSize /= 1024;
                }

                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            }

            return "";
        }
    }

    public static class TaskEx
    {
        public static Task<Task[]> WhenAllOrFirstException(params Task[] tasks)
        {
            var countdownEvent = new CountdownEvent(tasks.Length);
            var completer = new TaskCompletionSource<Task[]>();

            Action<Task> onCompletion = completed =>
            {
                if (completed.IsFaulted && completed.Exception != null)
                {
                    completer.TrySetException(completed.Exception.InnerExceptions);
                }

                if (countdownEvent.Signal() && !completer.Task.IsCompleted)
                {
                    completer.TrySetResult(tasks);
                }
            };

            foreach (var task in tasks)
            {
                task.ContinueWith(onCompletion);
            }

            return completer.Task;
        }
    }



}
