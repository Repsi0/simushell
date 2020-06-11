using System;

namespace SimuShell
{
    public class ShellCommand {
        public Predicate<CommandInput> execPredicate;
        public string cmdName;
        public ShellCommand(string cmd, Predicate<CommandInput> execPredicate_) {
            cmdName = cmd;
            execPredicate = execPredicate_;
        }
        public bool Execute(CommandInput args) {return execPredicate(args);}
    }
}