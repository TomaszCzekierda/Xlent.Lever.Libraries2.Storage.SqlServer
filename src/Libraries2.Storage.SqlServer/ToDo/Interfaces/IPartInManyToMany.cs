using System;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.ToDo.Interfaces
{
    /// <summary>
    /// The database columns that are expected in every facade database table
    /// </summary>
    public interface IPartInManyToMany<T>
    {
        T UpdatePrimaryId(T item, Guid newPrimaryId, Guid typeId = default(Guid));
    }
}
