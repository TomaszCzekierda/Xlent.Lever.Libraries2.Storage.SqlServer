using System.Collections.Generic;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPageEnvelope<TData, TId>
        where TData : IStorable<TId>
    {
        /// <summary>
        /// The data in this segment, this "page"
        /// </summary>
        IEnumerable<TData> Data { get; set; }

        /// <summary>
        /// Information about this segment of the data
        /// </summary>
        PageInfo PageInfo { get; set; }
    }
}
