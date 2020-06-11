using System;
using System.IO;
using System.Text;

namespace SimuShell
{
    public class ConsoleRecord {
        public string text = "";

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
        }
        public void Post() {
            Console.WriteLine(text);
        }
        public bool ContainsText() => text != "";
        public bool ContainsTextTrimWhiteSpace() => text.Trim() != "";
    }
}
