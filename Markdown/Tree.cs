using System.Collections;
using System.Collections.Generic;

namespace Markdown
{
    public interface IMatchType
    {

    }

    public class PatternMatch : IMatchType
    {
        public string PatternValue;
        public int Position;

        public PatternMatch(string patternValue, int position)
        {
            PatternValue = patternValue;
            Position = position;
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
        private readonly Node<char> root = new Node<char>();

        public void Add(IEnumerable<char> pattern)
        {
            var node = root;
            foreach (var symbol in pattern)
            {
                var child = node[symbol] ?? (node[symbol] = new Node<char>(symbol, node));
                node = child;
            }
            node.CurrentPrefix = pattern.ToString();
        }

        public void Build()
        {
            var queue = new Queue<Node<char>>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                foreach (var child in node)
                    queue.Enqueue(child);

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

        public IEnumerable<IMatchType> RoundMatches(string text)
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
                    yield return new PatternMatch(node.CurrentPrefix, i - node.CurrentPrefix.Length + 1);
            }
        }

        private class Node<TNode> : IEnumerable<Node<TNode>>
        {
            private readonly Dictionary<TNode, Node<TNode>> children =
                new Dictionary<TNode, Node<TNode>>();

            public Node()
            {
            }

            public Node(TNode word, Node<TNode> parent)
            {
                Word = word;
                Parent = parent;
            }

            public TNode Word { get; }

            public string CurrentPrefix { get; set; }

            public Node<TNode> Parent { get; }

            public Node<TNode> FailState { get; set; }

            public Node<TNode> this[TNode child]
            {
                get { return children.ContainsKey(child) ? children[child] : null; }
                set { children[child] = value; }
            }

            public IEnumerator<Node<TNode>> GetEnumerator()
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