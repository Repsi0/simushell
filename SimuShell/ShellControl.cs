using System;
using System.Linq;

namespace SimuShell
{
    public static class ShellControl
    {
        public static string currentdir = "/";
        public static string visibledir = currentdir;
        static void ExecuteCommand_Major(string command)
        {
            CommandExec(command);
            Interpret();
        }
        // Interpret command
        public static void Interpret()
        {
            // Await next command
            visibledir = ParseUtils.ConvDirectory(currentdir);
            Console.Write(visibledir);
            Console.Write(" - admin@SimuShell");
            Console.Write(">");
            string currentcommand = (Console.ReadLine());
            // Execute command
            ExecuteCommand_Major(currentcommand);
        }
        // Execute command
        public static ConsoleRecord CommandExec(string cmdStr)
        {
            ConsoleRecord record = new ConsoleRecord();
            string[] command = cmdStr.Split(' ');

            string cmd = command[0]; // Get main command from command/args array.
                                     // Find command with matching name
                                     // Boolean so we don't have two commands with matching names, if so, warn the user/dev.
            bool hasMatchedAlready = false;
            ShellCommand sc_to_run = null;
            foreach (ShellCommand sc in CommandRegistry.commands)
            {
                if (cmd == sc.cmdName)
                {
                    if (!hasMatchedAlready)
                    {
                        hasMatchedAlready = true;
                        sc_to_run = sc;
                    }
                    else record.Write(cmd + " was found to be a duplicate command name!");
                }
            }
            if (!hasMatchedAlready) record.Write(cmd + ": command not found");
            else sc_to_run.Execute(new CommandInput(command.Skip(1).ToArray(), record)); // Execute first command found, passing only the arguments (not command).
            if (!ParseUtils.ParseGreaterThan(cmdStr, record) && record.ContainsText()) record.Post();
            return record;
        }
    }
}
