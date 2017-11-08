using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Markdown
{
	public class Md
	{
	    private readonly Tree tagSearchTree;

	    public Md()
	    {
            var tagDictionary = new Dictionary<string, string>
            {
                {"_", "em"},
                {"__", "strong"}
            };

            tagSearchTree = BuildTagSearchTree(tagDictionary);
        }


		public string RenderToHtml(string markdown)
		{
            var tagQueue = GetCorrectTagQueue(markdown);

		    if (tagQueue.Count == 0)
		        return markdown;

		    using (var htmlWriter = new HtmlWriter(new MemoryStream()))
		    {
		        var prevTagEndPosition = 0;
		        while (tagQueue.Any())
		        {
		            var firstTag = tagQueue.Dequeue();
                    htmlWriter.Write(markdown.Substring(prevTagEndPosition, firstTag.Position - prevTagEndPosition));

                    var secondTag = tagQueue.Dequeue();
                    if(secondTag == null || secondTag.PatternName!=firstTag.PatternName)
                        continue;

                    using (htmlWriter.Tag(firstTag.PatternName))
		            {
                        var cutStartIndex = firstTag.Position + firstTag.PatternValue.Length;
                        var cutLength = secondTag.Position - cutStartIndex;
                        htmlWriter.Write(markdown.Substring(cutStartIndex, cutLength));
                    }
		            prevTagEndPosition = secondTag.Position + secondTag.PatternValue.Length;
		        }
                htmlWriter.Write(markdown.Substring(prevTagEndPosition));

                markdown = htmlWriter.Read();
		    }

            return markdown; 
		}

	    private Queue<MatchInfo> GetCorrectTagQueue(string markdown)
	    {
	        var tagQueue = new Queue<MatchInfo>();
	        foreach (var tagMatch in tagSearchTree.Find(markdown))
	        {
	            if (IsCorrectTag(markdown, tagMatch))
	                tagQueue.Enqueue(tagMatch);
	        }
	        return tagQueue;
	    }

	    public bool IsCorrectTag(string markdown, MatchInfo tag)
	    {
	        if (tag.Position > 0)
	        {
	            return markdown[tag.Position - 1] != '\\';
	        }

	        return true;
	    }

	    public Tree BuildTagSearchTree(Dictionary<string,string> tagDictionary)
	    {
            var tree = new Tree();
	        foreach (var pair in tagDictionary)
	        {
	            tree.Add(pair);
	        }
            tree.Build();
	        return tree;
	    }
	}
}