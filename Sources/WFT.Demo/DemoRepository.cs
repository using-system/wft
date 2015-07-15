using System;
using System.Activities;
using System.Collections.Generic;
using System.Reflection;

namespace WFT.Demo
{
    public class DemoRepository
    {
        public static readonly DemoRepository Current = new DemoRepository();

        private DemoRepository()
        {
            Demos = new Dictionary<string,List<Tuple<string,string>>>();

            foreach(Type type in Assembly.GetExecutingAssembly().GetTypes())
                if(typeof(Activity).IsAssignableFrom(type))
                {
                    string[] namespaceSegment = type.Namespace.Split('.');
                    string category = namespaceSegment[namespaceSegment.Length - 1];

                    if (!Demos.ContainsKey(category))
                        Demos.Add(category, new List<Tuple<string, string>>());

                    Demos[category].Add(new Tuple<string, string>(type.Name, type.ToString()));
                }
        }

        public Dictionary<string, List<Tuple<string, string>>> Demos { get; private set; }
    }
}
