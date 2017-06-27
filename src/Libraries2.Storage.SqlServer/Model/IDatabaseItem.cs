using System;
using Xlent.Lever.Libraries2.Standard.Storage.Model;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statmements
    /// </summary>
    public interface IDatabaseItem : IStorable<Guid>, IETag, ISqlMetadata
    {
    }
}