namespace Lwt.Creators
{
    using System.Threading.Tasks;
    using Lwt.Models;

    public interface ITextTermProcessor
    {
        Task ProcessTextTermAsync(Text text);
    }
}