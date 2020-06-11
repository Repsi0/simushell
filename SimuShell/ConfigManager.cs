using System;
using SUCC;

namespace SimuShell
{
    public static class ConfigManager {
        static readonly DataFile Config = new DataFile("Config"); //Initialize SUCC
        // Change SUCC settings
        public static void SUCC_SET()
        {
            string[] options = new string[2]; // Size must be the number of settings in array
            options[0] = "LOGGING - LOG ALL COMMANDS WRITTEN";
            options[1] = "START-P - SHOW PROMPT AT START OF APP";
            bool exit = false;
            while (!exit)
            { // Loop until user exits
                Console.Clear();
                foreach (string opt in options) Console.WriteLine(opt);
                Console.WriteLine(""); // New line between selections and input
                Console.Write("Select the option you would like to change (case sensitive), or type exit to exit: ");
                string input = Console.ReadLine();
                if (input != "exit")
                {
                    if (Config.KeyExists(input))
                    { // Must be updated if you wish to support more types than just booleans
                        Config.Set(input, Config.Get(input, "on") == "on" ? "off" : "on"); // Toggle config entry
                    }
                    else Console.WriteLine("Config entry '" + input + "' does not exist.");
                }
                else exit = true;
            }
            Console.Clear();
            ShellControl.Interpret();
        }
        public static void Load()
        {
            // Grab some information from SUCC
            string pH = Config.Get("LOGGING", "on");
            string if_START = Config.Get<string>("START-P");
            if (if_START == "on") { Console.WriteLine("Welcome to SimuShell. Type 'man' for manual."); }
        }
    }
}
