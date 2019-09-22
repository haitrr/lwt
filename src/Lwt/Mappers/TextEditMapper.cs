namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// d.
    /// </summary>
    public class TextEditMapper : BaseMapper<TextEditModel, Text>
    {
        /// <inheritdoc/>
        public override Text Map(TextEditModel from, Text result)
        {
            result.Language = from.Language;
            result.Title = from.Title;
            result.Content = from.Content;

            return result;
        }
    }
}