namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// mapper from text to text edit detail.
    /// </summary>
    public class TextEditDetailMapper : BaseMapper<Text, TextEditDetailModel>
    {
        /// <inheritdoc />
        public override TextEditDetailModel Map(Text text, TextEditDetailModel editDetailModel)
        {
            editDetailModel.Language = text.Language;
            editDetailModel.Title = text.Title;
            editDetailModel.Content = text.Content;
            return editDetailModel;
        }
    }
}