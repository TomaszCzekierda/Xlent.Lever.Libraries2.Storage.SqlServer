using System;
using Xlent.Lever.Libraries2.Storage.SqlServer.Logic;
using Xlent.Lever.Libraries2.Storage.SqlServer.Model;
using Xlent.Lever.Libraries2.Storage.SqlServer.ToDo.Interfaces;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.ToDo.Logic
{
    /// <summary>
    /// Many to many table
    /// </summary>
    /// <typeparam name="TFirstModel"></typeparam>
    /// <typeparam name="TFirstTable"></typeparam>
    /// <typeparam name="TManyToManyModel"></typeparam>
    /// <typeparam name="TSecondModel"></typeparam>
    /// <typeparam name="TSecondTable"></typeparam>
    public abstract class ManyToManyTableHandler<TFirstModel, TFirstTable, TManyToManyModel, TSecondModel, TSecondTable> : SingleTableHandler<TManyToManyModel>
        where TFirstModel : IDatabaseItem, new()
        where TFirstTable : SingleTableHandler<TFirstModel>, IPartInManyToMany<TFirstModel>
        where TManyToManyModel : IManyToMany, new() where TSecondModel : IDatabaseItem, new()
        where TSecondTable : SingleTableHandler<TSecondModel>, IPartInManyToMany<TSecondModel>
    {
        private readonly TFirstTable _firstTableLogic;
        private readonly TSecondTable _secondTableLogic;
        private readonly Guid _typeId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="firstTableLogic"></param>
        /// <param name="secondTableLogic"></param>
        /// <param name="typeId"></param>
        protected ManyToManyTableHandler(string connectionString, TFirstTable firstTableLogic, TSecondTable secondTableLogic, Guid typeId = default(Guid))
            :base(connectionString)
        {
            _firstTableLogic = firstTableLogic;
            _secondTableLogic = secondTableLogic;
            _typeId = typeId;
        }

        private object FirstOrSecond(bool first)
        {
            return first ? "First" : "Second";
        }

        // TODO: Implement many to many table.
        /*

        #region READ

        public virtual int ReadMaxSortOrder(bool first, Guid id)
        {
            var firstOrSecond = FirstOrSecond(first);
            var ignored = 0;
            var param = new DynamicParameters();
            param.Add("FirstOrSecondId", id);
            param.Add("TypeId", _typeId);
            param.Add("MaxValue", ignored, null, ParameterDirection.Output);
            ReadInto(param, $"SELECT @MaxValue=MAX({firstOrSecond}SortOrder) FROM [{TableName}] WHERE [{firstOrSecond}Id] = @FirstOrSecondId AND TypeId = @TypeId");
            return param.Get<int?>("MaxValue") ?? 0;
        }

        public TManyToManyModel ReadByFirstIdAndSecondId(Guid firstId, Guid secondId)
        {
            return SearchWhereTheOnlyOne(new { FirstId = firstId, SecondId = secondId, TypeId = _typeId },
                "FirstId = @FirstId AND SecondId = @SecondId AND TypeId = @TypeId");
        }

        #endregion

        #region CREATE
        /// <summary>
        /// Create a relationship a new relationship.
        /// </summary>
        /// <remarks>For the sort orders null means last, any other number is absolute 
        /// unless the number is larger than the current number of items, then that means last.</remarks>
        public override TManyToManyModel Create(TManyToManyModel item)
        {
            item.FirstSortOrder = MakeRoomForSortOrder(true, item.FirstId, item.FirstSortOrder);
            item.SecondSortOrder = MakeRoomForSortOrder(true, item.SecondId, item.SecondSortOrder);
            item = base.Create(item);
            MaybeUpdatePrimaryId(item);
            return item;
        }

        private void MaybeUpdatePrimaryId(TManyToManyModel item)
        {
            // Maybe update PrimaryId
            if (item.FirstSortOrder == 1)
            {
                var existing = _secondTableLogic.Read(item.SecondId);
                if (existing == null)
                {
                    throw new AssertionFailedException(
                        $"Expected item {item.SecondId} to exist in table {_secondTableLogic.TableName}.");
                }
                if (_secondTableLogic.NameOfPrimaryIdColumn(_typeId) != null)
                {
                    _secondTableLogic.UpdatePrimaryId(existing, item.FirstId, _typeId);
                }
            }
            if (item.SecondSortOrder == 1)
            {
                var existing = _firstTableLogic.Read(item.FirstId);
                if (existing == null)
                {
                    throw new AssertionFailedException(
                        $"Expected item {item.FirstId} to exist in table {_firstTableLogic.TableName}.");
                }
                if (_firstTableLogic.NameOfPrimaryIdColumn(_typeId) != null)
                {
                    _firstTableLogic.UpdatePrimaryId(existing, item.SecondId, _typeId);
                }
            }
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// Update the data for a item
        /// </summary>
        /// <param name="first">Is it the first sort order that we should update? False means update second sort order</param>
        /// <param name="firstId">The id for the first item.</param>
        /// <param name="secondId">The id for the second item.</param>
        /// <param name="newSortOrder">Where should the item be put in the sort order. null means last.</param>
        /// <returns>The updated item.</returns>
        public TManyToManyModel UpdateSortOrder(bool first, Guid firstId, Guid secondId, int? newSortOrder = null)
        {
            var doUpdate = false;
            var doCreate = false;
            var item = ReadByFirstIdAndSecondId(firstId, secondId);
            if (item == null)
            {
                doCreate = true;
                item = new TManyToManyModel
                {
                    TypeId = _typeId,
                    FirstId = firstId,
                    SecondId = secondId,
                    FirstSortOrder =
                        MakeRoomForSortOrder(true, firstId,
                            first ? newSortOrder : null),
                    SecondSortOrder =
                        MakeRoomForSortOrder(false, secondId,
                            first ? null : newSortOrder)
                };
            }
            else //if (newSortOrder != null)
            {
                if (first)
                {
                    if (item.FirstSortOrder != newSortOrder)
                    {
                        Debug.Assert(item.FirstSortOrder != null, "item.FirstSortOrder != null");
                        var currentValue = item.FirstSortOrder.Value;
                        item.FirstSortOrder = MakeRoomForSortOrder(true, item.FirstId, newSortOrder, currentValue);
                        doUpdate = true;
                    }
                }
                else
                {
                    if (item.SecondSortOrder != newSortOrder)
                    {
                        Debug.Assert(item.SecondSortOrder != null, "item.SecondSortOrder != null");
                        var currentValue = item.SecondSortOrder.Value;
                        item.SecondSortOrder = newSortOrder;
                        item.SecondSortOrder = MakeRoomForSortOrder(false, item.SecondId, newSortOrder, currentValue);
                        doUpdate = true;
                    }
                }
            }
            if (doUpdate) item = Update(item);
            else if (doCreate) item = base.Create(item);
            MaybeUpdatePrimaryId(item);
            return item;
        }

        private int MakeRoomForSortOrder(bool first, Guid oneId, int? newValue, int? currentValue = null)
        {
            var nextFirstSortOrder = 1 + ReadMaxSortOrder(first, oneId);
            var newValueNotNull = newValue ?? nextFirstSortOrder;
            if (currentValue != null)
            {
                // Move rather than insert
                if (!newValue.HasValue) newValueNotNull--;
                if (newValueNotNull == currentValue) return newValueNotNull;
                if (newValueNotNull > currentValue)
                {
                    ShiftSortOrder(first, true, oneId, (int)currentValue, newValueNotNull);
                }
                else
                {
                    ShiftSortOrder(first, false, oneId, newValueNotNull, (int)currentValue);
                }
                return newValueNotNull;
            }
            // Insert rather than move
            ShiftSortOrder(first, false, oneId, newValueNotNull);
            return newValueNotNull;
        }

        private void ShiftSortOrder(bool first, bool shiftLeft, Guid id, int firstValue, int? lastValue = null)
        {
            var firstOrSecond = FirstOrSecond(first);
            var addOrSubstract = shiftLeft ? "-" : "+";
            var smallerThanLastValue = lastValue == null ? "" : $"AND {firstOrSecond}SortOrder <= @LastValue";
            var sqlQuery = $"UPDATE [{TableName}] SET {firstOrSecond}SortOrder = {firstOrSecond}SortOrder{addOrSubstract}1, Etag=NEWID(), [RowUpdatedAt]=GETUTCDATE()" +
                $" WHERE [{firstOrSecond}Id] = @Id AND {firstOrSecond}SortOrder >= @FirstValue {smallerThanLastValue} AND TypeId = @TypeId";
            using (IDbConnection db = NewSqlConnection())
            {
                db.Execute(sqlQuery,
                    new { Id = id, FirstValue = firstValue, LastValue = lastValue, TypeId = _typeId });
            }
        }

        #endregion

        #region DELETE

        public void DeleteRelationshipsForDeletedId(Guid id)
        {
            using (IDbConnection db = NewSqlConnection())
            {
                var sqlQuery = $"DELETE FROM [{TableName}] WHERE TypeId = {_typeId} AND ([FirstId] = @Id OR [SecondId] = @Id)";
                db.Execute(sqlQuery, new { Id = id });
            }
        }
        #endregion


        #region SEARCH
        public TFirstModel ReadDefaultFirstBySecondId(Guid secondId)
        {
            return ReadDefaultById<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId);
        }

        public TSecondModel ReadDefaultSecondByFirstId(Guid firstId)
        {
            return ReadDefaultById<TSecondModel, TSecondTable>(false, _secondTableLogic, firstId);
        }

        private TModel ReadDefaultById<TModel, TLogic>(bool first, TLogic logic, Guid id)
            where TModel : IMandatoryDatabaseColumns
            where TLogic : Table<TModel>
        {
            var firstOrSecond = FirstOrSecond(first);
            var selectStatement = $"SELECT r.* FROM [{logic.TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.FirstId)" +
                   $" WHERE m2m.[{firstOrSecond}Id] = @Id AND m2m.[TypeId] = @TypeId AND m2m2.[{firstOrSecond}SortOrder] = 1";
            return logic.SearchTheOnlyOne(new { Id = id, TypeId = _typeId }, selectStatement);
        }

        public PageEnvelope<TFirstModel> SearchFirstBySecondId(Guid secondId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchByOtherId<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId, offset, limit);
        }

        public PageEnvelope<TSecondModel> SearchSecondByFirstId(Guid firstId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchByOtherId<TSecondModel, TSecondTable>(true, _secondTableLogic, firstId, offset, limit);
        }

        private PageEnvelope<TModel> SearchByOtherId<TModel, TLogic>(bool first, TLogic logic, Guid otherId, int offset = 0, int limit = Paging.DefaultLimit)
            where TModel : IMandatoryDatabaseColumns
            where TLogic : Table<TModel>
        {
            var firstOrSecond = FirstOrSecond(first);
            var other = FirstOrSecond(!first);
            var selectRest = $"FROM [{TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.{other}Id)" +
                   $" WHERE m2m.[{firstOrSecond}Id] = @OtherId AND m2m.[TypeId] = @TypeId";
            return logic.Search(new { OtherId = otherId, TypeId = _typeId }, "SELECT COUNT(r.[Id])", "SELECT r.*", selectRest, $"m2m.[{firstOrSecond}SortOrder]", offset, limit);
        }

        public PageEnvelope<TFirstModel> SearchFirstBySecondIdAndPrimaryId(Guid secondId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchByOtherIdAndPrimaryId<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId, offset, limit);
        }
        public PageEnvelope<TSecondModel> SearchSecondByFirstIdAndPrimaryId(Guid firstId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchByOtherId<TSecondModel, TSecondTable>(true, _secondTableLogic, firstId, offset, limit);
        }

        private PageEnvelope<TModel> SearchByOtherIdAndPrimaryId<TModel, TLogic>(bool first, TLogic logic, Guid otherId, int offset = 0, int limit = Paging.DefaultLimit)
            where TModel : IMandatoryDatabaseColumns
            where TLogic : Table<TModel>, IPartInManyToMany<TModel>
        {
            var nameOfPrimaryIdColumn = logic.NameOfPrimaryIdColumn(_typeId);
            if (nameOfPrimaryIdColumn == null) throw new NotImplementedException(nameof(nameOfPrimaryIdColumn));
            var firstOrSecond = FirstOrSecond(first);
            var other = FirstOrSecond(!first);
            var selectRest = $"FROM [{TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.{firstOrSecond}Id) AND {nameOfPrimaryIdColumn} = @OtherId" +
                   $" WHERE m2m.[{other}Id] = @OtherId AND m2m.[TypeId] = @TypeId";
            return logic.Search(new { OtherId = otherId, TypeId = _typeId }, "SELECT COUNT(r.[Id])", "SELECT r.*", selectRest, $"m2m.[{other}SortOrder]", offset, limit);
        }

        protected PageEnvelope<TManyToManyModel> SearchByFirstId(Guid firstId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchWhere(new { FirstId = firstId, TypeId = _typeId }, "[FirstId]=@FirstId AND [TypeId]=@TypeId", "m2m.[FirstSortOrder]", offset, limit);
        }

        protected PageEnvelope<TManyToManyModel> SearchBySecondId(Guid secondId, int offset = 0, int limit = Paging.DefaultLimit)
        {
            return SearchWhere(new { SecondId = secondId, TypeId = _typeId }, "[SecondId]=@SecondId AND [TypeId]=@TypeId", "m2m.[SecondSortOrder]", offset, limit);
        }
        #endregion
        */
    }
}

