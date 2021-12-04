namespace Lwt.Creators;

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Repositories;

/// <inheritdoc />
public class TextCreator : ITextCreator
{
    private readonly IValidator<Text> textValidator;
    private readonly ISqlTextRepository textRepository;
    private readonly IDbTransaction dbTransaction;
    private readonly ILogRepository logRepository;
    private readonly IAuthenticationHelper authenticationHelper;

    public TextCreator(
        IValidator<Text> textValidator,
        ISqlTextRepository textRepository,
        IDbTransaction dbTransaction, ILogRepository logRepository, IAuthenticationHelper authenticationHelper)
    {
        this.textValidator = textValidator;
        this.textRepository = textRepository;
        this.dbTransaction = dbTransaction;
        this.logRepository = logRepository;
        this.authenticationHelper = authenticationHelper;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Text text)
    {
        ValidationResult validationResult = this.textValidator.Validate(text);

        if (!validationResult.IsValid)
        {
            throw new BadRequestException(
                validationResult.Errors.First()
                    .ErrorMessage);
        }

        text.LastReadAt = DateTime.UtcNow;
        this.textRepository.Add(text);
        this.logRepository.Add(
            new Log(Constants.TextCreatedEvent,
                new TextCreateLogData(text.Title, text.Content),
                authenticationHelper.GetLoggedInUserName()
            ));
        await this.dbTransaction.CommitAsync();

        return text.Id;
    }
}