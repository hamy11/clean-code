using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class TagAnalyser
    {
        public static bool CheckTagRules(Tag currentTag, Stack<Tag> noPairTagStack, string markdown)
        {
            var rules = new List<Func<bool>>
            {
                () => !(currentTag.Name == "strong" && noPairTagStack.Any(x => x.Name == "em")),
                () => !(currentTag.Position > 0 && markdown[currentTag.Position - 1] == '\\')
            };

            return rules.All(rule => rule());
        }

        public static TagType GetTagType(PatternMatch tagMatch, string markdown)
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
    }
}