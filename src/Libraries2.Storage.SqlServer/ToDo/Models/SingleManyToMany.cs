using System;
using System.Collections.Generic;
using System.Text;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.ToDo.Models
{
    /// <summary>
    /// One table to keep all ManyToMany relations
    /// </summary>
    public class SingleManyToMany : ManyToMany
    {
        /// <inheritdoc />
        public override string TableName => "SingleManyToMany";
    }
}
