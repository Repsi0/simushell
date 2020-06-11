using System;
using System.Collections;
using System.IO;

namespace SimuShell
{
    public class AlphabeticalComparer : IComparer
    {
        public int Compare(object x, object y) {
            return new CaseInsensitiveComparer().Compare(x,y);
        }
    }
    public class ReverseAlphabeticalComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return new CaseInsensitiveComparer().Compare(y, x);
        }
    }
    public class FileSizeComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string pathA = (string)y;
            string pathB = (string)x;
            if (File.Exists(pathA) && File.Exists(pathB))
                return (int)(new FileInfo(pathA).Length / 1024 - new FileInfo(pathB).Length / 1024);
            else if (Directory.Exists(pathA) && Directory.Exists(pathB))
                return /* (int)(ParseUtils.GetDirectorySize(pathA) / 1024 - ParseUtils.GetDirectorySize(pathB) / 1024);*/ new CaseInsensitiveComparer().Compare(x, y);
            else throw new FileNotFoundException("Could not find file '" + pathA + "' or '" + pathB + "'!");
        }
    }
    public class ReverseFileSizeComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string pathA = (string)x;
            string pathB = (string)y;
            if (File.Exists(pathA) && File.Exists(pathB))
                return (int)(new FileInfo(pathA).Length / 1024 - new FileInfo(pathB).Length / 1024);
            else if (Directory.Exists(pathA) && Directory.Exists(pathB))
                return /*(int)(ParseUtils.GetDirectorySize(pathA) / 1024 - ParseUtils.GetDirectorySize(pathB) / 1024);*/ new CaseInsensitiveComparer().Compare(y, x);
            else throw new FileNotFoundException("Could not find file '" + pathA + "' or '" + pathB + "'!");
        }
    }
}
