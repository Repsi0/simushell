using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimuShell
{
    static class Program
    {
        // Main function
        static void Main() {
            // Initiate ConfigManager
            ConfigManager.Load();
            // Register commands
            CommandRegistry.RegisterCommands();
            // Enter the interpret loop
            ShellControl.Interpret();
        }
    }
}