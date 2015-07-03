using System.Collections.Generic;

public static class ExtensionMethods
{
    public static List<string> Parse(this string[] args)
    {
        List<string> argList = new List<string>();

        foreach (string arg in args)
        {
            argList.Add(arg.ToUpper());
        }

        return argList;
    }
}