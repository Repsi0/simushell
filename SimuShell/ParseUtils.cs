using System;
using System.IO;
using System.Collections.Generic;

namespace SimuShell
{
    public static class ParseUtils
    {
        // Adds a / to the end of a directory if there isn't one
        public static string FixDirectory(string path) => (path.EndsWith('/')) ? path : path + '/';
        // Converts directories from /home/{user}/ to /~/
        public static string ConvDirectory(string path) => (!path.StartsWith("/home/" + System.Environment.UserName, StringComparison.InvariantCulture) ? path
                                                     : "~" + path.Remove(0, "/home/".Length + System.Environment.UserName.Length));
        // Adds currentdir to the beginning of directory if it doesn't start with a /
        public static string ConvToAbsoluteDirectory(string path) => path.Trim().StartsWith('/') ? path : ShellControl.currentdir + path.Trim();

        public static bool ParseGreaterThan(string cmd, ConsoleRecord cr)
        { // Parse > and >>, for file output.
            bool parsed = false; // Bool to keep track of if there were any > or >>; later sent to CommandExec() to determine whether or not to also send to standard output
            string[] appends = cmd.Split(">>"); // Split by append operator
            bool overwriteLast = appends[appends.Length - 1].Contains('>'); // If the last file is actually two, separated by >, we are going to overwrite the 2nd one.
            List<string> fin = new List<string>();
            string[] allwrites;
            foreach (string a in appends)
            { // Split further, by >
                foreach (string b in a.Split('>')) fin.Add(b.Trim());
            }
            allwrites = fin.ToArray(); // Convert back to array
            if (allwrites.Length > 2 && allwrites[1].Trim() != "")
            { // If we have more than 2 sides of the command (for example: echo blah >> test > test2), and the first argument isn't blank
                // i starts at 1; we don't need the command part
                for (int i = 1; i < allwrites.Length - 1; i++)
                { // Minus one because we are only creating nonexistent files right now
                    string path = ConvToAbsoluteDirectory(allwrites[i]); // Get path -- if starts with /, don't append the supplied path to the current dir
                    if (!File.Exists(path)) File.Create(path); // If the file doesn't exist, make it.
                }
                string path_complete = ConvToAbsoluteDirectory(allwrites[allwrites.Length - 1]);
                if (!overwriteLast)
                { // If we're appending..
                    StreamWriter sw = File.AppendText(path_complete); // Open a streamwriter in append mode
                    sw.WriteLine(cr.text); // Append a line of text
                    sw.Flush(); // Write / clear up memory
                    parsed = true;
                }
                else
                {
                    File.WriteAllText(path_complete, cr.text); // Overwrite completely with text
                    parsed = true;
                }
            }
            else if (allwrites.Length == 2 && allwrites[1].Trim() != "")
            { // If we have exactly two sides of the command (echo blah >> test)
                string path_complete = ConvToAbsoluteDirectory(allwrites[1]);
                bool fileIsNew = (!File.Exists(path_complete)) | overwriteLast; // Is the file new?
                if (!overwriteLast)
                { // If we aren't overwriting...
                    StreamWriter sw = File.AppendText(path_complete); // Open in append mode
                    if (fileIsNew) sw.Write(cr.text); // Write text if it's new, otherwise...
                    else sw.Write('\n' + cr.text); // New line, then write text.
                    sw.Flush(); // Write / clear up memory
                    parsed = true;
                }
                else
                {
                    File.WriteAllText(path_complete, cr.text); // Overwrite
                    parsed = true;
                }
            }
            return parsed;
        }
    }
}
