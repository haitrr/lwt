namespace Lwt.Utilities;

using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.TokenAttributes;
using Lwt.Interfaces;

/// <summary>
/// Japanese text splitter.
/// </summary>
public class JapaneseTextSplitter : IJapaneseTextSplitter
{
    /// <inheritdoc />
    public IEnumerable<string> Split(string text)
    {
        var terms = new List<string>();

        using (var reader = new StringReader(text))
        {
            using (var tokenizer = new JapaneseTokenizer(reader, null, false, JapaneseTokenizerMode.NORMAL))
            {
                tokenizer.Reset();

                while (tokenizer.IncrementToken())
                {
                    var term = tokenizer.GetAttribute<ICharTermAttribute>();
                    terms.Add(term.ToString());
                }
            }
        }

        return terms;
    }
}