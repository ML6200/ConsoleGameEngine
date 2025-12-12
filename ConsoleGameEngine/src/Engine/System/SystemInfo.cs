using System;

namespace ConsoleGameEngine.Engine.System;

public abstract class SystemInfo
{
    public static class Os
    {
        public static bool IsWindows()
        {
            return OperatingSystem.IsWindows();
        }
        
        public static bool IsMacOsX()
        {
            return OperatingSystem.IsMacOS();
        }

        public static bool IsLinux()
        {
            return OperatingSystem.IsLinux();
        }

        public static bool IsFreeBsd()
        {
            return OperatingSystem.IsFreeBSD();
        }
        
        public static bool IsUnix()
        {
            return OperatingSystem.IsMacOS()
                   || OperatingSystem.IsLinux()
                   || OperatingSystem.IsFreeBSD();
        }
    }
}