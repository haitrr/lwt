namespace Lwt.Creators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;

    public class TextTermProcessor : ITextTermProcessor
    {
        private readonly ITextSeparator textSeparator;
        private readonly ISqlTermRepository termRepository;
        private readonly ITextTermRepository textTermRepository;
        private readonly ILanguageHelper languageHelper;
        private readonly ITextNormalizer textNormalizer;
        private readonly IDbTransaction dbTransaction;
        private readonly ISqlTextRepository textRepository;
        private readonly ILogger<ITextTermProcessor> logger;

        public TextTermProcessor(
            ITextSeparator textSeparator,
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository,
            ILanguageHelper languageHelper,
            ITextNormalizer textNormalizer,
            IDbTransaction dbTransaction,
            ISqlTextRepository textRepository,
            ILogger<ITextTermProcessor> logger)
        {
            this.textSeparator = textSeparator;
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
            this.languageHelper = languageHelper;
            this.textNormalizer = textNormalizer;
            this.dbTransaction = dbTransaction;
            this.textRepository = textRepository;
            this.logger = logger;
        }

        public async Task ProcessTextTermAsync()
        {
            using (IDbContextTransaction transaction = this.dbTransaction.BeginTransaction())
            {
                Text? processingText = await this.textRepository.Queryable()
                    .AsNoTracking()
                    .Where(t => t.ProcessedTermCount < t.TermCount || t.ProcessedTermCount == 0)
                    .FirstOrDefaultAsync();

                if (processingText == null)
                {
                    this.logger.LogInformation("No text to process.");
                    return;
                }

                if (processingText.TermCount == 0)
                {
                    this.logger.LogInformation($"New or updated text, set term count");
                    this.logger.LogInformation($"Processing new or reset, removing old text term");

                    if (this.textTermRepository.Queryable()
                        .Where(t => t.TextId == processingText.Id)
                        .Take(10000)
                        .DeleteFromQuery() != 0)
                    {
                        await transaction.CommitAsync();
                        return;
                    }
                }

                List<string> words = this.textSeparator
                    .SeparateText(processingText.Content, processingText.LanguageCode)
                    .ToList();

                this.logger.LogInformation($"Processing text {processingText.Id}");

                if (processingText.TermCount == 0)
                {
                    this.logger.LogInformation($"New or updated text, set term count");
                    this.logger.LogInformation($"Reset term count");

                    processingText.TermCount = words.Count;
                    processingText.ProcessedTermCount = 0;
                    this.textRepository.UpdateTermCountAndProcessedTermCount(processingText);
                    await this.dbTransaction.CommitAsync();
                    await transaction.CommitAsync();
                    return;
                }

                if (processingText.TermCount != words.Count)
                {
                    this.logger.LogInformation("Text separator changed , resetting processing");
                    processingText.ProcessedTermCount = 0;
                    processingText.TermCount = 0;
                    this.textRepository.UpdateTermCountAndProcessedTermCount(processingText);
                    await this.dbTransaction.CommitAsync();
                    await transaction.CommitAsync();
                    return;
                }

                long indexFrom = processingText.ProcessedTermCount;
                var processingWordCount = 1000;
                this.logger.LogInformation(
                    $"Processing text terms from {indexFrom} to {indexFrom + processingWordCount}");
                ILanguage language = this.languageHelper.GetLanguage(processingText.LanguageCode);

                if (indexFrom > int.MaxValue)
                {
                    throw new Exception("this text is too big to be processed");
                }

                string[] processingWords = words.Skip((int)indexFrom)
                    .Take(processingWordCount)
                    .ToArray();

                var termContentSet = new HashSet<string>();

                foreach (string word in processingWords.Where(word => !language.ShouldSkip(word)))
                {
                    termContentSet.Add(this.textNormalizer.Normalize(word, processingText.LanguageCode));
                }

                IEnumerable<Term> terms = await this.termRepository.TryGetManyByUserAndLanguageAndContentsAsync(
                    processingText.UserId,
                    processingText.LanguageCode,
                    termContentSet);

                Dictionary<string, Term> termDict = terms.ToDictionary(t => t.Content, t => t);

                List<Term> newTerms = this.MapTerms(processingWords, processingText, termDict, language);

                this.termRepository.BulkInsert(newTerms);

                List<TextTerm> textTerms = this.GetTextTerms(processingWords, indexFrom, processingText, termDict);

                this.logger.LogInformation("Saving change");
                this.textTermRepository.BulkInsert(textTerms);
                processingText.ProcessedTermCount = indexFrom + processingWords.Length;
                this.textRepository.UpdateTermCountAndProcessedTermCount(processingText);
                await this.dbTransaction.CommitAsync();
                await transaction.CommitAsync();
            }
        }

        private List<Term> MapTerms(
            string[] processingWords,
            Text processingText,
            Dictionary<string, Term> termDict,
            ILanguage language)
        {
            var newTerms = new List<Term>();

            for (var i = 0; i < processingWords.Count(); i += 1)
            {
                string word = processingWords[i];
                string normalizedWord = this.textNormalizer.Normalize(word, processingText.LanguageCode);

                if (!termDict.ContainsKey(normalizedWord) && !language.ShouldSkip(normalizedWord))
                {
                    var term = new Term
                    {
                        LanguageCode = processingText.LanguageCode,
                        Content = normalizedWord,
                        UserId = processingText.UserId,
                        LearningLevel = LearningLevel.Unknown,
                        Meaning = string.Empty,
                    };
                    termDict[normalizedWord] = term;
                    newTerms.Add(term);
                }
            }

            return newTerms;
        }

        private List<TextTerm> GetTextTerms(
            string[] processingWords,
            long indexFrom,
            Text processingText,
            Dictionary<string, Term> termDict)
        {
            var textTerms = new List<TextTerm>();

            for (var i = 0; i < processingWords.Length; i += 1)
            {
                string word = processingWords[i];
                long index = indexFrom + i;
                Term? term = null;
                string normalizedWord = this.textNormalizer.Normalize(word, processingText.LanguageCode);

                if (termDict.ContainsKey(normalizedWord))
                {
                    term = termDict[normalizedWord];
                }

                textTerms.Add(
                    new TextTerm { TermId = term?.Id, Content = word, Index = index, TextId = processingText.Id });
            }

            return textTerms;
        }
    }
}