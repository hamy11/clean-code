using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class Md
    {
        private readonly Tree tagSearchTree;
        private readonly HtmlWriter htmlWriter;
        private readonly Stack<Tag> noPairTagStack = new Stack<Tag>();

        public Md()
        {
            var tagDictionary = new Dictionary<string, string>
            {
                {"_", "em"},
                {"__", "strong"},
                {"~~", "strike"}
            };
            htmlWriter = new HtmlWriter(tagDictionary);
            tagSearchTree = BuildTagSearchTree(tagDictionary);
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

        public string RenderToHtml(string markdown)
        {
            using (var entityIterator = tagSearchTree.Find(markdown).GetEnumerator())
            {
                bool pairTagFounded;
                return RecoursionTagRender(entityIterator, markdown, out pairTagFounded);
            }
        }

        private string RecoursionTagRender(IEnumerator<IMatchType> entityIterator, string markdown,
            out bool isPairTagFounded)
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

                var tagMatch = (PatternMatch) entityIterator.Current;
                var tag = htmlWriter.GetTagEntity(tagMatch.PatternValue,tagMatch.Position, GetTagType(tagMatch, markdown));
                if (noPairTagStack.Any() && IsPair(tag, noPairTagStack.Peek()))
                {
                    noPairTagStack.Pop();
                    isPairTagFounded = true;
                    return renderBuilder.ToString();
                }

                noPairTagStack.Push(tag);
                var currentTagContent = RecoursionTagRender(entityIterator, markdown, out isPairTagFounded);
                var renderedLine = RenderLine(isPairTagFounded, tag, currentTagContent, markdown);

                renderBuilder.Append(renderedLine);
            }
            isPairTagFounded = false;
            return renderBuilder.ToString();
        }

        private string RenderLine(bool isPairTagFounded, Tag tag, string currentTagContent, string markdown)
        {
            return isPairTagFounded
                ? CheckRules(tag, noPairTagStack, markdown)
                    ? $"<{tag.Name}>{currentTagContent}</{tag.Name}>"
                    : $"{tag.Definition}{currentTagContent}{tag.Definition}"
                : $"{tag.Definition}{currentTagContent}";
        }

        private static bool CheckRules(Tag currentTag, Stack<Tag> noPairTagStack, string markdown)
        {
            var rules = new List<Func<bool>>
            {
                () => !(currentTag.Name == "strong" && noPairTagStack.Any() && noPairTagStack.Peek().Name == "em"),
                () => !(currentTag.Position > 0 && markdown[currentTag.Position - 1] == '\\')
            };

            return rules.All(rule => rule());
        }

        private static TagType GetTagType(PatternMatch tagMatch, string markdown)
        {
            var previousSymbolExists = tagMatch.Position > 0;
            var isNotOpeningTag = previousSymbolExists && markdown[tagMatch.Position - 1] != ' ';
            var followingSymbolExists = tagMatch.Position + tagMatch.PatternValue.Length < markdown.Length - 1;
            var isNotClosingTag = followingSymbolExists &&
                                  markdown[tagMatch.Position + tagMatch.PatternValue.Length] != ' ';

            if (isNotClosingTag && isNotOpeningTag)
                return TagType.Fake;

            return isNotClosingTag ? TagType.Opening : TagType.Closing;
        }

        private static bool IsPair(Tag currentTag, Tag previousTag)
        {
            return currentTag.Definition == previousTag.Definition && currentTag.Type != previousTag.Type;
        }
    }
}