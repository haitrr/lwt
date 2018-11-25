namespace Lwt.Mappers
{
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// language view mapper.
    /// </summary>
    public class LanguageViewMapper : BaseMapper<ILanguage,LanguageViewModel>
    {
        /// <inheritdoc/>
        public override LanguageViewModel Map(ILanguage language, LanguageViewModel viewModel)
        {
            viewModel.Name = language.Name;
            viewModel.Id = language.Id;
            return viewModel;
        }
    }
}