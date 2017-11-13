using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class Md
    {
        private readonly SearchTree tagSearchTree;
        private readonly HtmlWriter htmlWriter;
        private readonly Stack<Tag> openedTagsStack = new Stack<Tag>();
        private readonly TagsSearchStatusHandler handler = new TagsSearchStatusHandler();

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

        public SearchTree BuildTagSearchTree(Dictionary<string, string> tagDictionary)
        {
            var tree = new SearchTree();
            foreach (var pair in tagDictionary)
            {
                tree.Add(pair.Key);
            }
            tree.Build();
            return tree;
        }

        public string RenderToHtml(string markdown)
        {
            if (markdown == null) throw new ArgumentException();
            using (var entityIterator = tagSearchTree.RoundMatches(markdown).GetEnumerator())
            {
                return RecoursionTagRender(entityIterator, markdown);
            }
        }

        private string RecoursionTagRender(IEnumerator<IMatch> entityIterator, string markdown)
        {
            var renderBuilder = new StringBuilder();
            while (!handler.NoPairTagDetected && entityIterator.MoveNext() && entityIterator.Current != null)
            {
                if (TrySaveSymbol(entityIterator.Current, renderBuilder)) continue;

                var tag = htmlWriter.PatternMatchAsTag(entityIterator.Current, markdown);

                if (handler.IsClosingTagFounded(tag, openedTagsStack)) return renderBuilder.ToString();

                if (tag.Type == TagType.Opening) openedTagsStack.Push(tag);

                var currentTagContent = RecoursionTagRender(entityIterator, markdown);
                var isCorrectTag = TagAnalyser.CheckTagRules(tag, openedTagsStack, markdown);

                handler.UpdateSearchStatuses(tag);

                var renderedLine = HtmlWriter.RenderLine(tag, currentTagContent, handler.PairTagFounded, isCorrectTag);
                renderBuilder.Append(renderedLine);
            }
            handler.PairTagFounded = false;

            return renderBuilder.ToString();
        }

        private class TagsSearchStatusHandler
        {
            public bool NoPairTagDetected;
            public bool PairTagFounded;
            private Tag closingTag;

            public bool IsClosingTagFounded(Tag tag, Stack<Tag> openedTagsStack)
            {
                if (!openedTagsStack.Any() || tag.Type != TagType.Closing) return false;
                if (tag.IsPairTo(openedTagsStack.Peek()))
                {
                    NoPairTagDetected = false;
                    PairTagFounded = true;
                }
                else
                {
                    NoPairTagDetected = true;
                    closingTag = tag;
                }
                openedTagsStack.Pop();
                return true;
            }

            public void UpdateSearchStatuses(Tag tag)
            {
                if (!NoPairTagDetected || !tag.IsPairTo(closingTag)) return;
                //Когда по рекурсии поднялись вверх через беспарные теги и нашли парный
                PairTagFounded = true;
                NoPairTagDetected = false;
            }
        }

        private static bool TrySaveSymbol(IMatch currentEntity, StringBuilder renderBuilder)
        {
            if (currentEntity.GetType() != typeof(SymbolMatch)) return false;
            var symbolMatch = (SymbolMatch) currentEntity;
            renderBuilder.Append(symbolMatch.Symbol);
            return true;
        }
    }
}