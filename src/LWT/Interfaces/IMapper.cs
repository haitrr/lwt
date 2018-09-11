using System.Collections.Generic;

namespace Lwt.Interfaces
{
    public interface IMapper<TFrom, TTo> where TTo : class
    {
        TTo Map(TFrom from);

        TTo Map(TFrom from, TTo editedText);

        ICollection<TTo> Map(IEnumerable<TFrom> froms);
    }

    public interface IMapper<TFrom1, TFrom2, TTo>
    {
        TTo Map(TFrom1 from1, TFrom2 from2);

        TTo Map(TFrom1 from, TFrom2 from2, TTo to);

        ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s);
    }

    public interface IMapper<TFrom1, TFrom2, TFrom3, TTo>
    {
        TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3);

        TTo Map(TFrom1 from, TFrom2 from2, TFrom3 from3, TTo to);

        ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s, IEnumerable<TFrom3> from3s);
    }
}