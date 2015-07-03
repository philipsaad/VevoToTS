using System;
using System.Diagnostics;
using System.Reflection;

public static class ArgumentHandler
{
    public static void showUsage()
    {
        Console.WriteLine("Losslessly downloads a VEVO music video and outputs the result to a local Transport Stream.", System.Diagnostics.Process.GetCurrentProcess().ProcessName, Assembly.GetExecutingAssembly().GetName().Version.ToString());
        Console.WriteLine("");
        Console.WriteLine("{0} [/?] [/v]", AppDomain.CurrentDomain.FriendlyName);
        Console.WriteLine("");
        Console.WriteLine("  /?           Displays this usage information.");
        Console.WriteLine("  /v           Displays version information.");
    }

    public static void showVersion()
    {
        Console.WriteLine("{0} v{1}", Process.GetCurrentProcess().ProcessName, Assembly.GetExecutingAssembly().GetName().Version.ToString());
    }
}
