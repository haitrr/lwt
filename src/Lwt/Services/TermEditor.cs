using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;

namespace Lwt.Services;

public class TermEditor : ITermEditor
{
    private readonly ISqlTermRepository termRepository;
    private readonly IMapper<TermEditModel, Term> termEditMapper;
    private readonly IDbTransaction dbTransaction;
    private readonly ILogRepository logRepository;
    private readonly IAuthenticationHelper authenticationHelper;

    public TermEditor(ISqlTermRepository termRepository, IMapper<TermEditModel, Term> termEditMapper, IDbTransaction dbTransaction, ILogRepository logRepository, IAuthenticationHelper authenticationHelper)
    {
        this.termRepository = termRepository;
        this.termEditMapper = termEditMapper;
        this.dbTransaction = dbTransaction;
        this.logRepository = logRepository;
        this.authenticationHelper = authenticationHelper;
    }

    public async Task EditAsync(TermEditModel termEditModel, int termId, int userId)
    {
        Term current = await this.termRepository.GetUserTermAsync(termId, userId);
        var before = new TermSnapShot(current.Content, current.LanguageCode.ToString(),
            current.LearningLevel.ToString());

        Term edited = this.termEditMapper.Map(termEditModel, current);
        this.termRepository.Update(edited);
        var after = new TermSnapShot(current.Content, current.LanguageCode.ToString(),
            current.LearningLevel.ToString());
        this.logRepository.Add(
            new Log(
                Constants.TermEditedEvent,
                new TermEditedEventData(before, after),
                authenticationHelper.GetLoggedInUserName()
            )
        );
        await this.dbTransaction.CommitAsync();
    }
}