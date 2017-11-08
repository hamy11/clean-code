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

        private readonly Stack<MatchInfo> openedTagStack = new Stack<MatchInfo>();
        private int prevTagEndPos;

        public string RecoursionTagRender(IEnumerator<MatchInfo> tagIterator, string markdown, bool nested)
        {
            var renderBuilder = new StringBuilder();
            while (tagIterator.MoveNext())
            {
                var closingTag = tagIterator.Current;
                if (closingTag == null) break;

                if (!openedTagStack.Any())
                {
                    renderBuilder.Append(markdown.Substring(prevTagEndPos, closingTag.Position - prevTagEndPos));
                    openedTagStack.Push(closingTag);
                    continue;
                }

                var openingTag = openedTagStack.Pop();
                if (closingTag.PatternName == openingTag.PatternName)
                {
                    var tagLine = HtmlWriter.TagLine(openingTag.PatternName,
                        CutBetweenTags(markdown, openingTag, closingTag));
                    renderBuilder.Append(tagLine);
                    prevTagEndPos = closingTag.Position + closingTag.PatternValue.Length;

                    if (nested) return renderBuilder.ToString();

                    continue;
                }

                var nextTag = closingTag;
                var tagContentBuilder = new StringBuilder();
                var prevTag = openingTag;

                while (nextTag != null && openingTag.PatternName != nextTag.PatternName)
                {
                    openedTagStack.Push(nextTag);

                    tagContentBuilder.Append(CutBetweenTags(markdown, prevTag, nextTag));
                    tagContentBuilder.Append(RecoursionTagRender(tagIterator, markdown, true));

                    prevTag = tagIterator.Current;
                    tagIterator.MoveNext();
                    nextTag = tagIterator.Current;
                }
                tagContentBuilder.Append(markdown.Substring(prevTagEndPos, nextTag.Position - prevTagEndPos));

                renderBuilder.Append(HtmlWriter.TagLine(openingTag.PatternName, tagContentBuilder.ToString()));
                prevTagEndPos = nextTag.Position + nextTag.PatternValue.Length;

                if (nested) return renderBuilder.ToString();
            }
            renderBuilder.Append(markdown.Substring(prevTagEndPos));

            return renderBuilder.ToString();
        }

        private static string CutBetweenTags(string markdown, MatchInfo prevTag, MatchInfo nextTag)
        {
            var cutStartIndex = prevTag.Position + prevTag.PatternValue.Length;
            var cutLength = nextTag.Position - cutStartIndex;
            var preNestedPart = markdown.Substring(cutStartIndex, cutLength);
            return preNestedPart;
        }

        public string RenderToHtml(string markdown)
        {
            using (var tagIterator = tagSearchTree
                .Find(markdown).Where(x => IsCorrectTag(markdown, x)).GetEnumerator())
            {
                return RecoursionTagRender(tagIterator, markdown, false);
            }
        }

        public bool IsCorrectTag(string markdown, MatchInfo tag)
        {
            if (tag.Position > 0)
            {
                return markdown[tag.Position - 1] != '\\';
            }

            return true;
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