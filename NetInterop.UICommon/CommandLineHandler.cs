using System;
using System.Collections.Generic;
using System.Linq;
using Nalarium;

namespace NetInterop.UICommon
{
    internal static class CommandLineHandler
    {
        internal static Map Parse(String[] args, String[] allowedArguments)
        {
            var dictionary = new Map();
            var arguments = new List<String>(args);
            foreach (string arg in arguments)
            {
                string[] parts = arg.Split(':');
                if (parts.Length != 2)
                {
                    continue;
                }
                string parameter = parts[0].Replace("-", "").Trim();
                string value = parts[1].Replace("\"", "").Trim();
                if (allowedArguments.Count(p => p == parameter) == 0)
                {
                    continue;
                }
                dictionary.Add(parameter, value);
            }
            return dictionary;
        }
    }
}