using System.Collections.Generic;
using System.Linq;
using Lwt.Interfaces;

namespace Lwt.Services
{
    public abstract class BaseMapper<TFrom, TTo> : IMapper<TFrom, TTo> where TTo : class, new()
    {
        public TTo Map(TFrom from)
        {
            var to = new TTo();
            return Map(from, to);
        }

        public ICollection<TTo> Map(IEnumerable<TFrom> froms)
        {
            var objectCollection = new List<TTo>();

            if (froms != null)
            {
                foreach (TFrom from in froms)
                {
                    TTo newObject = Map(from);

                    if (newObject != null)
                    {
                        objectCollection.Add(newObject);
                    }
                }
            }

            return objectCollection;
        }

        public abstract TTo Map(TFrom from, TTo editedText);
    }

    public abstract class BaseMapper<TFrom1, TFrom2, TTo> : IMapper<TFrom1, TFrom2, TTo> where TTo : class, new()
    {
        public TTo Map(TFrom1 from1, TFrom2 from2)
        {
            var to = new TTo();
            return Map(from1, from2, to);
        }

        public ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s)
        {
            var tos = new List<TTo>();

            if (from1s != null && from2s != null && from1s.Count() == from2s.Count())
            {
                IEnumerable<(TFrom1 From1, TFrom2 From2)> froms = from1s.Zip(from2s,
                    (from1, from2) => (From1: from1, From2: from2));

                foreach ((TFrom1 From1, TFrom2 From2) from in froms)
                {
                    TTo to = Map(from.From1, from.From2);

                    if (to != null)
                    {
                        tos.Add(to);
                    }
                }
            }

            return tos;
        }

        public abstract TTo Map(TFrom1 from1, TFrom2 from2, TTo to);
    }

    public abstract class BaseMapper<TFrom1, TFrom2, TFrom3, TTo> : IMapper<TFrom1, TFrom2, TFrom3, TTo>
        where TTo : class, new()
    {
        public TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3)
        {
            var to = new TTo();
            return Map(from1, from2, from3, to);
        }

        public ICollection<TTo> Map(IEnumerable<TFrom1> from1s, IEnumerable<TFrom2> from2s, IEnumerable<TFrom3> from3s)
        {
            var tos = new List<TTo>();

            if (from1s != null && from2s != null && from3s != null && from1s.Count() == from2s.Count() &&
                from2s.Count() == from3s.Count())
            {
                IEnumerable<(TFrom1 From1, TFrom2 From2, TFrom3 TFrom3)> froms =
                    from1s.Zip(from2s.Zip(from3s, (from2, from3) => (from2, from3)),
                        (from1, from2) => (From1: from1, From2: from2.Item1, TFrom3: from2.Item2));

                foreach ((TFrom1 From1, TFrom2 From2, TFrom3 TFrom3) from in froms)
                {
                    TTo to = Map(from.From1, from.From2, from.TFrom3);

                    if (to != null)
                    {
                        tos.Add(to);
                    }
                }
            }

            return tos;
        }

        public abstract TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3, TTo to);
    }
}