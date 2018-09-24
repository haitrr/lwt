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
        public ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s, IEnumerable<TFrom3> from3s)
        {
            var tos = new List<TTo>();

            if (from1s != null && from2s != null && from3s != null && from1s.Count() == from2s.Count() &&
                from2s.Count() == from3s.Count())
            {
                IEnumerable<(TFrom1 From1, TFrom2 From2, TFrom3 TFrom3)> froms =
                    from1s.Zip(
                        from2s.Zip(from3s, (from2, from3) => (from2, from3)),
                        (from1, from2) => (From1: from1, From2: from2.from2, TFrom3: from2.from3));

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