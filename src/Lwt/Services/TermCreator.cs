using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lwt.Services;

public class TermCreator : ITermCreator
{
    private readonly ISqlTermRepository termRepository;
    private readonly IDbTransaction dbTransaction;
    private readonly IAuthenticationHelper authenticationHelper;
    private readonly ILogRepository logRepository;

    public TermCreator(
        ISqlTermRepository termRepository,
        IDbTransaction dbTransaction,
        IAuthenticationHelper authenticationHelper,
        ILogRepository logRepository)
    {
        this.termRepository = termRepository;
        this.dbTransaction = dbTransaction;
        this.authenticationHelper = authenticationHelper;
        this.logRepository = logRepository;
    }

    public async Task<int> CreateAsync(Term term)
    {
        Term? existingTerm = await this.termRepository.TryGetByUserAndLanguageAndContentAsync(
            term.UserId,
            term.LanguageCode,
            term.Content);

        if (existingTerm != null)
        {
            throw new BadRequestException("Term has already exist.");
        }

        await using (IDbContextTransaction transaction = this.dbTransaction.BeginTransaction())
        {
            this.termRepository.Add(term);
            this.dbTransaction.SaveChanges();
            this.logRepository.Add(
                new Log(
                    Constants.TermCreatedEvent,
                    new TermCreatedLogData(
                        term.LanguageCode.ToString(),
                        term.Content,
                        term.LearningLevel.ToString()
                    ),
                    authenticationHelper.GetLoggedInUserName()
                ).WithEntity(nameof(Term), term.Id)
            );
            this.dbTransaction.SaveChanges();
            await transaction.CommitAsync();
        }

        return term.Id;
    }
}