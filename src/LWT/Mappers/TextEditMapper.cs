using Lwt.Models;
using Lwt.Services;
using LWT.Models;

namespace Lwt.Mappers
{
    public class TextEditMapper : BaseMapper<TextEditModel, Text>
    {
        public override Text Map(TextEditModel editModel, Text editedText)
        {
            editedText.Title = editModel.Title;
            editedText.Content = editModel.Content;
            return editedText;
        }
    }
}