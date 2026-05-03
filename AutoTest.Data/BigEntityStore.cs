using System;
using System.Collections.Generic;
using System.Linq;
using LJC.FrameWorkV3.Data.EntityDataBase;

namespace AutoTest.Data
{
    public class BigEntityStore : IDataStore
    {
        T IDataStore.FindById<T>(string tableName, int id)
        {
            return BigEntityTableRemotingEngine.Find<T>(tableName, id);
        }

        IEnumerable<T> IDataStore.Find<T>(string tableName, string indexName, object[] keys)
        {
            return BigEntityTableRemotingEngine.Find<T>(tableName, indexName, keys);
        }

        IEnumerable<T> IDataStore.Scan<T>(string tableName, string indexName, object[] endKeys, object[] startKeys, int pageIndex, int pageSize, ref long total)
        {
            return BigEntityTableRemotingEngine.Scan<T>(tableName, indexName, endKeys, startKeys, pageIndex, pageSize, ref total);
        }

        void IDataStore.Delete<T>(string tableName, int id)
        {
            BigEntityTableRemotingEngine.Delete<T>(tableName, id);
        }

        IEnumerable<T> IDataStore.List<T>(string tableName, int pageIndex, int pageSize)
        {
            return BigEntityTableRemotingEngine.List<T>(tableName, pageIndex, pageSize);
        }

        void IDataStore.Insert<T>(string tableName, T entity)
        {
            BigEntityTableRemotingEngine.Insert(tableName, entity);
        }

        void IDataStore.Update<T>(string tableName, T entity)
        {
            BigEntityTableRemotingEngine.Update(tableName, entity);
        }

        public void Upsert<T>(string tableName, T entity)
        {
            BigEntityTableRemotingEngine.Upsert(tableName, entity);
        }
    }
}
