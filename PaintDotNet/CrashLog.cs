namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    internal static class CrashLog
    {
        public static string GetCrashLogHeader(DateTime appStartupTime)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter stream = new StringWriter(sb);
            WriteCrashLog(null, stream, appStartupTime);
            return sb.ToString();
        }

        public static void WriteCrashLog(Exception crashEx, TextWriter stream, DateTime appStartupTime)
        {
            string str;
            string str2;
            int expressionStack_B84_0;
            string[] expressionStack_B84_1;
            TextWriter expressionStack_B84_2;
            string expressionStack_B97_0;
            int expressionStack_B97_1;
            string[] expressionStack_B97_2;
            TextWriter expressionStack_B97_3;
            try
            {
                str = PdnResources.GetString2("CrashLog.HeaderText.Format");
            }
            catch (Exception exception)
            {
                str = "This text file was created because Paint.NET crashed.\r\nPlease e-mail this file to {0} so we can diagnose and fix the problem.\r\n, --- Exception while calling PdnResources.GetString(\"CrashLog.HeaderText.Format\"): " + exception.ToString() + Environment.NewLine;
            }
            try
            {
                str2 = string.Format(str, "crashlog@getpaint.net");
            }
            catch
            {
                str2 = string.Empty;
            }
            stream.WriteLine(str2);
            string fullAppName = "err";
            string str4 = "err";
            string str5 = "err";
            string currentDirectory = "err";
            string str7 = "err";
            string str8 = "err";
            string revision = "err";
            string str10 = "err";
            string str11 = "err";
            string str12 = "err";
            string str13 = "err";
            string str14 = "err";
            string cpuName = "err";
            string str16 = "err";
            string str17 = "err";
            string str18 = "err";
            string str19 = "err";
            string str20 = "err";
            string str21 = "err";
            string str22 = "err";
            string str23 = "err";
            string str24 = "err";
            string str25 = "err";
            string str26 = "err";
            try
            {
                try
                {
                    fullAppName = PdnInfo.FullAppName;
                }
                catch (Exception exception2)
                {
                    fullAppName = Application.ProductVersion + ", --- Exception while calling PdnInfo.GetFullAppName(): " + exception2.ToString() + Environment.NewLine;
                }
                try
                {
                    str4 = DateTime.Now.ToString();
                }
                catch (Exception exception3)
                {
                    str4 = "--- Exception while populating timeOfCrash: " + exception3.ToString() + Environment.NewLine;
                }
                try
                {
                    str5 = ((TimeSpan) (DateTime.Now - appStartupTime)).ToString();
                }
                catch (Exception exception4)
                {
                    str5 = "--- Exception while populating appUptime: " + exception4.ToString() + Environment.NewLine;
                }
                try
                {
                    currentDirectory = Environment.CurrentDirectory;
                }
                catch (Exception exception5)
                {
                    currentDirectory = "--- Exception while populating currentDir: " + exception5.ToString() + Environment.NewLine;
                }
                try
                {
                    str7 = Settings.SystemWide.GetString("TARGETDIR", "n/a");
                }
                catch (Exception exception6)
                {
                    str7 = "--- Exception while populating targetDir: " + exception6.ToString() + Environment.NewLine;
                }
                try
                {
                    str8 = Environment.OSVersion.Version.ToString();
                }
                catch (Exception exception7)
                {
                    str8 = "--- Exception while populating osVersion: " + exception7.ToString() + Environment.NewLine;
                }
                try
                {
                    revision = OS.Revision;
                }
                catch (Exception exception8)
                {
                    revision = "--- Exception while populating osRevision: " + exception8.ToString() + Environment.NewLine;
                }
                try
                {
                    str10 = OS.OSType.ToString();
                }
                catch (Exception exception9)
                {
                    str10 = "--- Exception while populating osType: " + exception9.ToString() + Environment.NewLine;
                }
                try
                {
                    str11 = Processor.NativeArchitecture.ToString().ToLower();
                }
                catch (Exception exception10)
                {
                    str11 = "--- Exception while populating processorNativeArchitecture: " + exception10.ToString() + Environment.NewLine;
                }
                try
                {
                    str12 = Environment.Version.ToString();
                }
                catch (Exception exception11)
                {
                    str12 = "--- Exception while populating clrVersion: " + exception11.ToString() + Environment.NewLine;
                }
                try
                {
                    string str27;
                    string str28;
                    string str29;
                    string str30;
                    int expressionStack_2E9_0;
                    string expressionStack_2FB_0;
                    string expressionStack_2F4_0;
                    string expressionStack_30B_0;
                    string expressionStack_30B_1;
                    string expressionStack_304_0;
                    string expressionStack_304_1;
                    string expressionStack_31B_0;
                    string expressionStack_31B_1;
                    string expressionStack_31B_2;
                    string expressionStack_314_0;
                    string expressionStack_314_1;
                    string expressionStack_314_2;
                    int expressionStack_364_0;
                    string expressionStack_376_0;
                    string expressionStack_36F_0;
                    string expressionStack_386_0;
                    string expressionStack_386_1;
                    string expressionStack_37F_0;
                    string expressionStack_37F_1;
                    string expressionStack_396_0;
                    string expressionStack_396_1;
                    string expressionStack_396_2;
                    string expressionStack_38F_0;
                    string expressionStack_38F_1;
                    string expressionStack_38F_2;
                    int expressionStack_3DF_0;
                    string expressionStack_3F1_0;
                    string expressionStack_3EA_0;
                    string expressionStack_401_0;
                    string expressionStack_401_1;
                    string expressionStack_3FA_0;
                    string expressionStack_3FA_1;
                    string expressionStack_411_0;
                    string expressionStack_411_1;
                    string expressionStack_411_2;
                    string expressionStack_40A_0;
                    string expressionStack_40A_1;
                    string expressionStack_40A_2;
                    int expressionStack_469_0;
                    string expressionStack_47B_0;
                    string expressionStack_474_0;
                    string expressionStack_48B_0;
                    string expressionStack_48B_1;
                    string expressionStack_484_0;
                    string expressionStack_484_1;
                    string expressionStack_49B_0;
                    string expressionStack_49B_1;
                    string expressionStack_49B_2;
                    string expressionStack_494_0;
                    string expressionStack_494_1;
                    string expressionStack_494_2;
                    string expressionStack_4AB_0;
                    string expressionStack_4AB_1;
                    string expressionStack_4AB_2;
                    string expressionStack_4AB_3;
                    string expressionStack_4A4_0;
                    string expressionStack_4A4_1;
                    string expressionStack_4A4_2;
                    string expressionStack_4A4_3;
                    string expressionStack_4D3_0;
                    string expressionStack_4DE_0;
                    string expressionStack_4D7_0;
                    string expressionStack_4EB_0;
                    string expressionStack_4EB_1;
                    string expressionStack_4E4_0;
                    string expressionStack_4E4_1;
                    string expressionStack_4F8_0;
                    string expressionStack_4F8_1;
                    string expressionStack_4F8_2;
                    string expressionStack_4F1_0;
                    string expressionStack_4F1_1;
                    string expressionStack_4F1_2;
                    bool flag = OS.VerifyFrameworkVersion(2, 0, 0, false);
                    bool flag2 = OS.VerifyFrameworkVersion(2, 0, 1, false);
                    bool flag3 = OS.VerifyFrameworkVersion(2, 0, 2, false);
                    if (!flag && !flag2)
                    {
                        expressionStack_2E9_0 = (int) flag3;
                    }
                    else
                    {
                        expressionStack_2E9_0 = 1;
                    }
                    bool flag4 = (bool) expressionStack_2E9_0;
                    if (flag)
                    {
                        expressionStack_2FB_0 = "2.0 (";
                        goto Label_02FB;
                    }
                    else
                    {
                        expressionStack_2F4_0 = "2.0 (";
                    }
                    string expressionStack_300_1 = expressionStack_2F4_0;
                    string expressionStack_300_0 = "";
                    goto Label_0300;
                Label_02FB:
                    expressionStack_300_1 = expressionStack_2FB_0;
                    expressionStack_300_0 = "rtm ";
                Label_0300:
                    if (flag2)
                    {
                        expressionStack_30B_1 = expressionStack_300_1;
                        expressionStack_30B_0 = expressionStack_300_0;
                        goto Label_030B;
                    }
                    else
                    {
                        expressionStack_304_1 = expressionStack_300_1;
                        expressionStack_304_0 = expressionStack_300_0;
                    }
                    string expressionStack_310_2 = expressionStack_304_1;
                    string expressionStack_310_1 = expressionStack_304_0;
                    string expressionStack_310_0 = "";
                    goto Label_0310;
                Label_030B:
                    expressionStack_310_2 = expressionStack_30B_1;
                    expressionStack_310_1 = expressionStack_30B_0;
                    expressionStack_310_0 = "sp1 ";
                Label_0310:
                    if (flag3)
                    {
                        expressionStack_31B_2 = expressionStack_310_2;
                        expressionStack_31B_1 = expressionStack_310_1;
                        expressionStack_31B_0 = expressionStack_310_0;
                        goto Label_031B;
                    }
                    else
                    {
                        expressionStack_314_2 = expressionStack_310_2;
                        expressionStack_314_1 = expressionStack_310_1;
                        expressionStack_314_0 = expressionStack_310_0;
                    }
                    string expressionStack_320_3 = expressionStack_314_2;
                    string expressionStack_320_2 = expressionStack_314_1;
                    string expressionStack_320_1 = expressionStack_314_0;
                    string expressionStack_320_0 = "";
                    goto Label_0320;
                Label_031B:
                    expressionStack_320_3 = expressionStack_31B_2;
                    expressionStack_320_2 = expressionStack_31B_1;
                    expressionStack_320_1 = expressionStack_31B_0;
                    expressionStack_320_0 = "sp2 ";
                Label_0320:
                    str27 = expressionStack_320_3 + (expressionStack_320_2 + expressionStack_320_1 + expressionStack_320_0).Trim() + ") ";
                    bool flag5 = OS.VerifyFrameworkVersion(3, 0, 0, false);
                    bool flag6 = OS.VerifyFrameworkVersion(3, 0, 1, false);
                    bool flag7 = OS.VerifyFrameworkVersion(3, 0, 2, false);
                    if (!flag5 && !flag6)
                    {
                        expressionStack_364_0 = (int) flag7;
                    }
                    else
                    {
                        expressionStack_364_0 = 1;
                    }
                    bool flag8 = (bool) expressionStack_364_0;
                    if (flag5)
                    {
                        expressionStack_376_0 = "3.0 (";
                        goto Label_0376;
                    }
                    else
                    {
                        expressionStack_36F_0 = "3.0 (";
                    }
                    string expressionStack_37B_1 = expressionStack_36F_0;
                    string expressionStack_37B_0 = "";
                    goto Label_037B;
                Label_0376:
                    expressionStack_37B_1 = expressionStack_376_0;
                    expressionStack_37B_0 = "rtm ";
                Label_037B:
                    if (flag6)
                    {
                        expressionStack_386_1 = expressionStack_37B_1;
                        expressionStack_386_0 = expressionStack_37B_0;
                        goto Label_0386;
                    }
                    else
                    {
                        expressionStack_37F_1 = expressionStack_37B_1;
                        expressionStack_37F_0 = expressionStack_37B_0;
                    }
                    string expressionStack_38B_2 = expressionStack_37F_1;
                    string expressionStack_38B_1 = expressionStack_37F_0;
                    string expressionStack_38B_0 = "";
                    goto Label_038B;
                Label_0386:
                    expressionStack_38B_2 = expressionStack_386_1;
                    expressionStack_38B_1 = expressionStack_386_0;
                    expressionStack_38B_0 = "sp1 ";
                Label_038B:
                    if (flag7)
                    {
                        expressionStack_396_2 = expressionStack_38B_2;
                        expressionStack_396_1 = expressionStack_38B_1;
                        expressionStack_396_0 = expressionStack_38B_0;
                        goto Label_0396;
                    }
                    else
                    {
                        expressionStack_38F_2 = expressionStack_38B_2;
                        expressionStack_38F_1 = expressionStack_38B_1;
                        expressionStack_38F_0 = expressionStack_38B_0;
                    }
                    string expressionStack_39B_3 = expressionStack_38F_2;
                    string expressionStack_39B_2 = expressionStack_38F_1;
                    string expressionStack_39B_1 = expressionStack_38F_0;
                    string expressionStack_39B_0 = "";
                    goto Label_039B;
                Label_0396:
                    expressionStack_39B_3 = expressionStack_396_2;
                    expressionStack_39B_2 = expressionStack_396_1;
                    expressionStack_39B_1 = expressionStack_396_0;
                    expressionStack_39B_0 = "sp2 ";
                Label_039B:
                    str28 = expressionStack_39B_3 + (expressionStack_39B_2 + expressionStack_39B_1 + expressionStack_39B_0).Trim() + ") ";
                    bool flag9 = OS.VerifyFrameworkVersion(3, 5, 0, false);
                    bool flag10 = OS.VerifyFrameworkVersion(3, 5, 1, false);
                    bool flag11 = OS.VerifyFrameworkVersion(3, 5, 1, true);
                    if (!flag9 && !flag10)
                    {
                        expressionStack_3DF_0 = (int) flag11;
                    }
                    else
                    {
                        expressionStack_3DF_0 = 1;
                    }
                    bool flag12 = (bool) expressionStack_3DF_0;
                    if (flag9)
                    {
                        expressionStack_3F1_0 = "3.5 (";
                        goto Label_03F1;
                    }
                    else
                    {
                        expressionStack_3EA_0 = "3.5 (";
                    }
                    string expressionStack_3F6_1 = expressionStack_3EA_0;
                    string expressionStack_3F6_0 = "";
                    goto Label_03F6;
                Label_03F1:
                    expressionStack_3F6_1 = expressionStack_3F1_0;
                    expressionStack_3F6_0 = "rtm ";
                Label_03F6:
                    if (flag10)
                    {
                        expressionStack_401_1 = expressionStack_3F6_1;
                        expressionStack_401_0 = expressionStack_3F6_0;
                        goto Label_0401;
                    }
                    else
                    {
                        expressionStack_3FA_1 = expressionStack_3F6_1;
                        expressionStack_3FA_0 = expressionStack_3F6_0;
                    }
                    string expressionStack_406_2 = expressionStack_3FA_1;
                    string expressionStack_406_1 = expressionStack_3FA_0;
                    string expressionStack_406_0 = "";
                    goto Label_0406;
                Label_0401:
                    expressionStack_406_2 = expressionStack_401_1;
                    expressionStack_406_1 = expressionStack_401_0;
                    expressionStack_406_0 = "sp1 ";
                Label_0406:
                    if (flag11)
                    {
                        expressionStack_411_2 = expressionStack_406_2;
                        expressionStack_411_1 = expressionStack_406_1;
                        expressionStack_411_0 = expressionStack_406_0;
                        goto Label_0411;
                    }
                    else
                    {
                        expressionStack_40A_2 = expressionStack_406_2;
                        expressionStack_40A_1 = expressionStack_406_1;
                        expressionStack_40A_0 = expressionStack_406_0;
                    }
                    string expressionStack_416_3 = expressionStack_40A_2;
                    string expressionStack_416_2 = expressionStack_40A_1;
                    string expressionStack_416_1 = expressionStack_40A_0;
                    string expressionStack_416_0 = "";
                    goto Label_0416;
                Label_0411:
                    expressionStack_416_3 = expressionStack_411_2;
                    expressionStack_416_2 = expressionStack_411_1;
                    expressionStack_416_1 = expressionStack_411_0;
                    expressionStack_416_0 = "sp1_CP ";
                Label_0416:
                    str29 = expressionStack_416_3 + (expressionStack_416_2 + expressionStack_416_1 + expressionStack_416_0).Trim() + ") ";
                    bool flag13 = OS.VerifyFrameworkVersion(4, 0, 0, OS.FrameworkProfile.Client);
                    bool flag14 = OS.VerifyFrameworkVersion(4, 0, 0, OS.FrameworkProfile.Full);
                    bool flag15 = OS.VerifyFrameworkVersion(4, 0, 1, OS.FrameworkProfile.Client);
                    bool flag16 = OS.VerifyFrameworkVersion(4, 0, 1, OS.FrameworkProfile.Full);
                    if ((!flag13 && !flag14) && !flag15)
                    {
                        expressionStack_469_0 = (int) flag16;
                    }
                    else
                    {
                        expressionStack_469_0 = 1;
                    }
                    bool flag17 = (bool) expressionStack_469_0;
                    if (flag13)
                    {
                        expressionStack_47B_0 = "4.0 (";
                        goto Label_047B;
                    }
                    else
                    {
                        expressionStack_474_0 = "4.0 (";
                    }
                    string expressionStack_480_1 = expressionStack_474_0;
                    string expressionStack_480_0 = "";
                    goto Label_0480;
                Label_047B:
                    expressionStack_480_1 = expressionStack_47B_0;
                    expressionStack_480_0 = "rtm ";
                Label_0480:
                    if (flag14)
                    {
                        expressionStack_48B_1 = expressionStack_480_1;
                        expressionStack_48B_0 = expressionStack_480_0;
                        goto Label_048B;
                    }
                    else
                    {
                        expressionStack_484_1 = expressionStack_480_1;
                        expressionStack_484_0 = expressionStack_480_0;
                    }
                    string expressionStack_490_2 = expressionStack_484_1;
                    string expressionStack_490_1 = expressionStack_484_0;
                    string expressionStack_490_0 = "";
                    goto Label_0490;
                Label_048B:
                    expressionStack_490_2 = expressionStack_48B_1;
                    expressionStack_490_1 = expressionStack_48B_0;
                    expressionStack_490_0 = "rtmEx ";
                Label_0490:
                    if (flag15)
                    {
                        expressionStack_49B_2 = expressionStack_490_2;
                        expressionStack_49B_1 = expressionStack_490_1;
                        expressionStack_49B_0 = expressionStack_490_0;
                        goto Label_049B;
                    }
                    else
                    {
                        expressionStack_494_2 = expressionStack_490_2;
                        expressionStack_494_1 = expressionStack_490_1;
                        expressionStack_494_0 = expressionStack_490_0;
                    }
                    string expressionStack_4A0_3 = expressionStack_494_2;
                    string expressionStack_4A0_2 = expressionStack_494_1;
                    string expressionStack_4A0_1 = expressionStack_494_0;
                    string expressionStack_4A0_0 = "";
                    goto Label_04A0;
                Label_049B:
                    expressionStack_4A0_3 = expressionStack_49B_2;
                    expressionStack_4A0_2 = expressionStack_49B_1;
                    expressionStack_4A0_1 = expressionStack_49B_0;
                    expressionStack_4A0_0 = "sp1 ";
                Label_04A0:
                    if (flag16)
                    {
                        expressionStack_4AB_3 = expressionStack_4A0_3;
                        expressionStack_4AB_2 = expressionStack_4A0_2;
                        expressionStack_4AB_1 = expressionStack_4A0_1;
                        expressionStack_4AB_0 = expressionStack_4A0_0;
                        goto Label_04AB;
                    }
                    else
                    {
                        expressionStack_4A4_3 = expressionStack_4A0_3;
                        expressionStack_4A4_2 = expressionStack_4A0_2;
                        expressionStack_4A4_1 = expressionStack_4A0_1;
                        expressionStack_4A4_0 = expressionStack_4A0_0;
                    }
                    string expressionStack_4B0_4 = expressionStack_4A4_3;
                    string expressionStack_4B0_3 = expressionStack_4A4_2;
                    string expressionStack_4B0_2 = expressionStack_4A4_1;
                    string expressionStack_4B0_1 = expressionStack_4A4_0;
                    string expressionStack_4B0_0 = "";
                    goto Label_04B0;
                Label_04AB:
                    expressionStack_4B0_4 = expressionStack_4AB_3;
                    expressionStack_4B0_3 = expressionStack_4AB_2;
                    expressionStack_4B0_2 = expressionStack_4AB_1;
                    expressionStack_4B0_1 = expressionStack_4AB_0;
                    expressionStack_4B0_0 = "sp1Ex ";
                Label_04B0:
                    str30 = expressionStack_4B0_4 + (expressionStack_4B0_3 + expressionStack_4B0_2 + expressionStack_4B0_1 + expressionStack_4B0_0).Trim() + ") ";
                    if (!flag4)
                    {
                        expressionStack_4D3_0 = string.Empty;
                    }
                    else
                    {
                        expressionStack_4D3_0 = str27;
                    }
                    if (flag8)
                    {
                        expressionStack_4DE_0 = expressionStack_4D3_0;
                        goto Label_04DE;
                    }
                    else
                    {
                        expressionStack_4D7_0 = expressionStack_4D3_0;
                    }
                    string expressionStack_4E0_1 = expressionStack_4D7_0;
                    string expressionStack_4E0_0 = string.Empty;
                    goto Label_04E0;
                Label_04DE:
                    expressionStack_4E0_1 = expressionStack_4DE_0;
                    expressionStack_4E0_0 = str28;
                Label_04E0:
                    if (flag12)
                    {
                        expressionStack_4EB_1 = expressionStack_4E0_1;
                        expressionStack_4EB_0 = expressionStack_4E0_0;
                        goto Label_04EB;
                    }
                    else
                    {
                        expressionStack_4E4_1 = expressionStack_4E0_1;
                        expressionStack_4E4_0 = expressionStack_4E0_0;
                    }
                    string expressionStack_4ED_2 = expressionStack_4E4_1;
                    string expressionStack_4ED_1 = expressionStack_4E4_0;
                    string expressionStack_4ED_0 = string.Empty;
                    goto Label_04ED;
                Label_04EB:
                    expressionStack_4ED_2 = expressionStack_4EB_1;
                    expressionStack_4ED_1 = expressionStack_4EB_0;
                    expressionStack_4ED_0 = str29;
                Label_04ED:
                    if (flag17)
                    {
                        expressionStack_4F8_2 = expressionStack_4ED_2;
                        expressionStack_4F8_1 = expressionStack_4ED_1;
                        expressionStack_4F8_0 = expressionStack_4ED_0;
                        goto Label_04F8;
                    }
                    else
                    {
                        expressionStack_4F1_2 = expressionStack_4ED_2;
                        expressionStack_4F1_1 = expressionStack_4ED_1;
                        expressionStack_4F1_0 = expressionStack_4ED_0;
                    }
                    string expressionStack_4FA_3 = expressionStack_4F1_2;
                    string expressionStack_4FA_2 = expressionStack_4F1_1;
                    string expressionStack_4FA_1 = expressionStack_4F1_0;
                    string expressionStack_4FA_0 = string.Empty;
                    goto Label_04FA;
                Label_04F8:
                    expressionStack_4FA_3 = expressionStack_4F8_2;
                    expressionStack_4FA_2 = expressionStack_4F8_1;
                    expressionStack_4FA_1 = expressionStack_4F8_0;
                    expressionStack_4FA_0 = str30;
                Label_04FA:
                    str13 = (expressionStack_4FA_3 + expressionStack_4FA_2 + expressionStack_4FA_1 + expressionStack_4FA_0).Trim();
                }
                catch (Exception exception12)
                {
                    str13 = "--- Exception while populating fxInventory: " + exception12.ToString() + Environment.NewLine;
                }
                try
                {
                    str14 = Processor.Architecture.ToString().ToLower();
                }
                catch (Exception exception13)
                {
                    str14 = "--- Exception while populating processorArchitecture: " + exception13.ToString() + Environment.NewLine;
                }
                try
                {
                    cpuName = Processor.CpuName;
                }
                catch (Exception exception14)
                {
                    cpuName = "--- Exception while populating cpuName: " + exception14.ToString() + Environment.NewLine;
                }
                try
                {
                    str16 = Processor.LogicalCpuCount.ToString() + "x";
                }
                catch (Exception exception15)
                {
                    str16 = "--- Exception while populating cpuCount: " + exception15.ToString() + Environment.NewLine;
                }
                try
                {
                    str17 = "@ ~" + Processor.ApproximateSpeedMhz.ToString() + "MHz";
                }
                catch (Exception exception16)
                {
                    str17 = "--- Exception while populating cpuSpeed: " + exception16.ToString() + Environment.NewLine;
                }
                try
                {
                    str18 = string.Empty;
                    string[] names = Enum.GetNames(typeof(ProcessorFeature));
                    bool flag18 = true;
                    for (int i = 0; i < names.Length; i++)
                    {
                        string str31 = names[i];
                        ProcessorFeature feature = (ProcessorFeature) Enum.Parse(typeof(ProcessorFeature), str31);
                        if (Processor.IsFeaturePresent(feature))
                        {
                            if (flag18)
                            {
                                str18 = "(";
                                flag18 = false;
                            }
                            else
                            {
                                str18 = str18 + ", ";
                            }
                            str18 = str18 + str31;
                        }
                    }
                    if (str18.Length > 0)
                    {
                        str18 = str18 + ")";
                    }
                }
                catch (Exception exception17)
                {
                    str18 = "--- Exception while populating cpuFeatures: " + exception17.ToString() + Environment.NewLine;
                }
                try
                {
                    str19 = $"0x{Processor.FPStatus.ToString("X")}";
                }
                catch (Exception exception18)
                {
                    str19 = "--- Exception while populating cpuFPStatus: " + exception18.ToString() + Environment.NewLine;
                }
                try
                {
                    str20 = ((Memory.TotalPhysicalBytes / ((ulong) 0x400L)) / ((ulong) 0x400L)) + " MB";
                }
                catch (Exception exception19)
                {
                    str20 = "--- Exception while populating totalPhysicalBytes: " + exception19.ToString() + Environment.NewLine;
                }
                try
                {
                    float xScaleFactor = UI.GetXScaleFactor();
                    str21 = $"{(96f * xScaleFactor).ToString("F2")} dpi ({xScaleFactor.ToString("F2")}x scale)";
                }
                catch (Exception exception20)
                {
                    str21 = "--- Exception while populating dpiInfo: " + exception20.ToString() + Environment.NewLine;
                }
                try
                {
                    int expressionStack_7D0_0;
                    object[] expressionStack_7D0_1;
                    string expressionStack_7D0_2;
                    int expressionStack_7C9_0;
                    object[] expressionStack_7C9_1;
                    string expressionStack_7C9_2;
                    VisualStyleClass visualStyleClass = UI.VisualStyleClass;
                    PdnTheme effectiveTheme = ThemeConfig.EffectiveTheme;
                    bool isCompositionEnabled = UI.IsCompositionEnabled;
                    string themeFileName = UI.ThemeFileName;
                    object[] args = new object[4];
                    args[0] = visualStyleClass.ToString();
                    args[1] = effectiveTheme.ToString();
                    if (isCompositionEnabled)
                    {
                        expressionStack_7D0_2 = "{0}/{1}{2} ({3})";
                        expressionStack_7D0_1 = args;
                        expressionStack_7D0_0 = 2;
                        goto Label_07D0;
                    }
                    else
                    {
                        expressionStack_7C9_2 = "{0}/{1}{2} ({3})";
                        expressionStack_7C9_1 = args;
                        expressionStack_7C9_0 = 2;
                    }
                    string format = expressionStack_7C9_2;
                    object[] expressionStack_7D5_2 = expressionStack_7C9_1;
                    int index = expressionStack_7C9_0;
                    string expressionStack_7D5_0 = "";
                    goto Label_07D5;
                Label_07D0:
                    format = expressionStack_7D0_2;
                    expressionStack_7D5_2 = expressionStack_7D0_1;
                    index = expressionStack_7D0_0;
                    expressionStack_7D5_0 = " + DWM";
                Label_07D5:
                    expressionStack_7D5_2[index] = expressionStack_7D5_0;
                    args[3] = themeFileName;
                    str22 = string.Format(format, args);
                }
                catch (Exception exception21)
                {
                    str22 = "--- Exception while populating themeInfo: " + exception21.ToString() + Environment.NewLine;
                }
                try
                {
                    str23 = "pdnr.c: " + PdnResources.Culture.Name + ", hklm: " + Settings.SystemWide.GetString("LanguageName", "n/a") + ", hkcu: " + Settings.CurrentUser.GetString("LanguageName", "n/a") + ", cc: " + CultureInfo.CurrentCulture.Name + ", cuic: " + CultureInfo.CurrentUICulture.Name;
                }
                catch (Exception exception22)
                {
                    str23 = "--- Exception while populating localeName: " + exception22.ToString() + Environment.NewLine;
                }
                try
                {
                    string str34;
                    string expressionStack_93E_0;
                    string expressionStack_91C_0;
                    string expressionStack_943_0;
                    string expressionStack_943_1;
                    string expressionStack_937_0;
                    string expressionStack_92A_0;
                    string expressionStack_92F_1;
                    string str33 = Settings.SystemWide.GetString("CHECKFORUPDATES", "err");
                    try
                    {
                        long ticks = long.Parse(Settings.CurrentUser.Get("LastUpdateCheckTimeTicks"));
                        str34 = new DateTime(ticks).ToShortDateString();
                    }
                    catch (Exception)
                    {
                        str34 = "err";
                    }
                    if (str33 == "1")
                    {
                        expressionStack_93E_0 = "{0}, {1}";
                        goto Label_093E;
                    }
                    else
                    {
                        expressionStack_91C_0 = "{0}, {1}";
                    }
                    if (str33 == "0")
                    {
                        expressionStack_937_0 = expressionStack_91C_0;
                        goto Label_0937;
                    }
                    else
                    {
                        expressionStack_92A_0 = expressionStack_91C_0;
                    }
                    if (str33 != null)
                    {
                        expressionStack_943_1 = expressionStack_92A_0;
                        expressionStack_943_0 = str33;
                        goto Label_0943;
                    }
                    else
                    {
                        expressionStack_92F_1 = expressionStack_92A_0;
                        string expressionStack_92F_0 = str33;
                    }
                    expressionStack_943_1 = expressionStack_92F_1;
                    expressionStack_943_0 = "null";
                    goto Label_0943;
                Label_0937:
                    expressionStack_943_1 = expressionStack_937_0;
                    expressionStack_943_0 = "false";
                    goto Label_0943;
                Label_093E:
                    expressionStack_943_1 = expressionStack_93E_0;
                    expressionStack_943_0 = "true";
                Label_0943:
                    str24 = string.Format(expressionStack_943_1, expressionStack_943_0, str34);
                }
                catch (Exception exception23)
                {
                    str24 = "--- Exception while populating updaterInfo: " + exception23.ToString() + Environment.NewLine;
                }
                try
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        builder.AppendFormat("{0}    {1} @ {2}", Environment.NewLine, assembly.FullName, assembly.Location);
                    }
                    str25 = builder.ToString();
                }
                catch (Exception exception24)
                {
                    str25 = "--- Exception while populating assembliesInfo: " + exception24.ToString() + Environment.NewLine;
                }
                try
                {
                    StringBuilder builder2 = new StringBuilder();
                    int num5 = Processor.Architecture.ToBitness();
                    foreach (ProcessStatus.ModuleFileNameAndBitness bitness in ProcessStatus.GetCurrentProcessModuleNames())
                    {
                        string fileVersion;
                        string str36 = string.Empty;
                        if (bitness.Bitness != num5)
                        {
                            str36 = $" ({bitness.Bitness.ToString()}-bit)";
                        }
                        try
                        {
                            fileVersion = FileVersionInfo.GetVersionInfo(bitness.FileName).FileVersion;
                        }
                        catch (Exception exception25)
                        {
                            fileVersion = $"ex: {exception25.GetType().FullName}";
                        }
                        builder2.AppendFormat("{0}    {1}{2}, version='{3}'", new object[] { Environment.NewLine, bitness.FileName, str36, fileVersion });
                    }
                    str26 = builder2.ToString();
                }
                catch (Exception exception26)
                {
                    str26 = "--- Exception while populating nativeModulesInfo: " + exception26.ToString() + Environment.NewLine;
                }
            }
            catch (Exception exception27)
            {
                stream.WriteLine("Exception while gathering app and system info: " + exception27.ToString());
            }
            stream.WriteLine("Application version: " + fullAppName);
            stream.WriteLine("Time of crash: " + str4);
            stream.WriteLine("Application uptime: " + str5);
            stream.WriteLine("Install directory: " + str7);
            stream.WriteLine("Current directory: " + currentDirectory);
            string[] strArray2 = new string[7];
            strArray2[0] = "OS Version: ";
            strArray2[1] = str8;
            if (string.IsNullOrEmpty(revision))
            {
                TextWriter expressionStack_B92_2 = stream;
                string[] expressionStack_B92_1 = strArray2;
                int expressionStack_B92_0 = 2;
                expressionStack_B97_3 = expressionStack_B92_2;
                expressionStack_B97_2 = expressionStack_B92_1;
                expressionStack_B97_1 = expressionStack_B92_0;
                expressionStack_B97_0 = "";
                goto Label_0B97;
            }
            else
            {
                expressionStack_B84_2 = stream;
                expressionStack_B84_1 = strArray2;
                expressionStack_B84_0 = 2;
            }
            expressionStack_B97_3 = expressionStack_B84_2;
            expressionStack_B97_2 = expressionStack_B84_1;
            expressionStack_B97_1 = expressionStack_B84_0;
            expressionStack_B97_0 = " " + revision;
        Label_0B97:
            expressionStack_B97_2[expressionStack_B97_1] = expressionStack_B97_0;
            strArray2[3] = " ";
            strArray2[4] = str10;
            strArray2[5] = " ";
            strArray2[6] = str11;
            expressionStack_B97_3.WriteLine(string.Concat(strArray2));
            stream.WriteLine(".NET version: CLR " + str12 + " " + str14 + ", FX " + str13);
            stream.WriteLine("Processor: " + str16 + " \"" + cpuName + "\" " + str17 + " " + str18 + ", fps=" + str19);
            stream.WriteLine("Physical memory: " + str20);
            stream.WriteLine("UI DPI: " + str21);
            stream.WriteLine("UI Theme: " + str22);
            stream.WriteLine("Updates: " + str24);
            stream.WriteLine("Locale: " + str23);
            stream.WriteLine("Managed assemblies: " + str25);
            stream.WriteLine("Native modules: " + str26);
            stream.WriteLine();
            stream.WriteLine("Exception details:");
            if (crashEx == null)
            {
                stream.WriteLine("(null)");
            }
            else
            {
                stream.WriteLine(crashEx.ToString());
                Exception[] loaderExceptions = null;
                if (crashEx is ReflectionTypeLoadException)
                {
                    loaderExceptions = ((ReflectionTypeLoadException) crashEx).LoaderExceptions;
                }
                if (loaderExceptions != null)
                {
                    for (int j = 0; j < loaderExceptions.Length; j++)
                    {
                        stream.WriteLine();
                        stream.WriteLine("Secondary exception details:");
                        if (loaderExceptions[j] == null)
                        {
                            stream.WriteLine("(null)");
                        }
                        else
                        {
                            stream.WriteLine(loaderExceptions[j].ToString());
                        }
                    }
                }
            }
            stream.WriteLine("------------------------------------------------------------------------------");
            stream.Flush();
        }
    }
}

