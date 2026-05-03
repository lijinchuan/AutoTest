using LJC.FrameWorkV3.Comm;
using System;
// avoid referencing System.Configuration in this project; use environment variables as fallback

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
            //var store = Environment.GetEnvironmentVariable("DataStore") ?? "BIGENTITY";
            var store = ConfigHelper.AppConfig("DataStore");
            if (!Enum.TryParse<DataStoreType>(store, true, out var type))
            {
                type = DataStoreType.BIGENTITY;
            }

            switch (type)
            {
                case DataStoreType.SQLITE:
                    var sqliteFile = ConfigHelper.AppConfig("SQLiteFile") ?? "AutoTestData.sqlite";
                    Current = new SqliteStore(sqliteFile);
                    break;
                case DataStoreType.BIGENTITY:
                default:
                    Current = new BigEntityStore();
                    break;
            }

            Current.Init();
        }
    }
}
