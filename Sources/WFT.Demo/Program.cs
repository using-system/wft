using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFT.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                string category = ReadCategory();
                string activity = ReadActivity(category);

                WorkflowInvoker invoker = new WorkflowInvoker(
                    (Activity)Activator.CreateInstance(Type.GetType(activity)));

                Console.WriteLine("Launch the activity {0}...", activity);
                invoker.Invoke();
                Console.WriteLine("End of the activity {0}.", activity);
            }

        }

        private static string ReadCategory()
        {
            Dictionary<int, string> choices = new Dictionary<int, string>();

            Console.WriteLine("Enter the category number you want to display and press [Enter]");
            int choiceIndex = 1;
            foreach(var demo in DemoRepository.Current.Demos)
            {
                Console.WriteLine("{0} : {1}", choiceIndex, demo.Key);
                choices.Add(choiceIndex, demo.Key);
                choiceIndex++;
            }


            while(true)
            {
                if (int.TryParse(Console.ReadLine(), out choiceIndex))
                    return choices[choiceIndex];

                Console.WriteLine("Please enter a number");

            }
        }

        private static string ReadActivity(string category)
        {
            Dictionary<int, string> choices = new Dictionary<int, string>();

            Console.WriteLine("Enter the demo number you want to execute and press [Enter]");
            int choiceIndex = 1;

            foreach (var demo in DemoRepository.Current.Demos[category])
            {
                Console.WriteLine("{0} : {1}", choiceIndex, demo.Item1);
                choices.Add(choiceIndex, demo.Item1);
                choiceIndex++;
            }

            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out choiceIndex))
                    return DemoRepository.Current.Demos[category][choiceIndex-1].Item2;

                Console.WriteLine("Please enter a number");

            }
        }

    }
}
