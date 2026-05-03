using System;
using System.Linq;
using System.Reflection;
using AutoTest.Data;
using AutoTest.Domain.Entity;
using Newtonsoft.Json;

namespace AutoTest.Migrate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting migration from BigEntity to SQLite...");
            // initialize both stores
            AutoTest.Data.DataStoreSwitcher.Init();

            // force BigEntity store
            var beStore = new AutoTest.Data.BigEntityStore();
            beStore.Init();

            var sqliteFile = args.Length > 0 ? args[0] : "AutoTestData.sqlite";
            var sqliteStore = new AutoTest.Data.SqliteStore(sqliteFile);

            // 可选参数: [sqliteFile] [batchSize]
            int batchSize = 1000;
            if (args.Length > 1 && int.TryParse(args[1], out var bs) && bs > 0) batchSize = bs;
            _batchSize = batchSize;
            Console.WriteLine($"目标 SQLite 文件: {sqliteFile}，批次大小: {_batchSize}");

            // Migrate tables in order
            MigrateTable<TestSource>(beStore, sqliteStore, nameof(TestSource));
            MigrateTable<TestSite>(beStore, sqliteStore, nameof(TestSite));
            MigrateTable<TestLogin>(beStore, sqliteStore, nameof(TestLogin));
            MigrateTable<TestPage>(beStore, sqliteStore, nameof(TestPage));
            MigrateTable<TestCase>(beStore, sqliteStore, nameof(TestCase));
            MigrateTable<TestEnv>(beStore, sqliteStore, nameof(TestEnv));
            MigrateTable<TestEnvParam>(beStore, sqliteStore, nameof(TestEnvParam));
            MigrateTable<TestCaseData>(beStore, sqliteStore, nameof(TestCaseData));
            MigrateTable<TestCaseInvokeLog>(beStore, sqliteStore, nameof(TestCaseInvokeLog));
            MigrateTable<TestCaseSetting>(beStore, sqliteStore, nameof(TestCaseSetting));
            MigrateTable<TestCaseParam>(beStore, sqliteStore, nameof(TestCaseParam));
            MigrateTable<APIDoc>(beStore, sqliteStore, nameof(APIDoc));
            MigrateTable<TestCaseDocExample>(beStore, sqliteStore, nameof(TestCaseDocExample));
            MigrateTable<ProxyServer>(beStore, sqliteStore, nameof(ProxyServer));
            MigrateTable<TestScript>(beStore, sqliteStore, nameof(TestScript));
            //MigrateTable<TestResult>(beStore, sqliteStore, nameof(TestResult));
            MigrateTable<FileDB>(beStore, sqliteStore, nameof(FileDB));
            MigrateTable<TestCookieContainer>(beStore, sqliteStore, nameof(TestCookieContainer));
            MigrateTable<Counter>(beStore, sqliteStore, nameof(Counter));
            MigrateTable<TestTaskBag>(beStore, sqliteStore, nameof(TestTaskBag));
            MigrateTable<TaskBagLog>(beStore, sqliteStore, nameof(TaskBagLog));
            MigrateTable<TestCaseUrlConfig>(beStore, sqliteStore, nameof(TestCaseUrlConfig));
            MigrateTable<RequestInterceptConfig>(beStore, sqliteStore, nameof(RequestInterceptConfig));
            MigrateTable<APITaskRequest>(beStore, sqliteStore, nameof(APITaskRequest));
            MigrateTable<APITaskResult>(beStore, sqliteStore, nameof(APITaskResult));

            Console.WriteLine("Migration complete.");
        }

        static void MigrateTable<T>(AutoTest.Data.BigEntityStore beStore, AutoTest.Data.SqliteStore sqliteStore, string tableName) where T : class, new()
        {
            Console.WriteLine($"开始迁移表: {tableName}");
            int page = 1;
            long migrated = 0;
            while (true)
            {
                var batch = beStore.List<T>(tableName, page, Program.GetBatchSize()).ToList();
                if (batch == null || batch.Count == 0) break;
                for (int i = 0; i < batch.Count; i++)
                {
                    var entity = NormalizeComplexFields(batch[i]);
                    sqliteStore.Upsert(tableName, entity);
                    migrated++;
                    if (migrated % 100 == 0 || (i == batch.Count - 1 && page == 1))
                    {
                        Console.WriteLine($"{tableName}: 已迁移 {migrated} 条");
                    }
                }
                page++;
            }
            Console.WriteLine($"表 {tableName} 迁移完成，已迁移 {migrated} 条记录。");
        }

        private static T NormalizeComplexFields<T>(T entity) where T : class
        {
            if (entity == null)
            {
                return null;
            }

            var type = entity.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in props)
            {
                var pt = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (IsSimpleType(pt))
                {
                    continue;
                }

                object value = null;
                try
                {
                    value = prop.GetValue(entity);
                }
                catch
                {
                    continue;
                }

                if (value == null)
                {
                    continue;
                }

                try
                {
                    var json = JsonConvert.SerializeObject(value);
                    var normalized = JsonConvert.DeserializeObject(json, prop.PropertyType);
                    prop.SetValue(entity, normalized);
                }
                catch
                {
                    // ignore normalization failures to keep migration progressing
                }
            }

            return entity;
        }

        private static bool IsSimpleType(Type t)
        {
            if (t == null) return false;
            return t.IsPrimitive
                || t.IsEnum
                || t == typeof(string)
                || t == typeof(decimal)
                || t == typeof(DateTime)
                || t == typeof(Guid)
                || t == typeof(TimeSpan)
                || t == typeof(byte[]);
        }

        static int _batchSize = 1000;
        public static int GetBatchSize() => _batchSize;
    }
}