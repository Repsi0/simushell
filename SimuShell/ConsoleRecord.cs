using System;
using System.IO;
using System.Text;

namespace SimuShell
{
    public class ConsoleRecord {
        public string text = "";
        public bool willClear = false;

        public ConsoleRecord() {
            text = "";
        }
        public void Write(string value) {
            // Record output
            text += value;
        }
        public void WriteLine(string value) {
            // Record output
            Write(value + '\n');
        }
        public void Clear() {
            text = "";
            willClear = true;
        }
        public void Post() {
            Console.WriteLine(text);
            if(willClear) Console.Clear();
        }
        public bool ContainsText() => text != "";
        public bool ContainsTextTrimWhiteSpace() => text.Trim() != "";
    }
}
