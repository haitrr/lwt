namespace Lwt.Services
{
    using System.Collections.Generic;

    using Lwt.Interfaces;

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="TFrom">from type.</typeparam>
    /// <typeparam name="TTo">to type.</typeparam>
    public abstract class BaseMapper<TFrom, TTo> : IMapper<TFrom, TTo>
        where TTo : class, new()
    {
        /// <inheritdoc/>
        public TTo Map(TFrom from)
        {
            var to = new TTo();

            return this.Map(from, to);
        }

        /// <inheritdoc/>
        public ICollection<TTo> Map(IEnumerable<TFrom> froms)
        {
            var objectCollection = new List<TTo>();

            if (froms != null)
            {
                foreach (TFrom from in froms)
                {
                    TTo newObject = this.Map(from);

                    if (newObject != null)
                    {
                        objectCollection.Add(newObject);
                    }
                }
            }

            return objectCollection;
        }

        /// <inheritdoc/>
        public abstract TTo Map(TFrom from, TTo t);
    }
}