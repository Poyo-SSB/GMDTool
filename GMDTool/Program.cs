using GMDTool.Convert;
using System;
using System.IO;

namespace GMDTool
{
    public class Program
    {
        private const string issues_link = "https://github.com/Poyo-SSB/GMDTool/issues";

        private static void Main(string[] args)
        {
            // TODO: options to disable lights, disable cameras, disable empty nodes

            var options = new GMDConverterOptions();

            string input;
            string output;

            if (args.Length == 0)
            {
                PrintError("No input file provided.");
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
                PrintError("Too many arguments.");
                PrintUsage();
                return;
            }

            try
            {
                Console.WriteLine($"input: {input}");
                Console.WriteLine($"output: {output}");
                var converter = new GMDConverter(input, output, options);
                converter.Export();
            }
            catch (FileNotFoundException)
            {
                PrintError("File not found.");
                PrintUsage();
            }
            catch (DirectoryNotFoundException)
            {
                PrintError("Directory not found.");
                PrintUsage();
            }
            catch (ArgumentException)
            {
                PrintError("Invalid output path.");
                PrintUsage();
            }
            catch (ApplicationException)
            {
                PrintError($"Something has gone terribly, terribly wrong. Please file an issue at {issues_link}.");
                throw;
            }
        }
        
        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {error}");
            Console.ResetColor();
        }

        private static void PrintUsage() => Console.WriteLine($"Usage: GMDTool <input file> <output file>");
    }
}
