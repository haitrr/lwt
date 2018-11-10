namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// language to language view model mapper.
    /// </summary>
    public class LanguageViewMapper : BaseMapper<Language, LanguageViewModel>
    {
        /// <inheritdoc/>
        public override LanguageViewModel Map(Language language, LanguageViewModel viewModel)
        {
            viewModel.Id = language.Id;
            viewModel.Name = language.Name;

            return viewModel;
        }
    }
}