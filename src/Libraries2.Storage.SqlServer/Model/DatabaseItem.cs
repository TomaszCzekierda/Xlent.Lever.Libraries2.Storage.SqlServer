using System;
using System.Collections.Generic;
using Xlent.Lever.Libraries2.Standard.Assert;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Model
{
    /// <summary>
    /// A base class that has the mandatory properties.
    /// </summary>
    /// <remarks>
    /// We recommend to inherit from <see cref="TimeStampedDatabaseItem"/>.
    /// </remarks>
    public abstract class DatabaseItem : IDatabaseItem
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public string ETag { get; set; }

        /// <inheritdoc />
        public abstract string TableName { get; }

        /// <inheritdoc />
        public abstract string OrderBy { get; }

        /// <inheritdoc />
        public abstract IEnumerable<string> CustomColumnNames { get; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocaction)
        {
            FulcrumValidate.IsNotDefaultValue(Id, nameof(Id), errorLocaction);
            FulcrumValidate.IsNotDefaultValue(ETag, nameof(ETag), errorLocaction);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as DatabaseItem;
            return o != null && Id.Equals(o.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }
    }
}
