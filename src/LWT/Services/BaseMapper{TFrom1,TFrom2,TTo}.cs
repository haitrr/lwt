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
    /// <typeparam name="TTo">TTo.</typeparam>
    public abstract class BaseMapper<TFrom1, TFrom2, TTo> : IMapper<TFrom1, TFrom2, TTo>
        where TTo : class, new()
    {
        /// <inheritdoc/>
        public TTo Map(TFrom1 from1, TFrom2 from2)
        {
            var to = new TTo();
            return this.Map(from1, from2, to);
        }

        /// <inheritdoc/>
        public ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s)
        {
            var tos = new List<TTo>();

            if (from1s != null && from2s != null && from1s.Count() == from2s.Count())
            {
                IEnumerable<(TFrom1 From1, TFrom2 From2)> froms = from1s.Zip(
                    from2s,
                    (from1, from2) => (From1: from1, From2: from2));

                foreach ((TFrom1 from11, TFrom2 from21) in froms)
                {
                    TTo to = this.Map(from11, from21);

                    if (to != null)
                    {
                        tos.Add(to);
                    }
                }
            }

            return tos;
        }

        /// <inheritdoc/>
        public abstract TTo Map(TFrom1 from1, TFrom2 from2, TTo t);
    }
}