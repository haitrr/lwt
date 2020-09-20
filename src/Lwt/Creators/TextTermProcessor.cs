namespace Lwt.Creators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;

    public class TextTermProcessor : ITextTermProcessor
    {
        private readonly IDbTransaction dbTransaction;
        private readonly ILogger<ITextTermProcessor> logger;
        private readonly ISqlTermRepository termRepository;
        private readonly ISqlTextRepository textRepository;
        private readonly ITextTermRepository textTermRepository;

        public TextTermProcessor(
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository,
            IDbTransaction dbTransaction,
            ISqlTextRepository textRepository,
            ILogger<ITextTermProcessor> logger)
        {
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
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
                    .Where(t => t.ProcessedIndex < t.Length - 1)
                    .FirstOrDefaultAsync();

                if (processingText == null)
                {
                    this.logger.LogInformation("No text to process.");
                    return;
                }

                if (processingText.ProcessedIndex == -1)
                {
                    if (this.RemoveExistingTextTerms(processingText.Id))
                    {
                        await transaction.CommitAsync();
                        return;
                    }
                }

                this.logger.LogInformation($"Processing text {processingText.Id}");

                int indexFrom = processingText.ProcessedIndex + 1;
                var processingCharCount = 100;
                int indexTo = Math.Min(indexFrom + processingCharCount, processingText.Content.Length - 1);
                this.logger.LogInformation($"Processing text content index from {indexFrom} to {indexTo}");

                List<Term>? userTerms = await this.termRepository.Queryable()
                    .Where(t => t.UserId == processingText.UserId && t.LanguageCode == processingText.LanguageCode)
                    .ToListAsync();

                Dictionary<string, Term> contentDict = userTerms.ToDictionary(t => t.Content, t => t);
                List<string> dictionary = userTerms.Select(t => t.Content)
                    .ToList();

                int finish = Finish(
                    indexFrom,
                    processingText,
                    dictionary,
                    indexTo,
                    contentDict,
                    out List<TextTerm> textTerms);

                processingText.ProcessedIndex = finish;
                processingText.TermCount += textTerms.Count;
                this.textTermRepository.BulkInsert(textTerms);
                this.textRepository.UpdateTermCountAndProcessedTermCount(processingText);
                await this.dbTransaction.CommitAsync();
                await transaction.CommitAsync();
            }
        }

        private static int Finish(
            int indexFrom,
            Text processingText,
            List<string> dictionary,
            int indexTo,
            Dictionary<string, Term> contentDict,
            out List<TextTerm> textTerms)
        {
            int start = indexFrom;
            int end = indexFrom;
            string content = processingText.Content;
            IEnumerable<string> matches = dictionary;
            int finish = indexFrom;
            textTerms = new List<TextTerm>();
            string? match = null;

            while (end < indexTo)
            {
                string current = content[start.. (end + 1)];
                matches = matches.Where(t => t.AsParallel().StartsWith(current.ToUpperInvariant()));
                int count = matches.Count();

                if (count == 1)
                {
                    string? m = matches.First();
                    end = start + m.Length - 1;

                    if (end > indexTo)
                    {
                        if (end > processingText.Length - 1)
                        {
                            count = 0;
                        }
                        else
                        {
                            finish = start - 1;
                            break;
                        }
                    }
                    else
                    {
                        match = m;
                        current = content[start.. (end + 1)];

                        if (current.ToUpperInvariant() != match)
                        {
                            count = 0;
                            match = null;
                        }
                        else
                        {
                            Term term = contentDict[match];
                            textTerms.Add(
                                new TextTerm
                                {
                                    TextId = processingText.Id,
                                    Content = current,
                                    IndexFrom = start,
                                    IndexTo = end,
                                    TermId = term.Id,
                                });
                            matches = dictionary;
                            finish = end;
                            start = end + 1;
                            end = end + 1;
                            match = null;
                            continue;
                        }
                    }
                }

                if (count == 0)
                {
                    if (match != null)
                    {
                        end = start + match.Length - 1;
                        current = content[start.. (end + 1)];
                        var term = contentDict[match];
                        textTerms.Add(
                            new TextTerm()
                            {
                                TextId = processingText.Id, Content = current, IndexFrom = start, IndexTo = end, TermId = term.Id,
                            });
                        finish = end;
                        start = end + 1;
                        end = end + 1;
                    }
                    else
                    {
                        textTerms.Add(
                            new TextTerm()
                            {
                                TextId = processingText.Id,
                                Content = content[start]
                                    .ToString(),
                                IndexFrom = start,
                                IndexTo = start,
                            });
                        finish = start;
                        start = start + 1;
                        end = start;
                    }

                    matches = dictionary;
                    match = null;
                    continue;
                }

                if (count > 1)
                {
                    if (matches.AsParallel().Contains(current.ToUpperInvariant()))
                    {
                        match = current.ToUpperInvariant();
                    }

                    end += 1;
                }
            }

            return finish;
        }

        private bool RemoveExistingTextTerms(int textId)
        {
            this.logger.LogInformation($"New or updated text, set term count");
            this.logger.LogInformation($"Processing new or reset, removing old text term");

            if (this.textTermRepository.Queryable()
                .Where(t => t.TextId == textId)
                .Take(10000)
                .DeleteFromQuery() != 0)
            {
                return true;
            }

            return false;
        }
    }
}