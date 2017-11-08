using System;
using System.IO;
using System.Text;

namespace Markdown
{
    public class HtmlWriter 
    {
        public static string TagLine(string tagName, string line)
        {
            return $"<{tagName}>{line}</{tagName}>";
        }
    }
}