namespace Lwt.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Lucene.Net.Analysis.Ja;
    using Lucene.Net.Analysis.TokenAttributes;
    using Lwt.Interfaces;

    /// <summary>
    /// Japanese text splitter.
    /// </summary>
    public class JapaneseTextSplitter : IJapaneseTextSplitter, System.IDisposable
    {
        private readonly JapaneseTokenizer segmenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseTextSplitter"/> class.
        /// </summary>
        public JapaneseTextSplitter()
        {
            this.segmenter = new JapaneseTokenizer(null, null, false, JapaneseTokenizerMode.NORMAL);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="JapaneseTextSplitter"/> class.
        /// </summary>
        ~JapaneseTextSplitter()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public IEnumerable<string> Split(string text)
        {
            this.segmenter.SetReader(new StringReader(text));
            this.segmenter.Reset();
            var terms = new List<string>();

            while (this.segmenter.IncrementToken())
            {
                var term = this.segmenter.GetAttribute<ICharTermAttribute>();
                terms.Add(term.ToString());
            }

            return terms;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// dispose resource.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.ReleaseUnmanagedResources();

            if (disposing)
            {
                this.segmenter?.Dispose();
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }
    }
}