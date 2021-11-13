namespace Lwt.Utilities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;

/// <inheritdoc />
public class TermCounter : ITermCounter
{
    private readonly ISqlTermRepository termRepository;
    private readonly ISkippedWordRemover skippedWordRemover;
    private readonly ITextNormalizer textNormalizer;
    private readonly ITextSeparator textSeparator;

    public TermCounter(
        ISqlTermRepository termRepository,
        ISkippedWordRemover skippedWordRemover,
        ITextNormalizer textNormalizer,
        ITextSeparator textSeparator)
    {
        this.termRepository = termRepository;
        this.skippedWordRemover = skippedWordRemover;
        this.textNormalizer = textNormalizer;
        this.textSeparator = textSeparator;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<LearningLevel, long>> CountByLearningLevelAsync(
        IEnumerable<string> words,
        LanguageCode languageCode,
        int userId)
    {
        IEnumerable<string> notSkippedTerms = this.skippedWordRemover.RemoveSkippedWords(words, languageCode);
        IEnumerable<string> notSkippedTermsNormalized =
            this.textNormalizer.Normalize(notSkippedTerms, languageCode);
        var termDict = new Dictionary<string, long>();

        foreach (string term in notSkippedTermsNormalized)
        {
            if (termDict.ContainsKey(term))
            {
                termDict[term] += 1;
            }
            else
            {
                termDict[term] = 1;
            }
        }

        Dictionary<string, LearningLevel> countDict =
            await this.termRepository.GetLearningLevelAsync(
                userId,
                languageCode,
                termDict.Keys.ToHashSet());

        var result = new Dictionary<LearningLevel, long>();
        IEnumerable<LearningLevel> learningLevels = LearningLevel.GetAll();
        foreach (LearningLevel termLearningLevel in learningLevels)
        {
            result[termLearningLevel] = 0;
        }

        foreach (string word in termDict.Keys)
        {
            if (!countDict.ContainsKey(word))
            {
                result[LearningLevel.Unknown] += termDict[word];
                continue;
            }

            result[countDict[word]] += termDict[word];
        }

        return result;
    }

    public long CountTermFromTextContent(Text text)
    {
        return this.textSeparator.SeparateText(text.Content, text.LanguageCode)
            .LongCount();
    }
}