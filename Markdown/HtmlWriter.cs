using System;
using System.IO;

namespace Markdown
{
    public class HtmlWriter : IDisposable
    {
        private readonly StreamReader sReader;
        private readonly StreamWriter sWriter;

        public HtmlWriter(Stream stream)
        {
            sReader = new StreamReader(stream);
            sWriter = new StreamWriter(stream);
        }

        public string Read()
        {
            sWriter.Flush();
            sReader.BaseStream.Position = 0;
            return sReader.ReadToEnd();
        }

        public void Write(string content)
        {
            sWriter.Write(content);
        }

        private bool isDisposed;

        ~HtmlWriter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool fromDisposeMethod)
        {
            if (isDisposed) return;
            if (fromDisposeMethod)
            {
                sWriter.Dispose();
            }
            isDisposed = true;
        }

        public TagToken Tag(string tagName)
        {
            sWriter.Write($"<{tagName}>");
            return new TagToken(this, tagName);
        }

        public class TagToken : IDisposable
        {
            private readonly HtmlWriter writer;
            private readonly string tag;

            public TagToken(HtmlWriter writer, string tag)
            {
                this.writer = writer;
                this.tag = tag;
            }

            public void Dispose()
            {
                writer.sWriter.Write($"</{tag}>");
            }
        }
    }
}