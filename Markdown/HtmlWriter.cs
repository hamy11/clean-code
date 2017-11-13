using System;
using System.Collections.Generic;

namespace Markdown
{
    public class HtmlWriter
    {
        private static Dictionary<string, string> TagDictionary;

        public static Func<string, int, TagType, Tag> GetTagEntity =
            (definition, position, type) => new Tag(definition, position, TagDictionary[definition], type);

        public HtmlWriter(Dictionary<string, string> tagDictionary)
        {
            TagDictionary = tagDictionary;
        }

        public Tag PatternMatchAsTag(IMatch currentEntity, string markdown)
        {
            var tagMatch = (PatternMatch) currentEntity;
            return GetTagEntity(tagMatch.PatternValue, tagMatch.Position,
                TagAnalyser.GetTagType(tagMatch, markdown));
        }

        public static string RenderLine(Tag tag, string currentTagContent, bool isPairTagFounded, bool isCorrectTag)
        {
            return isPairTagFounded
                ? isCorrectTag
                    ? $"<{tag.Name}>{currentTagContent}</{tag.Name}>"
                    : $"{tag.Definition}{currentTagContent}{tag.Definition}"
                : $"{tag.Definition}{currentTagContent}";
        }
    }
}