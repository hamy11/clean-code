using System;
using System.Collections;
using System.Collections.Generic;

namespace Markdown
{
    public interface IMatch
    {

    }

    public class PatternMatch : IMatch
    {
        public readonly string PatternValue;
        public readonly int Position;

        public PatternMatch(string patternValue, int position)
        {
            PatternValue = patternValue;
            Position = position;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (PatternMatch) obj;
            return p.PatternValue == PatternValue && p.Position == Position;
        }

        public override int GetHashCode()
        {
            return PatternValue.GetHashCode() ^ Position;
        }
    }

    public class SymbolMatch : IMatch
    {
        public readonly char Symbol;
        public readonly int Position;

        public SymbolMatch(char symbol, int position)
        {
            Symbol = symbol;
            Position = position;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (SymbolMatch) obj;
            return p.Symbol == Symbol && p.Position == Position;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode() ^ Position;
        }
    }

    public class SearchTree //Aho-Corasick search tree
    {
        private readonly Node<char> root = new Node<char>();
        private bool hasBuilded;

        public void Add(IEnumerable<char> pattern)
        {
            if (pattern == null)
                throw new ArgumentException();
            var node = root;
            foreach (var symbol in pattern)
            {
                var child = node[symbol] ?? (node[symbol] = new Node<char>(symbol, node));
                node = child;
            }
            node.CurrentPrefix = pattern.ToString();
            hasBuilded = false;
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
            hasBuilded = true;
        }

        public IEnumerable<IMatch> RoundMatches(string text)
        {
            if (!hasBuilded)
                throw new InvalidOperationException();

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