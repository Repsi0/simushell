using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SUCC;

namespace SimuShell
{
    static class Program
    {   
        static readonly DataFile Config = new DataFile("Config"); //Initialize SUCC
        public static string logPath = "~/.tmp/simushell-cmd.log";

        static List<ShellCommand> commands = new List<ShellCommand>();
        public static string manPages = "\nSimuShell manual page:\n\n";
        public static string currentdir = "/";
        public static string visibledir = currentdir;

        static string[] ConvertToCommand(string command) {
            return command.Split(' ');
        }
        static string FixDirectory (string path) => (path.EndsWith('/')) ? path : path + '/';
        static string ConvDirectory(string path) => (!path.StartsWith("/home/"        + System.Environment.UserName.ToLowerInvariant(),StringComparison.InvariantCulture) ? path
                                                     : "~"+path.Remove(0, "/home/".Length + System.Environment.UserName.ToLowerInvariant().Length));
        public static void RegisterCommand(string cmdName, Predicate<string[]> cmd, string manEntry) {
            commands.Add(new ShellCommand(cmdName, cmd));
            manPages += cmdName + " - " + manEntry + "\n";
        }

        static void Interpret()
        {
            // Await next command
            visibledir = ConvDirectory(currentdir);
            Console.Write(visibledir);
            Console.Write(" - admin@SimuShell");
            Console.Write(">");
            string currentcommand = (Console.ReadLine());
            // Execute command
            CommandExec(ConvertToCommand(currentcommand));
        }
        public static void PrintValues(String[] myArr)
        {
            foreach (String i in myArr) {
                Console.WriteLine(i);
            }
        }
        public static void CommandExec(string[] command) {
            string cmd = command[0]; // Get main command from command/args array.
            // Find command with matching name
            // Boolean so we don't have two commands with matching names, if so, warn the user/dev.
            bool hasMatchedAlready = false;
            ShellCommand sc_to_run = null;
            foreach(ShellCommand sc in commands) {
                if (cmd == sc.cmdName) {
                    if (!hasMatchedAlready) {
                        hasMatchedAlready = true;
                        sc_to_run = sc;
                    } else Console.WriteLine(cmd + " was found to be a duplicate command name!");
                }
            }
            if(!hasMatchedAlready) Console.WriteLine(cmd + ": command not found");
            else sc_to_run.Execute(command.Skip(1).ToArray()); // Execute first command found, passing only the arguments (not command).
            Interpret();
        }
        static void Main() {
            // Grab some information from SUCC
            string pH = Config.Get("LOGGING", "on");
            pH = Config.Get("START-P", "on");
            pH = Config.Get("START-P", "on");
            string if_START = Config.Get<string>("START-P");
            if (if_START == "on") { Console.WriteLine("Welcome to SimuShell. Type 'man' for manual."); }
            // Register commands
            RegisterCommand("clear", (Predicate<string[]>)delegate (string[] args) {
                Console.Clear();
                return true;
            }, "clear the terminal screen");
            RegisterCommand("pwd", (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine(currentdir);
                return true;
            }, "print name of current/working directory");
            RegisterCommand("ls", (Predicate<string[]>)delegate (string[] args) {
                int amountoflisted = 0;
                // Get all files + folders in directory
                string[] filesindir = Directory.GetFiles(currentdir);
                string[] foldsindir = Directory.GetDirectories(currentdir);
                // Compile both into a single array
                string[] complete = new string[filesindir.Length + foldsindir.Length];
                foldsindir.CopyTo(complete, 0); filesindir.CopyTo(complete, foldsindir.Length);
                bool isFile = false;
                int numperline = 5;
                int[] maxLength = new int[numperline];
                for(int i = 0; i < complete.Length; i++) {
                    int col = i%numperline;
                    if (complete[i].Length > maxLength[col]) maxLength[col] = complete[i].Length;
                }
                for (int i = 0; i < complete.Length; i++)
                {
                    // Appends '/', but only if it's a folder, and not a directory.
                    if (i >= foldsindir.Length && !isFile) isFile = true;
                    string newname = complete[i] + (isFile ? "" : "/");
                    // Add 3 spaces at the end of each name to make it look nice. (May change in future?)
                    Console.Write(newname.PadLeft(i%numperline==0?0:3).PadRight(3 + maxLength[i%numperline]));
                    // Insert a new line every 5 listed.
                    amountoflisted = amountoflisted + 1;
                    if (amountoflisted == numperline)
                    {
                        // Only write another line if we're not on the last one (prevents from weird looking spacing at the end)
                        if (i != complete.Length - 1) Console.WriteLine("");
                        amountoflisted = 0;
                    }
                }
                // Write an additional line to return to normal shell appearance.
                Console.WriteLine("");
                return true;
            }, "list directory contents");
            RegisterCommand("cd", (Predicate<string[]>)delegate (string[] args) {
                string[] path = args[0].Split('/'); // Should be first argument; split into each directory
                // If path starts with /, start searching from /, if ~, start searching from user dir, else, just use current directory.
                string newPath = args[0].StartsWith('/') ? "/" : 
                                 (args[0].StartsWith('~') ? "/home/" + System.Environment.UserName : currentdir);
                if (args[0].StartsWith('~')) path = args[0].Remove(0, 1).Split('/'); // Remove ~ if in user's directory.
                foreach (string dir in path) {
                    if (dir == "..") {
                        //Go back one -- double remove since there is an extra / at the end, but not if we're already at /
                        if (newPath != "/") {
                            newPath = newPath.Remove(newPath.LastIndexOf('/'));
                            newPath = newPath.Remove(newPath.LastIndexOf('/'));
                        }
                    }
                    else if (dir == ".") { } // Same directory; no change
                    else {
                        string[] dirs = Directory.GetDirectories(newPath);
                        string pathPlusDir = newPath + dir;
                        // Check that directory exists
                        if (Array.Exists(dirs, directory => directory.Contains(pathPlusDir))) {
                            newPath = pathPlusDir;
                        } else {
                            Console.WriteLine("Not a valid directory: " + pathPlusDir);
                            return false;
                        }
                    }
                    newPath = FixDirectory(newPath);
                }
                currentdir = newPath;
                return true;
            }, "change current/working directory");
            RegisterCommand("man", (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine(manPages);
                return true;
            }, "show the manuals (this text)");
            RegisterCommand("exit", (Predicate<string[]>)delegate (string[] args) {
                System.Environment.Exit(0);
                return true; // Never run; but needed
            }, "exit SimuShell");
            RegisterCommand("echo", (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine(string.Join("", args));
                return true;
            }, "display a line of text");
            RegisterCommand("settings", (Predicate<string[]>)delegate (string[] args) {
                SUCC_SET();
                return true;
            }, "open SUCC config settings");
            RegisterCommand("help", (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine("Welcome to the SimuShell help prompt! Go away, this is under construction. GET OUT!");
                return true;
            }, "show help text");
            // Enter the interpret loop
            Interpret();
        }

        /*public static void TestExternProgram(){
            Console.WriteLine("Test1 Works!");
        }*/

        public static void SUCC_SET() { // Change SUCC settings
            string[] options = new string[2]; // Size must be the number of settings in array
            options[0] = "LOGGING - LOG ALL COMMANDS WRITTEN";
            options[1] = "START-P - SHOW PROMPT AT START OF APP";
            bool exit=false;
            while(!exit) { // Loop until user exits
                Console.Clear();
                PrintValues(options);
                Console.WriteLine(""); // New line between selections and input
                Console.Write("Select the option you would like to change (case sensitive), or type exit to exit: ");
                string input = Console.ReadLine();
                if (input != "exit") {
                    if(Config.KeyExists(input)) { // Must be updated if you wish to support more types than just booleans
                        Config.Set(input, Config.Get(input,"on")=="on"?"off":"on"); // Toggle config entry
                    } else Console.WriteLine("Config entry '" + input + "' does not exist.");
                } else exit=true;
            }
            Console.Clear();
            Interpret();
        }
    }
}