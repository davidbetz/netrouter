using System;

namespace NetInterop.Routing
{
    public static class Log
    {
        public static void Write(string category, string name)
        {
            Write(category, name, string.Empty);
        }

        public static void Write(string category, string name, string data)
        {
            if (String.IsNullOrEmpty(category))
            {
                throw new ArgumentNullException("category");
            }
            if (String.IsNullOrEmpty(category))
            {
                throw new ArgumentNullException("name");
            }
            category = category.ToUpper();
            if (!RoutingController.DebugCategoryList.Contains(category))
            {
                return;
            }
            name = name.ToUpper();
            RoutingController.VerboseOutput(String.Format("{0}:{1}{2}" + Environment.NewLine, category, name, ":" + data));
        }
    }
}