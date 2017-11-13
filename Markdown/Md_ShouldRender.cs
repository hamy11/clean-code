using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_ShouldRender
    {
        private Md parser;

        [SetUp]
        public void SetUp()
        {
            parser = new Md();
        }

        [Test]
        public void RenderToHtml_ShouldThrowException_WhenMarkdownIsNull()
        {
            new Action(() => parser.RenderToHtml(null)).ShouldThrow<ArgumentException>();
        }

        [TestCase("", TestName = "Пустая строка")]
        [TestCase("string without tags", TestName = "Строка без тегов")]
        public void RenderToHtml_ShouldReturnOriginal(string str)
        {
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [TestCase("a _b_ c", "a <em>b</em> c", TestName = "Одиночные подчеркивания")]
        [TestCase("__b__ c", "<strong>b</strong> c", TestName = "Двойные подчеркивания")]
        public void RenderToHtml_ShouldRender_WhenNonNestedTags(string str, string expected)
        {
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }

        [TestCase("text _content_ abc _content1_ end", "text <em>content</em> abc <em>content1</em> end",
             TestName = "Два невложенных тега <em>")]
        [TestCase("text __content__ abc __content1__ end",
             "text <strong>content</strong> abc <strong>content1</strong> end",
             TestName = "Два невложенных тега <strong>")]
        [TestCase("text __c__ abc _c1_ abc2 __c3__ abc3 _c4_ end",
             "text <strong>c</strong> abc <em>c1</em> abc2 <strong>c3</strong> abc3 <em>c4</em> end",
             TestName = "Невложенные теги <strong> <em> <strong> <em>")]
        public void RenderToHtml_ShouldRender_WhenNonNastedTags(string baseString, string expected)
        {
            parser.RenderToHtml(baseString).Should().BeEquivalentTo(expected);
        }

        [TestCase("a __b _c_ d__ e", "a <strong>b <em>c</em> d</strong> e",
             TestName = "Одинарные подчеркивания внутри двойных")]
        [TestCase("a __b _c_ d _e_ f__ g", "a <strong>b <em>c</em> d <em>e</em> f</strong> g",
             TestName = "Две пары одинарных подчеркиваний внутри двойных")]
        [TestCase("a __b _c ~~d~~ e_ f__ g", "a <strong>b <em>c <strike>d</strike> e</em> f</strong> g",
             TestName = "Тильды внутри одинарных подчеркиваний внутри двойных")]
        public void RenderToHtml_ShouldRenderNestedTags(string str, string expected)
        {
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }

        [TestCase("a \\_b\\_ c", "a \\_b\\_ c", TestName = "Когда теги экранированы")]
        [TestCase("a b_12_3 c", "a b_12_3 c", TestName = "Когда теги внутри слова с цифрами")]
        [TestCase("a _b __c__ d_", "a <em>b __c__ d</em>", TestName = "Когда двойное подчеркивание внутри одинарного")]
        public void RenderToHtml_ShouldNotRenderDueToRules(string str, string expected)
        {
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }

        [TestCase("t _a end", "t _a end", TestName = "Когда открывающий тег бег пары")]
        [TestCase("__a _b c", "__a _b c", TestName = "Когда два разных открывающих тега бег пары")]
        [TestCase("t _a end", "t _a end", TestName = "Когда открывающий тег бег пары")]
        [TestCase("t a_ end_ b", "t a_ end_ b", TestName = "Когда два закрывающих тега бег пары")]
        [TestCase("t _a __b  end", "t _a __b  end", TestName = "Когда два открывающих тега бег пары")]
        [TestCase("t _a _a c d_ end", "t _a <em>a c d</em> end",
             TestName = "Когда два открывающих и один закрывающий тег")]
        [TestCase("t _a __b d_ end", "t <em>a __b d</em> end", TestName = "Когда открывающий тег __ внутри пары тегов _"
         )]
        [TestCase("t _a __b __c d_ end", "t <em>a __b __c d</em> end",
             TestName = "Когда два открывающих тега __ внутри пары тегов _")]
        public void RenderToHtml_ShouldRender_WhenTagsWithoutPairExists(string str, string expected)
        {
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }
    }

    [TestFixture]
    public class SearchTreeShould
    {

        

        [Test]
        public void RoundMatches_ShouldFindMaxState()
        {
            var searchTree = new SearchTree();
            searchTree.Add("_");
            searchTree.Add("__");
            searchTree.Add("___");
            searchTree.Build();
            var result = searchTree.RoundMatches("___").ToList();
            var expected = new List<IMatch> {new PatternMatch("___", 0)};
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void RoundMatches_ShouldFind_WhenDifferentPatterns()
        {
            var searchTree = new SearchTree();
            searchTree.Add("~");
            searchTree.Add("_");
            searchTree.Add("_~");
            searchTree.Add("__");
            searchTree.Build();
            var result = searchTree.RoundMatches("__ _~ _~~").ToList();
            var expected = new List<IMatch>
            {
                new PatternMatch("__", 0),
                new SymbolMatch(' ', 2),
                new PatternMatch("_~", 3),
                new SymbolMatch(' ', 5),
                new PatternMatch("_~", 6),
                new PatternMatch("~", 8)
            };
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void RoundMatches_ShouldThrow_WhenRoundingWithoutBuild()
        {
            var searchTree = new SearchTree();
            searchTree.Add("_");
            searchTree.Build();
            searchTree.Add("~~");
            new Action(() => searchTree.RoundMatches("~_~~").ToList()).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void RoundMatches_ShouldFind_WhenAddPatternAfterBuild()
        {
            var searchTree = new SearchTree();
            searchTree.Add("_");
            searchTree.Build();
            searchTree.Add("___");
            searchTree.Build();
            var result = searchTree.RoundMatches("___").ToList();
            var expected = new List<IMatch> { new PatternMatch("___", 0) };
            CollectionAssert.AreEqual(expected, result);
        }
    }
}