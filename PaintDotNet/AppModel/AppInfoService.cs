namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using System;

    internal sealed class AppInfoService : MarshalByRefObject, IAppInfoService
    {
        public Version AppVersion =>
            PdnInfo.Version;

        public string InstallDirectory =>
            PdnInfo.ApplicationDir2;

        public string UserDataDirectory =>
            PdnInfo.UserDataPath3;
    }
}

