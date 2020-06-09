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
        public static string currentdir = "/";

        static string[] ConvertToCommand(string command) {
            return command.Split(' ');
        }
        static void Interpret()
        {
            // Await next command
            Console.Write(currentdir);
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
            foreach(ShellCommand sc in commands) {
                if (cmd == sc.cmdName) {
                    sc.Execute(command.Skip(1).ToArray()); // Execute, passing only the arguments (not command).
                    return;
                }
            }
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
            commands.Add(new ShellCommand("clear",    (Predicate<string[]>)delegate (string[] args) {
                Console.Clear();
                return true;
            }));
            commands.Add(new ShellCommand("pwd",      (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine(currentdir);
                return true;
            }));
            commands.Add(new ShellCommand("ls",       (Predicate<string[]>)delegate (string[] args) {
                int amountoflisted = 0;
                // Get all files + folders in directory
                string[] filesindir = Directory.GetFiles(currentdir);
                string[] foldsindir = Directory.GetDirectories(currentdir);
                // Compile both into a single array
                string[] complete = new string[filesindir.Length + foldsindir.Length];
                complete.CopyTo(foldsindir, 0); complete.CopyTo(filesindir, foldsindir.Length);
                bool isFile = false;
                for (int i = 0; i < complete.Length; i++)
                {
                    // Appends '/', but only if it's a folder, and not a directory.
                    if (i >= foldsindir.Length && !isFile) isFile = true;
                    string newname = complete[i] + (isFile ? "" : "/");
                    // Add 3 spaces at the end of each name to make it look nice. (May change in future?)
                    Console.Write(newname.PadRight(3 + newname.Length));
                    // Insert a new line every 5 listed.
                    amountoflisted = amountoflisted + 1;
                    if (amountoflisted == 5)
                    {
                        // Only write another line if we're not on the last one (prevents from weird looking spacing at the end)
                        if (i != complete.Length - 1) Console.WriteLine("");
                        amountoflisted = 0;
                    }
                }
                // Write an additional line to return to normal shell appearance.
                Console.WriteLine("");
                return true;
            }));
            commands.Add(new ShellCommand("cd",       (Predicate<string[]>)delegate (string[] args) {
                string[] path = args[0].Split('/'); // Should be first argument; split into each directory
                string newPath = currentdir;
                foreach (string dir in path) {
                    if (dir == "..") {
                        //Go back one -- if we're not at the beginning already
                        newPath = newPath.LastIndexOf("/") == 0 ? newPath : newPath.Remove(newPath.LastIndexOf('/'));
                    }
                    else if (dir == ".") { } // Same directory; no change
                    else {
                        string[] dirs = Directory.GetDirectories(newPath);
                        string pathPlusDir = newPath == "/" ? newPath + dir : newPath + "/" + dir; // Appends dir to path if path is /, otherwise adds another / before adding dir
                        // Check that directory exists
                        if (Array.Exists(dirs, directory => directory.Contains(pathPlusDir))) {
                            newPath = pathPlusDir;
                        } else {
                            Console.WriteLine("Not a valid directory: " + pathPlusDir);
                            return false;
                        }
                    }
                }
                currentdir = newPath;
                return true;
            }));
            commands.Add(new ShellCommand("man",      (Predicate<string[]>)delegate (string[] args) {
                string[] man = new string[6]; // Size must be the amount of commands in the array/man page
                man[0] = "cd; change directory ('..' to go back a directory)";
                man[1] = "list (ls); list all files and folders in directory";
                man[2] = "dir; list current directory";
                man[3] = "man; lists available commands";
                man[4] = "exit (quit); exits SimuShell";
                man[5] = "echo; prints text after it";
                PrintValues(man);
                return true;
            }));
            commands.Add(new ShellCommand("exit",     (Predicate<string[]>)delegate (string[] args) {
                System.Environment.Exit(0);
                return true; // Never run; but needed
            }));
            commands.Add(new ShellCommand("echo",     (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine(string.Join("", args));
                return true;
            }));
            commands.Add(new ShellCommand("settings", (Predicate<string[]>)delegate (string[] args) {
                SUCC_SET();
                return true;
            }));
            commands.Add(new ShellCommand("help",     (Predicate<string[]>)delegate (string[] args) {
                Console.WriteLine("Welcome to the SimuShell help prompt! Go away, this is under construction. GET OUT!");
                return true;
            }));
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