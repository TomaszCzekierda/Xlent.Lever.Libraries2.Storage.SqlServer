using System.Collections.Generic;
using System.Linq;
using Xlent.Lever.Libraries2.Standard.Storage.Model;
using Xlent.Lever.Libraries2.Storage.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Storage.SqlServer.Logic
{
    internal static class Helper
    {
        public static string Create(IDatabaseItem item) => $"INSERT INTO dbo.[{item.TableName}] ({ColumnList(item)}) values ({ArgumentList(item)})";

        public static string Read(IDatabaseItem item, string where) => $"SELECT {ColumnList(item)} FROM [{item.TableName}] WHERE {where}";

        public static string Read(IDatabaseItem item, string where, string orderBy) => $"SELECT {ColumnList(item)} FROM [{item.TableName}] WHERE {where} ORDER BY {orderBy}";
        

        public static string Update(IDatabaseItem item, string oldEtag) => $"UPDATE [{item.TableName}] SET {UpdateList(item)} WHERE Id = @Id AND ETag == '{oldEtag}'";

        public static string Delete(IDatabaseItem item) => $"DELETE FROM [{item.TableName}] WHERE Id = @Id";

        public static string ColumnList(IDatabaseItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]"));

        public static string ArgumentList(IDatabaseItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"@{name}"));

        public static string UpdateList(IDatabaseItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]=@{name}"));

        public static IEnumerable<string> NonCustomColumnNames(IDatabaseItem item)
        {
            var list = new List<string> {"Id", "ETag"};
            var timeStamped = item as ITimeStamped;
            if (timeStamped == null) return list;
            list.AddRange(new [] {"CreatedAt", "UpdatedAt"});
            return list;
        }

        public static IEnumerable<string> AllColumnNames(IDatabaseItem item)
        {
            var list = NonCustomColumnNames(item).ToList();
            list.AddRange(item.CustomColumnNames);
            return list;
        }
    }
}
