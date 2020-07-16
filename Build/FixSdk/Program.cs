using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace FixSdk
{
    class Program
    {
        static void Main(string[] args)
        {
            int filesProcessed = 0;
            if (args.Length != 1) throw new ArgumentException("ONE PARAMETER CONTAINS ROOT OF SDKS");
            string root = args[0];
            string[] filesList =
            {
                Path.Combine(root, @"GameLift_12_14_2018\GameLift-SDK-Release-3.3.0\GameLift-CSharp-ServerSDK-3.3.0\Net45\packages.config"),
                Path.Combine(root, @"GameLift_09_03_2019\GameLift-SDK-Release-3.4.0\GameLift-CSharp-ServerSDK-3.4.0\Net45\packages.config"),
            };
            foreach (string fileName in filesList)
            {
                if (File.Exists(fileName))
                {
                    List<string> file = File.ReadAllLines(fileName).ToList();
                    if (!file[8].Contains("1.4.0"))
                    file.Insert(8, @"  <package id=""System.Collections.Immutable"" version=""1.4.0"" targetFramework=""net45"" />");
                    File.WriteAllLines(fileName, file.ToArray());
                    filesProcessed++;
                }
                else
                {
                    Console.WriteLine($"{fileName} not found");
                }
            }
            Console.WriteLine($"FixSdk processed {filesProcessed} files");
            if (filesProcessed == 0) Console.WriteLine($"Incorrect root? {args[0]}");
        }
    }
}
