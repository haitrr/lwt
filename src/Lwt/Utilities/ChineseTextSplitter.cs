namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using JiebaNet.Segmenter;
    using Lwt.Interfaces;

    /// <inheritdoc />
    public class ChineseTextSplitter : IChineseTextSplitter
    {
        private readonly JiebaSegmenter segmenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChineseTextSplitter"/> class.
        /// </summary>
        public ChineseTextSplitter()
        {
            this.segmenter = new JiebaSegmenter();
            this.segmenter.LoadUserDict("dicts/dict.txt.big");
            this.segmenter.LoadUserDict("dicts/idf.txt.big");
        }

        /// <inheritdoc />
        public IEnumerable<string> Split(string text)
        {
            return this.segmenter.Cut(text, false, false);
        }
    }
}