using System;
using System.Collections.Generic;

namespace Markdown
{
    public class HtmlWriter
    {
        private static Dictionary<string, string> TagDictionary;

        public Func<string, int, TagType, Tag> GetTagEntity =
            (definition, position, type) => new Tag(definition, position, TagDictionary[definition], type);

        public HtmlWriter(Dictionary<string, string> tagDictionary)
        {
            TagDictionary = tagDictionary;
        }
    }

    public class Tag
    {
        public string Definition;
        public string Name;
        public int Position;
        public TagType Type;

        public Tag(string definition, int position, string name, TagType type)
        {
            Definition = definition;
            Position = position;
            Name = name;
            Type = type;
        }
    }

    public enum TagType
    {
        Fake = 0,
        Opening = 1,
        Closing = 2
    }
}