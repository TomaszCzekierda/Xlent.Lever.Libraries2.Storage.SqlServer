using Xlent.Lever.Libraries2.Standard.Assert;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Storage
{
    /// <summary>
    /// Things required to be a storable class
    /// </summary>
    /// <typeparam name="TId">The type for the property <see cref="Id"/>.</typeparam>
    public interface IStorable<TId> : IValidatable
    {
        /// <summary>
        /// The id for the storable item.
        /// </summary>
        TId Id { get; set; }

        /// <summary>
        /// This is a pattern to achieve optimistic concurrency control, https://en.wikipedia.org/wiki/Optimistic_concurrency_control
        /// </summary>
        string ETag { get; set; }
    }
}
