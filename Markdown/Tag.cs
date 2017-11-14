namespace Markdown
{
    public class Tag
    {
        public readonly string Definition;
        public readonly string Name;
        public readonly int Position;
        public readonly TagType Type;

        public Tag(string definition, int position, string name, TagType type)
        {
            Definition = definition;
            Position = position;
            Name = name;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (Tag) obj;
            return p.Definition == Definition;
        }

        public override int GetHashCode()
        {
            return Definition.GetHashCode();
        }

        public override string ToString()
        {
            return $"name: {Name}; Definition: {Definition}; Type: {Type}; Pos: {Position};";
        }
    }

    public static class TagExtensions
    {
        public static bool IsPairTo(this Tag currentTag, Tag previousTag)
            => previousTag != null && currentTag.Definition == previousTag.Definition && currentTag.Type != previousTag.Type;
    }
}