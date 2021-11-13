namespace Lwt.Utilities;

using System.Linq;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Models;
using Lwt.Repositories;
using Microsoft.EntityFrameworkCore;

/// <inheritdoc />
public class UserTextGetter : IUserTextGetter
{
    private readonly ISqlTextRepository textRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTextGetter"/> class.
    /// </summary>
    /// <param name="textRepository"> text repo.</param>
    public UserTextGetter(ISqlTextRepository textRepository)
    {
        this.textRepository = textRepository;
    }

    /// <inheritdoc/>
    public async Task<Text> GetUserTextAsync(int textId, int userId)
    {
        Text? text = await this.textRepository.Queryable()
            .Where(t => t.Id == textId && t.UserId == userId)
            .Select(t => new Text() { Bookmark = t.Bookmark, Id = t.Id })
            .FirstOrDefaultAsync();

        if (text == null)
        {
            throw new ForbiddenException("You don't have permission to access this text.");
        }

        return text;
    }
}