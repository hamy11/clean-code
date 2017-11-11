using System.Collections;
using System.Collections.Generic;

namespace Markdown
{
    public interface IMatchType
    {

    }

    public class TagMatch : IMatchType
    {
        public string TagDefinition;
        public string TagName;
        public int Position;

        public TagMatch(string tagName, string tagDefinition, int position)
        {
            TagDefinition = tagDefinition;
            TagName = tagName;
            Position = position;
        }

        public override string ToString()
        {
            return $"tagName - {TagName}; tagDefinition - {TagDefinition}; position - {Position};";
        }
    }

    public class SymbolMatch : IMatchType
    {
        public char Symbol;
        public int Position;

        public SymbolMatch(char symbol, int position)
        {
            Symbol = symbol;
            Position = position;
        }
    }

    public class Tree //Aho-Corasick search tree
    {
        private readonly Node<char, string> root = new Node<char, string>();

        public void Add(string s)
        {
            Add(s, s);
        }

        public void Add(KeyValuePair<string, string> pair)
        {
            Add(pair.Key, pair.Value);
        }

        public void Add(IEnumerable<char> word, string value)
        {
            var node = root;
            foreach (var symbol in word)
            {
                var child = node[symbol] ?? (node[symbol] = new Node<char, string>(symbol, node));
                node = child;
            }

            node.CurrentPrefix = word.ToString();
            node.Value = value;
        }

        public void Build()
        {
            var queue = new Queue<Node<char, string>>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                // visit children
                foreach (var child in node)
                    queue.Enqueue(child);

                // fail link of root is root
                if (node == root)
                {
                    root.FailState = root;
                    continue;
                }

                var fail = node.Parent.FailState;

                while (fail[node.Word] == null && fail != root)
                    fail = fail.FailState;

                node.FailState = fail[node.Word] ?? root;
                if (node.FailState == node)
                    node.FailState = root;
            }
        }

        public IEnumerable<IMatchType> Find(string text)
        {
            var node = root;
            for (var i = 0; i < text.Length; i++)
            {
                var symbol = text[i];
                while (node[symbol] == null && node != root)
                    node = node.FailState;

                node = node[symbol] ?? root;

                //если это не одно из состояний автомата (т.е данный символ это часть текста, а не какого-либо тега)
                if (node == root)
                {
                    yield return new SymbolMatch(text[i], i);
                    continue;
                }

                var nextSymbolPosition = i + 1;
                var nextNodeExists = nextSymbolPosition < text.Length && node[text[nextSymbolPosition]] != null;

                // проверяем, есть ли следующее состояние автомата, включающее текущее
                if (!nextNodeExists) 
                    yield return new TagMatch(node.Value, node.CurrentPrefix, i - node.CurrentPrefix.Length + 1);
            }
        }

        private class Node<TNode, TNodeValue> : IEnumerable<Node<TNode, TNodeValue>>
        {
            private readonly Dictionary<TNode, Node<TNode, TNodeValue>> children =
                new Dictionary<TNode, Node<TNode, TNodeValue>>();

            public Node()
            {
            }

            public Node(TNode word, Node<TNode, TNodeValue> parent)
            {
                Word = word;
                Parent = parent;
            }

            public TNode Word { get; }

            public string CurrentPrefix { get; set; }

            public Node<TNode, TNodeValue> Parent { get; }

            public Node<TNode, TNodeValue> FailState { get; set; }

            public Node<TNode, TNodeValue> this[TNode child]
            {
                get { return children.ContainsKey(child) ? children[child] : null; }
                set { children[child] = value; }
            }

            public TNodeValue Value { get; set; }

            public IEnumerator<Node<TNode, TNodeValue>> GetEnumerator()
            {
                return children.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}