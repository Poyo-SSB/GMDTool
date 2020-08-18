using GMDTool.Convert;
using System;
using System.IO;

namespace GMDTool
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string input = String.Empty;
            string output = String.Empty;

            if (args.Length == 0)
            {
                Console.WriteLine("Error: No input file provided.");
                PrintUsage();
                return;
            }
            else if (args.Length == 1)
            {
                input = args[0];
                output = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(args[0]) + ".dae");
            }
            else if (args.Length == 2)
            {
                input = args[0];
                output = args[1];
            }
            else
            {
                Console.WriteLine("Error: Too many arguments.");
                PrintUsage();
                return;
            }
            
            try
            {
                GMDConverter.Export(input, output);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: File not found.");
                PrintUsage();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Error: Directory not found.");
                PrintUsage();
            }
        }

        private static void PrintUsage() => Console.WriteLine($"Usage: GMDTool <input file> <output file>");
    }
}
