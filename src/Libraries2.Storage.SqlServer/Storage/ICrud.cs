using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Standard.Error.Logic;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Storage
{
    /// <summary>
    /// Interface for CRUD operation on any class that implements <see cref="IStorable{TId}"/>.
    /// </summary>
    /// <typeparam name="TStorable">The typo of objects that should have CRUD operations.</typeparam>
    /// <typeparam name="TId">The type for the <see cref="IStorable{TId}.Id"/> property.</typeparam>
    public interface ICrud<TStorable, TId>
        where TStorable : IStorable<TId>
    {
        /// <summary>
        /// Create a new "row" in storage for another item and returns the final result (by making a final Get).
        /// </summary>
        /// <param name="item">The item to store.</param>
        /// <returns>The new item as it was saved, including an updated <see cref="IStorable{TId}.ETag"/></returns>
        /// <remarks>The Id in <paramref name="item"/> will be ignored.</remarks>
        Task<TStorable> CreateAsync(TStorable item);

        /// <summary>
        /// Read a the "row" from storage that has <see cref="IStorable{TId}.Id"/> set to <paramref name="id"/>.
        /// </summary>
        /// <returns>The found object.</returns>
        /// <exception cref="FulcrumNotFoundException">Thrown if the <paramref name="id"/> could not be found.</exception>
        Task<TStorable> ReadAsync(TId id);

        /// <summary>
        /// Update a "row" in storage for another item and returns the final result (by making a final Get).
        /// </summary>
        /// <param name="item">The updated item.</param>
        /// <returns>The updated item as it was saved, including an updated <see cref="IStorable{TId}.ETag"/></returns>
        /// <exception cref="FulcrumNotFoundException">Thrown if the <see cref="IStorable{TId}.Id"/> for <paramref name="item"/> could not be found.</exception>
        /// <exception cref="FulcrumConflictException">Thrown if the <see cref="IStorable{TId}.ETag"/> for <paramref name="item"/> was outdated.</exception>
        Task<TStorable> UpdateAsync(TStorable item);

        /// <summary>
        /// Delete the "row" from storage that has Id set to <paramref name="id"/>.
        /// </summary>
        /// <remarks>The method is idempotent, i.e. if the row as not found, it will not throw any exception. </remarks>
        Task DeleteAsync(TId id);
    }
}
