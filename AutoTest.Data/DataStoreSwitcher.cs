using System;
using System.Configuration;

namespace AutoTest.Data
{
    public enum DataStoreType
    {
        BIGENTITY,
        SQLITE
    }

    public static class DataStoreSwitcher
    {
        public static IDataStore Current { get; private set; }

        public static void Init()
        {
            var store = ConfigurationManager.AppSettings["DataStore"] ?? "BIGENTITY";
            if (!Enum.TryParse<DataStoreType>(store, true, out var type))
            {
                type = DataStoreType.BIGENTITY;
            }

            switch (type)
            {
                case DataStoreType.SQLITE:
                    var sqliteFile = ConfigurationManager.AppSettings["SQLiteFile"] ?? "AutoTestData.sqlite";
                    // Prefer Biz-level SqliteStore implementation if present (it contains the full implementation).
                    try
                    {
                        var bizType = AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType("AutoTest.Biz.Data.SqliteStore"))
                            .FirstOrDefault(t => t != null);
                        if (bizType != null)
                        {
                            Current = (IDataStore)Activator.CreateInstance(bizType, sqliteFile);
                        }
                        else
                        {
                            Current = new SqliteStore(sqliteFile);
                        }
                    }
                    catch
                    {
                        Current = new SqliteStore(sqliteFile);
                    }
                    break;
                case DataStoreType.BIGENTITY:
                default:
                    Current = new BigEntityStore();
                    break;
            }
        }
    }
}
