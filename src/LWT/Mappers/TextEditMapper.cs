namespace Lwt.Mappers
{
    using Lwt.Models;

    using LWT.Models;

    using Lwt.Services;

    /// <summary>
    /// d.
    /// </summary>
    public class TextEditMapper : BaseMapper<TextEditModel, Text>
    {
        /// <inheritdoc/>
        public override Text Map(TextEditModel editModel, Text editedText)
        {
            editedText.Title = editModel.Title;
            editedText.Content = editModel.Content;

            return editedText;
        }
    }
}