using System;
using McMaster.Extensions.CommandLineUtils;

namespace Founders
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var app = new CommandLineApplication();

            app.Description =
                "Creates files with the names of given strings and containing the coresponding hashcode.";
            app.HelpOption("-?|-h|--help");
            app.ExtendedHelpText =
                "Following argument is required -strings and options are case sensitive.";
            var stringsOpt = app.Option(
    "-add|--add <string>",
    "Defines one of the strings you wish to create a file for. Ex \" -add hello \"",
    CommandOptionType.MultipleValue);

            var pathOpt = app.Option(
    "-path|--path <path>",
    "Defines the path of the files to write to. Ex \" -path C:\\myDirectory\\\". Defaults to the relative directory",
    CommandOptionType.SingleValue);

            var enableTimestampOpt = app.Option(
    "-timestamp|--timestamp",
    "Option for the filenames to be extended by the current timestamp",
    CommandOptionType.NoValue);


            app.Command("hide", (command) =>
            {
                command.Description = "Instruct the ninja to hide in a specific location.";
                command.HelpOption("-?|-h|--help");

                var locationArgument = command.Argument("[location]",
                                           "Where the ninja should hide.");

                command.OnExecute(() =>
                {
                    var location = locationArgument.h
                  ? locationArgument.Value()
                  : "under a turtle";
                    Console.WriteLine("Ninja is hidden " + location);
                });
            });

        }
    }
}
