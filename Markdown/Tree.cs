using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
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
            node.Values.Add(value);
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
                    root.Fail = root;
                    continue;
                }

                var fail = node.Parent.Fail;

                while (fail[node.Word] == null && fail != root)
                    fail = fail.Fail;

                node.Fail = fail[node.Word] ?? root;
                if (node.Fail == node)
                    node.Fail = root;
            }
        }

        public IEnumerable<MatchInfo> Find(string text)
        {
            var node = root;
            for (var i = 0; i < text.Length; i++)
            {
                var symbol = text[i];
                while (node[symbol] == null && node != root)
                    node = node.Fail;

                node = node[symbol] ?? root;

                if (node == root) continue;

                var nextSymbolPosition = i + 1;
                var nextNodeExists = nextSymbolPosition < text.Length && node[text[nextSymbolPosition]] != null;
                if (!nextNodeExists) // проверяем есть ли следующее состояние автомата, включающее текущее
                    yield return
                        new MatchInfo(node.Values.First(), node.CurrentPrefix, i - node.CurrentPrefix.Length + 1);
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

            public Node<TNode, TNodeValue> Fail { get; set; }

            public Node<TNode, TNodeValue> this[TNode c]
            {
                get { return children.ContainsKey(c) ? children[c] : null; }
                set { children[c] = value; }
            }

            public List<TNodeValue> Values { get; } = new List<TNodeValue>();

            public IEnumerator<Node<TNode, TNodeValue>> GetEnumerator()
            {
                return children.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string ToString()
            {
                return Word.ToString();
            }
        }
    }

    public class MatchInfo
    {
        public string PatternName;
        public string PatternValue;
        public int Position;

        public MatchInfo(string patternName, string patternValue, int position)
        {
            PatternName = patternName;
            PatternValue = patternValue;
            Position = position;
        }

        public override string ToString()
        {
            return $"patternName - {PatternName}; position - {Position};";
        }
    }
}