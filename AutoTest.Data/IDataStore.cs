using System.Collections.Generic;

namespace AutoTest.Data
{
    public interface IDataStore
    {
        T FindById<T>(string tableName, int id) where T : class;
        IEnumerable<T> Find<T>(string tableName, string indexName, object[] keys) where T : class;
        IEnumerable<T> Find<T>(string tableName, Func<T, bool> predicate) where T : class;
        IEnumerable<T> FindBatch<T>(string tableName, IEnumerable<object> keys) where T : class;
        IEnumerable<T> Scan<T>(string tableName, string indexName, object[] endKeys, object[] startKeys, int pageIndex, int pageSize, ref long total) where T : class;
        IEnumerable<T> List<T>(string tableName, int pageIndex, int pageSize) where T : class;
        void Insert<T>(string tableName, T entity);
        void Update<T>(string tableName, T entity);
        void Upsert<T>(string tableName, T entity);
        void Delete<T>(string tableName, int id) where T : class;
    }
}
