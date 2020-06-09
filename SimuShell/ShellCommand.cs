using System;

namespace SimuShell
{
    public class ShellCommand {
        public Predicate<string[]> execPredicate;
        public string cmdName;
        public ShellCommand(string cmd, Predicate<string[]> execPredicate_) {
            cmdName = cmd;
            execPredicate = execPredicate_;
        }
        public bool Execute(string[] args) {return execPredicate(args);}
    }
}