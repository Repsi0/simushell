using System;
using System.Collections.Generic;
using System.IO;

namespace SimuShell
{
    public static class CommandRegistry
    {
        public static List<ShellCommand> commands = new List<ShellCommand>(); // List of commands
        public static string manPages = "\nSimuShell manual page:\n\n"; // Man pages string

        // Registers a command, requiring the command's name, the function, and the manual entry.
        public static void RegisterCommand(string cmdName, Predicate<CommandInput> cmd, string manEntry)
        {
            commands.Add(new ShellCommand(cmdName, cmd));
            manPages += cmdName + " - " + manEntry + "\n";
        }

        public static void RegisterCommands()
        {
            RegisterCommand("clear", (Predicate<CommandInput>)delegate (CommandInput input) {
                input.cr.Clear();
                return true;
            }, "clear the terminal screen");
            RegisterCommand("pwd", (Predicate<CommandInput>)delegate (CommandInput input) {
                input.cr.Write(ShellControl.currentdir);
                return true;
            }, "print name of current/working directory");
            RegisterCommand("ls", (Predicate<CommandInput>)delegate (CommandInput input) {
                int amountoflisted = 0;
                // Get all files + folders in directory
                string[] filesindir = Directory.GetFiles(ShellControl.currentdir);
                string[] foldsindir = Directory.GetDirectories(ShellControl.currentdir);
                // Compile both into a single array
                string[] complete = new string[filesindir.Length + foldsindir.Length];
                foldsindir.CopyTo(complete, 0); filesindir.CopyTo(complete, foldsindir.Length);
                bool isFile = false;
                int numperline = 5;
                int[] maxLength = new int[numperline];
                for (int i = 0; i < complete.Length; i++)
                {
                    int col = i % numperline;
                    // Make it look nicer by converting /home/{USER}/ to ~/
                    complete[i] = ParseUtils.ConvDirectory(complete[i]);
                    if (complete[i].Length > maxLength[col]) maxLength[col] = complete[i].Length;
                }
                for (int i = 0; i < complete.Length; i++)
                {
                    // Appends '/', but only if it's a folder, and not a directory.
                    if (i >= foldsindir.Length && !isFile) isFile = true;
                    string newname = complete[i] + (isFile ? "" : "/");
                    // Add 3 spaces at the end of each name to make it look nice. (May change in future?)
                    input.cr.Write(newname.PadLeft(i % numperline == 0 ? 0 : 3).PadRight(3 + maxLength[i % numperline]));
                    // Insert a new line every 5 listed.
                    amountoflisted = amountoflisted + 1;
                    if (amountoflisted == numperline)
                    {
                        // Only write another line if we're not on the last one (prevents from weird looking spacing at the end)
                        if (i != complete.Length - 1) input.cr.WriteLine("");
                        amountoflisted = 0;
                    }
                }
                // Write an additional line to return to normal shell appearance.
                input.cr.Write("");
                return true;
            }, "list directory contents");
            RegisterCommand("cd", (Predicate<CommandInput>)delegate (CommandInput input) {
                string[] path = input.args[0].Split('/'); // Should be first argument; split into each directory
                // If path starts with /, start searching from /, if ~, start searching from user dir, else, just use current directory.
                string newPath = input.args[0].StartsWith('/') ? "/" :
                                 (input.args[0].StartsWith('~') ? "/home/" + System.Environment.UserName : ShellControl.currentdir);
                if (input.args[0].StartsWith('~')) path = input.args[0].Remove(0, 1).Split('/'); // Remove ~ if in user's directory.
                foreach (string dir in path)
                {
                    if (dir == "..")
                    {
                        //Go back one -- double remove since there is an extra / at the end, but not if we're already at /
                        if (newPath != "/")
                        {
                            newPath = newPath.Remove(newPath.LastIndexOf('/')).Remove(newPath.Remove(newPath.LastIndexOf('/')).LastIndexOf('/'));
                        }
                    }
                    else if (dir == ".") { } // Same directory; no change
                    else
                    {
                        string[] dirs = Directory.GetDirectories(newPath);
                        string pathPlusDir = newPath + dir;
                        // Check that directory exists
                        if (Array.Exists(dirs, directory => directory.Contains(pathPlusDir)))
                        {
                            newPath = pathPlusDir;
                        }
                        else
                        {
                            input.cr.Write("Not a valid directory: " + pathPlusDir);
                            return false;
                        }
                    }
                    newPath = ParseUtils.FixDirectory(newPath);
                }
                ShellControl.currentdir = newPath;
                return true;
            }, "change current/working directory");
            RegisterCommand("man", (Predicate<CommandInput>)delegate (CommandInput input) {
                input.cr.Write(manPages);
                return true;
            }, "show the manuals (this text)");
            RegisterCommand("exit", (Predicate<CommandInput>)delegate (CommandInput input) {
                System.Environment.Exit(0);
                return true; // Never run; but needed
            }, "exit SimuShell");
            RegisterCommand("echo", (Predicate<CommandInput>)delegate (CommandInput input) {
                string tex1 = string.Join(" ", input.args);

                input.cr.Write(tex1.Remove(tex1.IndexOf('>')));
                return true;
            }, "display a line of text");
            RegisterCommand("settings", (Predicate<CommandInput>)delegate (CommandInput input) {
                ConfigManager.SUCC_SET();
                return true;
            }, "open SUCC config settings");
            RegisterCommand("help", (Predicate<CommandInput>)delegate (CommandInput input) {
                input.cr.Write("Welcome to the SimuShell help prompt! Go away, this is under construction. GET OUT!");
                return true;
            }, "show help text");
            RegisterCommand("cat", (Predicate<CommandInput>)delegate (CommandInput input) {
                string path = ParseUtils.ConvToAbsoluteDirectory(input.args[0]);
                if (Directory.Exists(path))
                {
                    input.cr.Write("cat: " + input.args[0] + ": Is a directory");
                    return false;
                }
                else if (File.Exists(path))
                {
                    input.cr.Write(File.ReadAllText(path));
                    return true;
                }
                else
                {
                    input.cr.Write("cat: " + input.args[0] + ": No such file or directory");
                    return false;
                }
            }, "concatenate files and print");
        }
    }
}
