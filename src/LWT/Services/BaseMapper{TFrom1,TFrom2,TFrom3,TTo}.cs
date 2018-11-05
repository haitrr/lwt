namespace Lwt.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using Lwt.Interfaces;

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="TFrom1">TFrom1.</typeparam>
    /// <typeparam name="TFrom2">TFrom2.</typeparam>
    /// <typeparam name="TFrom3">TFrom3.</typeparam>
    /// <typeparam name="TTo">TTo.</typeparam>
    public abstract class BaseMapper<TFrom1, TFrom2, TFrom3, TTo> : IMapper<TFrom1, TFrom2, TFrom3, TTo>
        where TTo : class, new()
    {
        /// <inheritdoc/>
        public TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3)
        {
            var to = new TTo();

            return this.Map(from1, from2, from3, to);
        }

        /// <inheritdoc/>
        public ICollection<TTo> Map(IEnumerable<TFrom1> from1S, IEnumerable<TFrom2> from2S, IEnumerable<TFrom3> from3S)
        {
            var tos = new List<TTo>();

            TFrom3[] enumerable = from3S as TFrom3[] ?? from3S.ToArray();
            TFrom1[] from1Array = from1S as TFrom1[] ?? from1S.ToArray();
            TFrom2[] from2Array = from2S as TFrom2[] ?? from2S.ToArray();

            if (from1Array.Length == from2Array.Length && from2Array.Length == enumerable.Length)
            {
                IEnumerable<(TFrom1 From1, TFrom2 From2, TFrom3 TFrom3)> froms = from1Array.Zip(
                    from2Array.Zip(enumerable, (from2, from3) => (from2, from3)),
                    (from1, from2) => (From1 : from1, From2 : from2.from2, TFrom3 : from2.from3));

                foreach ((TFrom1 From1, TFrom2 From2, TFrom3 TFrom3) from in froms)
                {
                    TTo to = this.Map(from.From1, from.From2, from.TFrom3);

                    if (to != null)
                    {
                        tos.Add(to);
                    }
                }
            }

            return tos;
        }

        /// <inheritdoc/>
        public abstract TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3, TTo t);
    }
}