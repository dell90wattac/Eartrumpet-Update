using System;

namespace EarTrumpet.Extensions
{
    public enum OSVersions : int
    {
        RS3 = 16299,
        RS4 = 17134,
        RS5_1809 = 17763,
        Version19H1 = 18362,
        Version21H2 = 21390,
        Windows11 = 22000,
        // DWMWA_SYSTEMBACKDROP_TYPE, and so Mica, arrived in 22H2. Windows 11
        // 21H2 rounds corners but rejects the backdrop attribute.
        Windows11_22H2 = 22621,
    }

    public static class OperatingSystemExtensions
    {
        public static bool IsAtLeast(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build >= (int)version;
        }

        public static bool IsGreaterThan(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build > (int)version;
        }

        public static bool IsLessThan(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build < (int)version;
        }
    }
}
