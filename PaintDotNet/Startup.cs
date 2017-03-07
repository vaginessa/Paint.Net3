namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Dialogs;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Updates;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class Startup
    {
        private string[] args;
        private static Startup instance;
        private MainForm mainForm;
        private static DateTime startupTime;
        private static object unhandledExSync = new object();

        private Startup(string[] args)
        {
            this.args = args;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
            Process.GetCurrentProcess().Kill();
        }

        private bool CheckForImportantFiles()
        {
            string[] strArray = new string[] { 
                "ICSharpCode.SharpZipLib.dll", "Interop.WIA.dll", "PaintDotNet.Base.dll", "PaintDotNet.Core.dll", "PaintDotNet.Data.dll", "PaintDotNet.Effects.dll", "PaintDotNet.exe.config", @"Native.x86\PaintDotNet.Native.x86.dll", @"Native.x64\PaintDotNet.Native.x64.dll", "PaintDotNet.Resources.dll", "PaintDotNet.Strings.3.DE.resources", "PaintDotNet.Strings.3.ES.resources", "PaintDotNet.Strings.3.FR.resources", "PaintDotNet.Strings.3.IT.resources", "PaintDotNet.Strings.3.JA.resources", "PaintDotNet.Strings.3.KO.resources",
                "PaintDotNet.Strings.3.PT-BR.resources", "PaintDotNet.Strings.3.RU.resources", "PaintDotNet.Strings.3.resources", "PaintDotNet.Strings.3.ZH-CHS.resources", "PaintDotNet.SystemLayer.dll", "PaintDotNet.SystemLayer.Native.x86.dll", "PaintDotNet.SystemLayer.Native.x64.dll", "SetupNgen.exe", "SetupNgen.exe.config", "ShellExtension_x64.dll", "ShellExtension_x86.dll", "UpdateMonitor.exe", "UpdateMonitor.exe.config", "WiaProxy32.exe", "WiaProxy32.exe.config"
            };
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> items = null;
            foreach (string str2 in strArray)
            {
                bool flag;
                try
                {
                    FileInfo info = new FileInfo(Path.Combine(directoryName, str2));
                    flag = !info.Exists;
                }
                catch (Exception)
                {
                    flag = true;
                }
                if (flag)
                {
                    if (items == null)
                    {
                        items = new List<string>();
                    }
                    items.Add(str2);
                }
            }
            if (items != null)
            {
                if (Shell.ReplaceMissingFiles(items.ToArrayEx<string>()))
                {
                    return true;
                }
                Process.GetCurrentProcess().Kill();
            }
            return false;
        }

        public static bool CloseApplication()
        {
            List<Form> list = new List<Form>();
            foreach (Form form in Application.OpenForms)
            {
                if (form.Modal && !object.ReferenceEquals(form, instance.mainForm))
                {
                    list.Add(form);
                }
            }
            if (list.Count > 0)
            {
                return false;
            }
            return CloseForm(instance.mainForm);
        }

        private static bool CloseForm(Form form)
        {
            ArrayList list = new ArrayList(Application.OpenForms);
            if (list.IndexOf(form) == -1)
            {
                return false;
            }
            form.Close();
            ArrayList list2 = new ArrayList(Application.OpenForms);
            return (list2.IndexOf(form) == -1);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int index = args.Name.IndexOf("PdnLib", StringComparison.InvariantCultureIgnoreCase);
            Assembly assembly = null;
            if (index == 0)
            {
                assembly = typeof(ColorBgra).Assembly;
            }
            return assembly;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException((Exception) e.ExceptionObject);
            Process.GetCurrentProcess().Kill();
        }

        [STAThread]
        public static int Main(string[] args)
        {
            startupTime = DateTime.Now;
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Startup.CurrentDomain_AssemblyResolve);
                instance = new Startup(args);
                instance.Start();
            }
            catch (Exception exception)
            {
                try
                {
                    UnhandledException(exception);
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception)
                {
                    UI.MessageBox(null, exception.ToString(), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Process.GetCurrentProcess().Kill();
                }
            }
            return 0;
        }

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Startup.CurrentDomain_UnhandledException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, true);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, false);
            Application.ThreadException += new ThreadExceptionEventHandler(Startup.Application_ThreadException);
            Control.CheckForIllegalCrossThreadCalls = true;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            this.StartPart2();
        }

        public static void StartNewInstance(IWin32Window parent, string fileName)
        {
            string str;
            if ((fileName != null) && (fileName.Length != 0))
            {
                str = "\"" + fileName + "\"";
            }
            else
            {
                str = "";
            }
            StartNewInstance(parent, false, new string[] { str });
        }

        public static void StartNewInstance(IWin32Window parent, bool requireAdmin, string[] args)
        {
            string str2;
            StringBuilder builder = new StringBuilder();
            foreach (string str in args)
            {
                builder.Append(' ');
                if (str.IndexOf(' ') != -1)
                {
                    builder.Append('"');
                }
                builder.Append(str);
                if (str.IndexOf(' ') != -1)
                {
                    builder.Append('"');
                }
            }
            if (builder.Length > 0)
            {
                str2 = builder.ToString(1, builder.Length - 1);
            }
            else
            {
                str2 = null;
            }
            Shell.Execute(parent, Application.ExecutablePath, str2, requireAdmin ? ExecutePrivilege.RequireAdmin : ExecutePrivilege.AsInvokerOrAsManifest, ExecuteWaitType.ReturnImmediately);
        }

        private void StartPart2()
        {
            bool flag = false;
            for (int i = 0; i < this.args.Length; i++)
            {
                if (string.Compare(this.args[i], "/skipRepairAttempt", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    flag = true;
                }
            }
            if (!flag && this.CheckForImportantFiles())
            {
                StartNewInstance(null, false, this.args);
            }
            else if (!Shell.VerifyVCRedistsInstalled())
            {
                Shell.ReplaceMissingFiles(new string[] { "Microsoft Visual C++ Redistributable" });
            }
            else
            {
                string name = Settings.CurrentUser.GetString("LanguageName", null);
                if (name == null)
                {
                    name = Settings.SystemWide.GetString("LanguageName", null);
                }
                if (name != null)
                {
                    try
                    {
                        CultureInfo info = new CultureInfo(name, true);
                        Thread.CurrentThread.CurrentUICulture = info;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (((Environment.Version.Major != 4) && !OS.VerifyFrameworkVersion(3, 5, 1, OS.FrameworkProfile.Client)) && !OS.VerifyFrameworkVersion(3, 5, 1, OS.FrameworkProfile.Full))
                {
                    string message = PdnResources.GetString2("Error.FXRequirement");
                    Utility.ErrorBox(null, message);
                    string url = PdnResources.GetString2("FXDownload.URL");
                    try
                    {
                        Shell.LaunchUrl2(null, url);
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    this.StartPart3();
                }
            }
        }

        private void StartPart3()
        {
            string mutexName = (from s in this.args
                where s.StartsWith("/mutexName=", StringComparison.InvariantCultureIgnoreCase)
                select s.Substring("/mutexName=".Length) into s
                where !s.IsNullOrEmpty()
                select s).FirstOrDefault<string>().SelectIfNull<string>("PaintDotNet");
            this.StartPart4(mutexName);
        }

        private void StartPart4(string mutexName)
        {
            if (!Processor.IsFeaturePresent(ProcessorFeature.SSE))
            {
                string message = PdnResources.GetString2("Error.SSERequirement");
                Utility.ErrorBox(null, message);
            }
            else if ((this.args.Length == 1) && (this.args[0] == "/updateOptions"))
            {
                UpdatesOptionsDialog.ShowUpdateOptionsDialog(null, false);
            }
            else
            {
                SingleInstanceManager manager = new SingleInstanceManager(mutexName);
                if (!manager.IsFirstInstance)
                {
                    manager.FocusFirstInstance();
                    foreach (string str2 in this.args)
                    {
                        manager.SendInstanceMessage(str2, 30);
                    }
                    manager.Dispose();
                    manager = null;
                }
                else
                {
                    this.mainForm = new MainForm(this.args);
                    this.mainForm.SingleInstanceManager = manager;
                    manager = null;
                    Application.Run(this.mainForm);
                    try
                    {
                        this.mainForm.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                    this.mainForm = null;
                }
            }
        }

        private static void UnhandledException(Exception ex)
        {
            lock (unhandledExSync)
            {
                string str3;
                using (StreamWriter writer = new StreamWriter(Path.Combine(Shell.GetVirtualPath(VirtualFolderName.UserDesktop, true), "pdncrash.log"), true))
                {
                    writer.AutoFlush = true;
                    CrashLog.WriteCrashLog(ex, writer, startupTime);
                }
                try
                {
                    str3 = PdnResources.GetString2("Startup.UnhandledError.Format");
                }
                catch (Exception)
                {
                    str3 = "There was an unhandled error, and Paint.NET must be closed. Refer to the file '{0}', which has been placed on your desktop, for more information.";
                }
                string message = string.Format(str3, "pdncrash.log");
                Utility.ErrorBox(null, message);
            }
        }
    }
}

