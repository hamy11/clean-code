using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                {"__", "strong"},
                {"~~", "strike"}
            };

            tagSearchTree = BuildTagSearchTree(tagDictionary);
        }

        private readonly Stack<TagMatch> noPairTagStack = new Stack<TagMatch>();
        
        public string RenderToHtml(string markdown)
        {
            using (var entityIterator = tagSearchTree.Find(markdown).GetEnumerator())
            {
                return RecoursionTagRender(entityIterator, markdown);
            }
        }

        public string RecoursionTagRender(IEnumerator<IMatchType> entityIterator, string markdown)
        {
            var renderBuilder = new StringBuilder();
            while (entityIterator.MoveNext())
            {
                if (entityIterator.Current == null) break;
                if (entityIterator.Current.GetType() == typeof(SymbolMatch))
                {
                    var symbolMatch = (SymbolMatch) entityIterator.Current;
                    renderBuilder.Append(symbolMatch.Symbol);
                    continue;
                }

                var currentTag = (TagMatch) entityIterator.Current;
                if (noPairTagStack.Any() && currentTag.TagDefinition == noPairTagStack.Peek().TagDefinition)
                {
                    noPairTagStack.Pop();
                    return renderBuilder.ToString();
                }
                
                noPairTagStack.Push(currentTag);
                var currentTagContent = RecoursionTagRender(entityIterator, markdown);
                renderBuilder.Append(HtmlWriter.TagLine(currentTag.TagName, currentTagContent));
            }
            return renderBuilder.ToString();
        }

        public Tree BuildTagSearchTree(Dictionary<string, string> tagDictionary)
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