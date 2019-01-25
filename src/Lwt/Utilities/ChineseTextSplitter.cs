using Lwt.Interfaces;

namespace Lwt.Models
{
    using System.Collections.Generic;
    using JiebaNet.Segmenter;

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
        }

        /// <inheritdoc />
        public IEnumerable<string> Split(string text)
        {
            return this.segmenter.Cut(text);
        }
    }
}