namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class StartupState : UpdatesState
    {
        public const int MaxBuildAgeForUpdateChecking = 0x447;

        public StartupState() : base(false, false, MarqueeStyle.Marquee)
        {
        }

        private static void DeleteUpdateMsi()
        {
            string fileName = Path.GetFileName(Settings.CurrentUser.GetString("UpdateMsiFileName", null));
            string extension = Path.GetExtension(fileName);
            string dirPath = Environment.ExpandEnvironmentVariables(@"%TEMP%\PdnSetup");
            foreach (string str5 in new string[] { "UpdateMonitor.exe", "UpdateMonitor.exe.config" })
            {
                FileSystem.TryDeleteFile(dirPath, str5);
            }
            if ((fileName != null) && ((string.Compare(".msi", extension, true, CultureInfo.InvariantCulture) == 0) || (string.Compare(".exe", extension, true, CultureInfo.InvariantCulture) == 0)))
            {
                string filePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), fileName);
                for (int i = 3; i > 0; i--)
                {
                    if (FileSystem.TryDeleteFile(filePath))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                Settings.CurrentUser.TryDelete("UpdateMsiFileName");
            }
            if (Directory.Exists(dirPath))
            {
                FileSystem.TryDeleteDirectory(dirPath);
            }
        }

        public override void OnEnteredState()
        {
            DeleteUpdateMsi();
            if ((Security.IsAdministrator || Security.CanElevateToAdministrator) && ShouldCheckForUpdates())
            {
                PingLastUpdateCheckTime();
                base.StateMachine.QueueInput(PrivateInput.GoToChecking);
            }
            else
            {
                base.StateMachine.QueueInput(UpdatesAction.Continue);
            }
        }

        public static void PingLastUpdateCheckTime()
        {
            Settings.CurrentUser.SetString("LastUpdateCheckTimeTicks", DateTime.Now.Ticks.ToString());
        }

        public override void ProcessInput(object input, out PaintDotNet.State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new ReadyToCheckState();
            }
            else
            {
                if (!input.Equals(PrivateInput.GoToChecking))
                {
                    throw new ArgumentException();
                }
                newState = new CheckingState();
            }
        }

        public static bool ShouldCheckForUpdates()
        {
            bool flag;
            if (PdnInfo.IsTestMode)
            {
                bool isUpdateRebootPending = OS.IsUpdateRebootPending;
                return true;
            }
            if (PdnInfo.IsDebugBuild && OS.IsUpdateRebootPending)
            {
                return false;
            }
            bool flag2 = "1" == Settings.SystemWide.GetString("CHECKFORUPDATES", "0");
            TimeSpan span = new TimeSpan(MinBuildAgeForUpdateChecking, 0, 0, 0);
            TimeSpan span2 = new TimeSpan(0x447, 0, 0, 0);
            TimeSpan span3 = (TimeSpan) (DateTime.Now - PdnInfo.BuildTime);
            if ((span3 < span) || (span3 > span2))
            {
                flag = false;
            }
            else if (flag2)
            {
                try
                {
                    string s = Settings.CurrentUser.GetString("LastUpdateCheckTimeTicks", null);
                    if (s == null)
                    {
                        flag = true;
                    }
                    else
                    {
                        long ticks = long.Parse(s);
                        DateTime time = new DateTime(ticks);
                        TimeSpan span4 = (TimeSpan) (DateTime.Now - time);
                        flag = span4 > new TimeSpan(UpdateCheckIntervalDays, 0, 0, 0);
                    }
                }
                catch (Exception)
                {
                    flag = true;
                }
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                flag &= !OS.IsUpdateRebootPending;
            }
            return flag;
        }

        public static int MinBuildAgeForUpdateChecking
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 7;
                }
                return 0;
            }
        }

        public static int UpdateCheckIntervalDays
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 10;
                }
                return 1;
            }
        }
    }
}

