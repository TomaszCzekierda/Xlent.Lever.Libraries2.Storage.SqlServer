﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Standard.Assert;
using Xlent.Lever.Libraries2.Standard.Error.Logic;
using Xlent.Lever.Libraries2.Storage.SqlServer.Model;
using Xlent.Lever.Libraries2.Storage.SqlServer.Storage;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Logic
{
    /// <summary>
    /// Helper class for advanced SELECT statmements
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public class SingleTableHandler<TDatabaseItem> : Database, ICrudAll<TDatabaseItem, Guid>, ISearch<TDatabaseItem>
        where TDatabaseItem : IDatabaseItem, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public SingleTableHandler(string connectionString)
            : base(connectionString)
        {
        }

        #region ICrud

        /// <inheritdoc />
        public async Task<TDatabaseItem> CreateAsync(TDatabaseItem item)
        {
            var id = await InternalCreateAsync(item);
            return await ReadAsync(id);
        }

        /// <inheritdoc />
        private async Task<Guid> InternalCreateAsync(TDatabaseItem item)
        {
            if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
            item.ETag = Guid.NewGuid().ToString();
            InternalContract.RequireValidatedAndNotNull(item, nameof(item));
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(Helper.Create(item), item);
            }
            return item.Id;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = new TDatabaseItem
            {
                Id = id
            };
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(Helper.Delete(item), new { Id = id });
            }
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> ReadAsync(Guid id)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = new TDatabaseItem
            {
                Id = id
            };
            return await SearchWhereSingle(item, "Id = @Id");
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> UpdateAsync(TDatabaseItem item)
        {
            InternalContract.RequireValidatedAndNotNull(item, nameof(item));
            await InternalUpdateAsync(item);
            return await ReadAsync(item.Id);
        }

        /// <inheritdoc />
        private async Task InternalUpdateAsync(TDatabaseItem item)
        {
            InternalContract.RequireValidatedAndNotNull(item, nameof(item));
            var oldItem = await ReadAsync(item.Id);
            if (oldItem == null) throw new FulcrumNotFoundException($"Table {item.TableName} did not contain an item with id {item.Id}");
            if (!string.Equals(oldItem.ETag, item.ETag)) throw new FulcrumConflictException("Could not update. Your data was stale. Please reload and try again.");
            item.ETag = Guid.NewGuid().ToString();
            using (var db = NewSqlConnection())
            {
                var count = await db.ExecuteAsync(Helper.Update(item, oldItem.ETag), item);
                if (count == 0) throw new FulcrumConflictException("Could not update. Your data was stale. Please reload and try again.");
            }
        }
        #endregion

        #region ICrudAll
        /// <inheritdoc />
        public Task<IPageEnvelope<TDatabaseItem, Guid>> ReadAllAsync(Guid id, int offset = 0, int limit = PageInfo.DefaultLimit)
        {

        }

        /// <inheritdoc />
        public Task DeleteAllAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ISearch

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAllAsync(string orderBy, int offset = 0,
            int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            return await SearchWhereAsync(null, orderBy, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAdvancedAsync(object param, string countFirst, string selectFirst,
            string selectRest, string orderBy = null, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            var total = CountItemsAdvanced(param, countFirst, selectRest);
            var selectStatement = selectRest == null ? null : $"{selectFirst} {selectRest}";
            var data = await SearchInternalAsync(param, selectStatement, orderBy, offset, limit);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(string where, string orderBy = null,
            int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            return await SearchWhereAsync(null, where, orderBy, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(object param, string where = null,
            string orderBy = null, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            var total = CountItemsWhere(param, where);
            var data = await SearchInternalWhereAsync(param, where, orderBy, offset, limit);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchWhereSingle(object param, string where)
        {
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return await SearchAdvancedSingle(param, $"SELECT * FROM [{item.TableName}] WHERE ({where})");
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchAdvancedSingle(object param, string selectStatement)
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            return await SearchFirstAdvancedAsync(param, selectStatement);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstWhereAsync(object param, string where = null, string orderBy = null)
        {
            var result = await SearchInternalWhereAsync(param, where, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstAdvancedAsync(object param, string selectStatement, string orderBy = null)
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            var result = await SearchInternalAsync(param, selectStatement, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public int CountItemsWhere(object param, string where = null)
        {
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return CountItemsAdvanced(param, "SELECT COUNT(*)", $"FROM [{item.TableName}] WHERE ({where})");
        }

        /// <inheritdoc />
        public int CountItemsAdvanced(object param, string selectFirst, string selectRest)
        {
            InternalContract.RequireNotNullOrWhitespace(selectFirst, nameof(selectFirst));
            InternalContract.RequireNotNullOrWhitespace(selectRest, nameof(selectRest));
            if (selectRest == null) return 0;
            var selectStatement = $"{selectFirst} {selectRest}";
            using (IDbConnection db = NewSqlConnection())
            {
                return db.Query<int>(selectStatement, param)
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalWhereAsync(object param, string where = null, string orderBy = null,
            int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return await SearchInternalAsync(param, $"SELECT * FROM [{item.TableName}] WHERE ({where})", orderBy, offset, limit);
        }

        /// <summary>
        /// Find the items specified by the <paramref name="selectStatement"/>.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectStatement"/> condition.</param>
        /// <param name="selectStatement">The SELECT statement, including WHERE, but not ORDER BY.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        /// 
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalAsync(object param, string selectStatement, string orderBy = null,
            int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            if (orderBy == null) orderBy = "1";
            using (IDbConnection db = NewSqlConnection())
            {
                var sqlQuery = $"{selectStatement} " +
                               $" ORDER BY {orderBy}" +
                               $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";

               return await db.QueryAsync<TDatabaseItem>(sqlQuery, param);
            }
        }
        #endregion
    }
}