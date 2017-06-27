﻿using System;
using System.Collections.Generic;
using Xlent.Lever.Libraries2.Storage.SqlServer.Storage;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Model
{
    /// <summary>
    /// A paging envelope for returning segments of data.
    /// </summary>
    public class PageEnvelope<TData> : IPageEnvelope<TData, Guid>
        where TData : IStorable<Guid>
    {
        /// <inheritdoc />
        public IEnumerable<TData> Data { get; set; }

        /// <inheritdoc />
        public PageInfo PageInfo { get; set; }
    }
}