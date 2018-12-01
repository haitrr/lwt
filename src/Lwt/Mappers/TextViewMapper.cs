namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;

    /// <summary>
    /// text to view model mapper.
    /// </summary>
    public class TextViewMapper : BaseMapper<Text, TextViewModel>
    {
        /// <inheritdoc/>
        public override TextViewModel Map(Text text, TextViewModel viewModel)
        {
            viewModel.Title = text.Title;
            viewModel.Language = text.Language;
            viewModel.Id = text.Id;

            return viewModel;
        }
    }
}