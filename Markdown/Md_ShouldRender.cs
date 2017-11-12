using System;
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

        [Test]
        public void RenderToHtml_ShouldReturnOriginal_WhenNoUnderlines()
        {
            const string str = "string without tags";
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [Test]
        public void RenderToHtml_ShouldRenderEmTag_WhenSurroundedBySingleUnderlines()
        {
            const string str = "Текст _окруженный с двух сторон_ одинарными символами подчерка";
            const string expected = "Текст <em>окруженный с двух сторон</em> одинарными символами подчерка";
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void RenderToHtml_ShouldRenderStrongTag_WhenSurroundedByDoubleUnderlines()
        {
            const string str = "__Двумя символами__ — должен становиться жирным с помощью тега <strong>.";
            const string result =
                "<strong>Двумя символами</strong> — должен становиться жирным с помощью тега <strong>.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(result);
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

        [Test]
        public void RenderToHtml_ShouldRenderEmWithinStrong_WhenSingleUnderlinesWithinDoubleUnderlines()
        {
            const string str = "Внутри __двойного выделения _одинарное_ тоже__ работает.";
            const string result = "Внутри <strong>двойного выделения <em>одинарное</em> тоже</strong> работает.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(result);
        }

        [Test]
        public void RenderToHtml_ShouldRenderEmWithinStrong_WhenAlotSingleUnderlinesWithinDoubleUnderlines()
        {
            const string str = "начало __пре _нестед1_ центр _нестед2_ паст__ конец.";
            const string result = "начало <strong>пре <em>нестед1</em> центр <em>нестед2</em> паст</strong> конец.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(result);
        }

        [Test]
        public void RenderToHtml_ShouldRender_WhenStrikeWithinSingleUnderlineWithinDoubleUnderline()
        {
            const string str = "Внутри __двойного _одинарное ~~зачеркнутое~~ конец_ тоже__ работает.";
            const string result =
                "Внутри <strong>двойного <em>одинарное <strike>зачеркнутое</strike> конец</em> тоже</strong> работает.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(result);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenShieldedSingleUnderlines()
        {
            const string str = "Не часть разметки. \\_Вот это\\_ , не должно выделиться тегом < em>.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenDoubleUnderlinesWithinSingleUnderlines()
        {
            const string str = "Но не наоборот — внутри _одинарного __двойное__ не работает_.";
            const string expected = "Но не наоборот — внутри <em>одинарного __двойное__ не работает</em>.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenUnderlinesWithinTextWithNumbers()
        {
            const string str = "Подчерки внутри текста c цифрами_12_3 не считаются выделением.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenNotAPairTags()
        {
            const string str = "__непарные _символы не считаются выделением.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenOnlyClosingTags()
        {
            const string str =
                "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением и остаются просто символами подчерка.";
            parser.RenderToHtml(str).Should().BeEquivalentTo(str);
        }

        [Test]
        public void RenderToHtml_ShouldNotRender_WhenOnlyOpenTags()
        {
            const string str = "Подчерки, заканчивающие _выделение _не считаются_ окончанием выделения";
            const string expected = "Подчерки, заканчивающие _выделение <em>не считаются</em> окончанием выделения";
            parser.RenderToHtml(str).Should().BeEquivalentTo(expected);
        }
    }
}