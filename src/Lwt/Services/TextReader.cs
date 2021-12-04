using System;
using System.Linq;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lwt.Services;

public class TextReader : ITextReader
{
    private readonly ISqlTextRepository textRepository;
    private readonly IMapper<Text, TextReadModel> textReadMapper;
    private readonly IDbTransaction dbTransaction;
    private readonly ILogRepository logRepository;
    private readonly IAuthenticationHelper authenticationHelper;

    public TextReader(
        ISqlTextRepository textRepository,
        IMapper<Text, TextReadModel> textReadMapper,
        IDbTransaction dbTransaction,
        ILogRepository logRepository,
        IAuthenticationHelper authenticationHelper)
    {
        this.textRepository = textRepository;
        this.textReadMapper = textReadMapper;
        this.dbTransaction = dbTransaction;
        this.logRepository = logRepository;
        this.authenticationHelper = authenticationHelper;
    }

    /// <inheritdoc />
    public async Task<TextReadModel> ReadAsync(int id, int userId)
    {
        Text? text = await this.textRepository.Queryable()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(
                t => new Text()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Bookmark = t.Bookmark,
                    LanguageCode = t.LanguageCode,
                    TermCount = t.TermCount,
                })
            .SingleOrDefaultAsync();

        if (text == null)
        {
            throw new NotFoundException("Text not found.");
        }

        text.LastReadAt = DateTime.UtcNow;
        this.textRepository.UpdateTextLastReadAt(text);
        this.logRepository.Add(
            new Log(
                Constants.TextReadEvent,
                new TextReadLogData(text.Id, text.LanguageCode.ToString()),
            authenticationHelper.GetLoggedInUserName()
        ).WithEntity(nameof(Text),text.Id)
            );
        await this.dbTransaction.CommitAsync();
        return this.textReadMapper.Map(text);
    }
}