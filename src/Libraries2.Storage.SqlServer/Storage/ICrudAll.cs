using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Standard.Error.Logic;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Storage
{
    /// <summary>
    /// Interface for extended CRUD operation on any class that implements <see cref="IStorable{TId}"/>.
    /// </summary>
    /// <typeparam name="TStorable">The typo of objects that should have CRUD operations.</typeparam>
    /// <typeparam name="TId">The type for the <see cref="IStorable{TId}.Id"/> property.</typeparam>
    public interface ICrudAll<TStorable, TId> : ICrud<TStorable, TId>
        where TStorable : IStorable<TId>
    {
        /// <summary>
        /// Read all the "rows" from storage.
        /// </summary>
        /// <returns>The found objects.</returns>
        Task<IPageEnvelope<TStorable, TId>> ReadAllAsync(TId id, int offset = 0, int limit = PageInfo.DefaultLimit);

        /// <summary>
        /// Delete all "rows" from storage that has Id set to <paramref name="id"/>.
        /// </summary>
        /// <remarks>The method is idempotent, i.e. if no rows wasfound, it will not throw any exception. </remarks>
        Task DeleteAllAsync();
    }
}
