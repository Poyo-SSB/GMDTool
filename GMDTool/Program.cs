using CommandLine;
using GMDTool.Convert;
using System;
using System.IO;

namespace GMDTool
{
    public class Program
    {
        private const string version = "v1.0.0";

        private const string issues_link = "https://github.com/Poyo-SSB/GMDTool/issues";

        private static void Main(string[] args)
        {
            new Parser(config => config.HelpWriter = null).ParseArguments<GMDConverterOptions>(args)
                .WithParsed(o =>
                {
                    if (o.Output == null)
                    {
                        o.Output = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(o.Input) + ".dae");
                    }

                    try
                    {
                        var converter = new GMDConverter(o);
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
                        throw;
                    }
                    catch (ApplicationException)
                    {
                        PrintError($"Something has gone terribly, terribly wrong. Please file an issue at {issues_link}.");
                        throw;
                    }
                })
                .WithNotParsed(e =>
                {
                    if (e.IsHelp())
                    {
                        PrintUsage();
                    }
                    else if (e.IsVersion())
                    {
                        Console.WriteLine(version);
                    }
                    else
                    {
                        foreach (var error in e)
                        {
                            switch (error.Tag)
                            {
                                case ErrorType.MissingRequiredOptionError:
                                    PrintError("Input file not provided.");
                                    break;
                            }
                            PrintUsage();
                        }
                    }
                });
        }
        
        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {error}");
            Console.ResetColor();
        }

        private static void PrintUsage()
        {
            Console.WriteLine($"Usage: GMDTool [flags] <input file> [output file]");
            Console.WriteLine($"Flags:");
            Console.WriteLine($"    --help                  Displays this usage screen.");
            Console.WriteLine($"    --version               Prints the version of this tool.");
            Console.WriteLine($"    -b, --blender-output    Enables Blender compatibility output. See GitHub readme for details.");
            Console.WriteLine($"    -i, --ignore-empty      Ignores empty nodes.");
        }
    }
}
