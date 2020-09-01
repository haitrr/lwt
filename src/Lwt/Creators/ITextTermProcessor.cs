namespace Lwt.Creators
{
    using System.Threading.Tasks;

    public interface ITextTermProcessor
    {
        Task ProcessTextTermAsync();
    }
}