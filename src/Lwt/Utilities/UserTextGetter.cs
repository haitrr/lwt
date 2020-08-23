namespace Lwt.Utilities
{
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class UserTextGetter : IUserTextGetter
    {
        private readonly ITextRepository textRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTextGetter"/> class.
        /// </summary>
        /// <param name="textRepository"> text repo.</param>
        public UserTextGetter(ITextRepository textRepository)
        {
            this.textRepository = textRepository;
        }

        /// <inheritdoc/>
        public async Task<Text> GetUserTextAsync(int textId, int userId)
        {
            Text text = await this.textRepository.GetByIdAsync(textId);

            if (text.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            return text;
        }
    }
}