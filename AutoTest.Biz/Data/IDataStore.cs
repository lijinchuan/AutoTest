using System;
using System.Collections.Generic;

namespace AutoTest.Data
{
    public interface IDataStore
    {
        T FindById<T>(string tableName, int id) where T : class, new();
        T Find<T>(string tableName, int id) where T : class, new();
        IEnumerable<T> Find<T>(string tableName, string indexName, object[] keys) where T : class, new();
        IEnumerable<T> Find<T>(string tableName, Func<T, bool> predicate) where T : class, new();
        IEnumerable<T> FindBatch<T>(string tableName, IEnumerable<object> keys) where T : class, new();
        long Count(string tableName, string indexName, object[] keys);
        IEnumerable<T> Scan<T>(string tableName, string indexName, object[] endKeys, object[] startKeys, int pageIndex, int pageSize, ref long total) where T : class, new();
        void Delete<T>(string tableName, int id) where T : class, new();
        IEnumerable<T> List<T>(string tableName, int pageIndex, int pageSize) where T : class, new();
        void Insert<T>(string tableName, T entity) where T : class, new();
        void InsertBatch<T>(string tableName, IEnumerable<T> entities) where T : class, new();
        void Update<T>(string tableName, T entity) where T : class, new();
        void Upsert<T>(string tableName, T entity) where T : class, new();

        void Init();
    }
}
