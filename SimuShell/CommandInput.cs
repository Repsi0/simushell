using System;
namespace SimuShell
{
    public class CommandInput
    {
        public readonly string[] args;
        public ConsoleRecord cr;
        public CommandInput(string[] args_, ConsoleRecord cr_) {
            args = args_;
            cr = cr_;
        }
    }
}
