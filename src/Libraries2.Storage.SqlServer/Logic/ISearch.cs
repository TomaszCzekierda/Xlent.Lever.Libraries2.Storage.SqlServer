using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Storage.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Logic
{
    /// <summary>
    /// Methods for searching (SELECT ... FROM ... WHERE ... GROUP BY) in an SQL database
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public interface ISearch<TDatabaseItem> where TDatabaseItem : IDatabaseItem, new()
    {
        /// <summary>
        /// Find the number of rows that fulfill the <paramref nae="where"/> condition..
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> expression.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <returns>The number of rows that fulfill the where statement.</returns>
        int CountItemsWhere(object param, string where = null);

        /// <summary>
        /// Find the number of rows that fulfill the <paramref name="selectRest"/> condition..
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectRest"/> expression.</param>
        /// <param name="selectFirst">The first part of the SELECT statement, something like SELECT COUNT(*), but not WHERE.</param>
        /// <param name="selectRest">The last part of the SELECT statement, beginning with FROM</param>
        /// <returns>The number of rows that fulfill the where statement.</returns>
        int CountItemsAdvanced(object param, string selectFirst, string selectRest);

        /// <summary>
        /// Fetches all rows for the current table.
        /// </summary>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        Task<PageEnvelope<TDatabaseItem>> SearchAllAsync(string orderBy, int offset = 0, int limit = 100);

        /// <summary>
        /// Both selectes objects and counts them, returning a <see cref="PageEnvelope{TData}"/>
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectRest"/> .</param>
        /// <param name="countFirst">A SELECT COUNT(*) like statement for counting the obhjects in the query.</param>
        /// <param name="selectFirst">The first part of the SELECT statement, including only which columns to fetch, but not WHERE and ORDER BY.</param>
        /// <param name="selectRest">The last part of the SELECT statement, common to both <paramref name="countFirst"/> and <paramref name="selectFirst"/>, not including ORDER BY</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        Task<PageEnvelope<TDatabaseItem>> SearchAdvancedAsync(object param, string countFirst, string selectFirst, string selectRest, string orderBy = null, int offset = 0, int limit = 100);

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(string where, string orderBy = null, int offset = 0, int limit = 100);

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> expression.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(object param, string where = null, string orderBy = null, int offset = 0, int limit = 100);

        /// <summary>
        /// Find the item specified by the <paramref name="where"/> condition.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> search condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <returns>The found item or null.</returns>
        /// <remarks>If more than one item is found, an excepton is thrown.</remarks>
        /// <remarks>If you just want the first item of possibly many that matches the where condition, 
        /// please use <see cref="SearchFirstWhereAsync"/></remarks>
        Task<TDatabaseItem> SearchWhereSingle(object param, string where);

        /// <summary>
        /// Find the item specified by the <paramref name="selectStatement"/> condition.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectStatement"/> search condition.</param>
        /// <param name="selectStatement">The full SELECT statement, including WHERE, but not ORDER BY.</param>
        /// <returns>The found item or null.</returns>
        /// <remarks>If more than one item is found, an excepton is thrown.</remarks>
        /// <remarks>If you just want the first item of possibly many that matches the where condition, 
        /// please use <see cref="SearchFirstWhereAsync"/></remarks>
        Task<TDatabaseItem> SearchAdvancedSingle(object param, string selectStatement);

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause and using fields from the <paramref name="param"/>.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <returns>The found items.</returns>
        Task<TDatabaseItem> SearchFirstWhereAsync(object param, string where = null, string orderBy = null);

        /// <summary>
        /// Find the items specified by the <paramref name="selectStatement"/> clause and using fields from the <paramref name="param"/>.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectStatement"/> condition.</param>
        /// <param name="selectStatement">The SELECT statement, including WHERE, but not ORDER BY.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <returns>The found items.</returns>
        Task<TDatabaseItem> SearchFirstAdvancedAsync(object param, string selectStatement, string orderBy = null);
    }
}