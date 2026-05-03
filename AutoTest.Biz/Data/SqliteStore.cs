using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace AutoTest.Data
{
    public class SqliteStore : IDataStore, IDisposable
    {
        private readonly string _file;
        private readonly IDbConnection _conn;

        private static readonly Dictionary<string, string[]> CanonicalFields = new Dictionary<string, string[]>
        {
            { nameof(AutoTest.Domain.Entity.TestSource), new string[] { "Id" } },
            { nameof(AutoTest.Domain.Entity.TestSite), new[] { "SourceId", "Name" } },
            { nameof(AutoTest.Domain.Entity.TestLogin), new[] { "SiteId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TestPage), new[] { "SiteId", "Order" } },
            { nameof(AutoTest.Domain.Entity.TestCase), new[] { "PageId", "Order" } },
            { nameof(AutoTest.Domain.Entity.TestEnv), new[] { "SiteId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TestEnvParam), new[] { "SiteId", "EnvId", "Name" } },
            { nameof(AutoTest.Domain.Entity.TestCaseData), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TestCaseInvokeLog), new[] { "TestCaseId", "EnvId", "CDate" } },
            { nameof(AutoTest.Domain.Entity.TestCaseSetting), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TestCaseParam), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.APIDoc), new[] { "TestSourceId", "TestCaseId" } },
            { nameof(AutoTest.Domain.Entity.TestCaseDocExample), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.ProxyServer), new string[0] },
            { nameof(AutoTest.Domain.Entity.TestScript), new[] { "SourceId", "SiteId", "ScriptName" } },
            { nameof(AutoTest.Domain.Entity.TestResult), new[] { "TestCaseId", "EnvId", "TestStartDate" } },
            { nameof(AutoTest.Domain.Entity.FileDB), new[] { "FileName" } },
            { nameof(AutoTest.Domain.Entity.TestCookieContainer), new[] { "SiteId", "Env", "Account" } },
            { nameof(AutoTest.Domain.Entity.Counter), new[] { "CounterName" } },
            { nameof(AutoTest.Domain.Entity.TestTaskBag), new[] { "SiteId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TaskBagLog), new[] { "TaskBagId", "Id" } },
            { nameof(AutoTest.Domain.Entity.TestCaseUrlConfig), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.RequestInterceptConfig), new[] { "TestCaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.APITaskRequest), new[] { "CaseId", "Id" } },
            { nameof(AutoTest.Domain.Entity.APITaskResult), new[] { "TaskId", "Id" } }
        };

        public SqliteStore(string file)
        {
            _file = Path.GetFullPath(file);
            var needCreate = !File.Exists(_file);

            _conn = CreateSqliteConnection(_file);
            _conn.Open();

            if (needCreate)
            {
                foreach (var kv in CanonicalFields)
                {
                    CreateTableIfNotExists(kv.Key);
                }
            }
        }

        public IEnumerable<T> Scan<T>(string tableName, string indexName, object[] endKeys, object[] startKeys, int pageIndex, int pageSize, ref long total) where T : class, new()
        {
            // basic implementation: translate canonical fields into WHERE between startKeys and endKeys
            if (!CanonicalFields.TryGetValue(tableName, out var fields) || fields.Length == 0)
            {
                total = 0;
                return Enumerable.Empty<T>();
            }

            var usedFields = fields.Take(Math.Max(startKeys?.Length ?? 0, endKeys?.Length ?? 0)).ToArray();
            var whereParts = new List<string>();
            using (var cmd = _conn.CreateCommand())
            {
                // build range conditions, tolerate reversed start/end input
                for (int i = 0; i < usedFields.Length; i++)
                {
                    var fname = usedFields[i];

                    var hasStart = startKeys != null && i < startKeys.Length;
                    var hasEnd = endKeys != null && i < endKeys.Length;
                    var startVal = hasStart ? startKeys[i] : null;
                    var endVal = hasEnd ? endKeys[i] : null;

                    if (hasStart && hasEnd && startVal != null && endVal != null)
                    {
                        object minVal = startVal;
                        object maxVal = endVal;
                        if (TryCompare(startVal, endVal, out var cmp) && cmp > 0)
                        {
                            minVal = endVal;
                            maxVal = startVal;
                        }

                        whereParts.Add($"[{fname}] >= @s{i}");
                        var ps = cmd.CreateParameter(); ps.ParameterName = $"@s{i}"; ps.Value = minVal ?? DBNull.Value; cmd.Parameters.Add(ps);

                        whereParts.Add($"[{fname}] <= @e{i}");
                        var pe = cmd.CreateParameter(); pe.ParameterName = $"@e{i}"; pe.Value = maxVal ?? DBNull.Value; cmd.Parameters.Add(pe);
                    }
                    else
                    {
                        if (hasStart)
                        {
                            whereParts.Add($"[{fname}] >= @s{i}");
                            var ps = cmd.CreateParameter(); ps.ParameterName = $"@s{i}"; ps.Value = startVal ?? DBNull.Value; cmd.Parameters.Add(ps);
                        }
                        if (hasEnd)
                        {
                            whereParts.Add($"[{fname}] <= @e{i}");
                            var pe = cmd.CreateParameter(); pe.ParameterName = $"@e{i}"; pe.Value = endVal ?? DBNull.Value; cmd.Parameters.Add(pe);
                        }
                    }
                }

                var colsType = ResolveEntityType(tableName);
                var props = (colsType ?? typeof(T)).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToArray();
                var cols = string.Join(",", props.Select(prop => $"[{prop.Name}]"));

                var whereClause = whereParts.Count > 0 ? "WHERE " + string.Join(" AND ", whereParts) : string.Empty;

                // get total
                cmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}] {whereClause};";
                var o = cmd.ExecuteScalar();
                total = Convert.ToInt64(o);

                // select paged
                var pidx = Math.Max(1, pageIndex);
                var offset = (pidx - 1) * pageSize;
                cmd.CommandText = $"SELECT {cols} FROM [{tableName}] {whereClause} ORDER BY Id DESC LIMIT @limit OFFSET @offset;";
                var pl = cmd.CreateParameter(); pl.ParameterName = "@limit"; pl.Value = pageSize; cmd.Parameters.Add(pl);
                var po = cmd.CreateParameter(); po.ParameterName = "@offset"; po.Value = offset; cmd.Parameters.Add(po);

                using (var rdr = cmd.ExecuteReader())
                {
                    var list = new List<T>();
                    while (rdr.Read())
                    {
                        var inst = Activator.CreateInstance(colsType ?? typeof(T));
                        foreach (var prop in props)
                        {
                            int ord = -1;
                            try { ord = rdr.GetOrdinal(prop.Name); } catch { ord = -1; }
                            if (ord < 0) continue;
                            if (rdr.IsDBNull(ord)) continue;
                            var val = rdr.GetValue(ord);
                            SetPropertyValue(prop, inst, val);
                        }
                        list.Add((T)inst);
                    }
                    return list;
                }
            }
        }

        public void Delete<T>(string tableName, int id) where T : class, new()
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM [{tableName}] WHERE Id=@id;";
                var pid = cmd.CreateParameter(); pid.ParameterName = "@id"; pid.Value = id; cmd.Parameters.Add(pid);
                cmd.ExecuteNonQuery();
            }
        }

        private IDbConnection CreateSqliteConnection(string file)
        {
            var sysType = Type.GetType("System.Data.SQLite.SQLiteConnection, System.Data.SQLite");
            if (sysType != null)
            {
                var conn = (IDbConnection)Activator.CreateInstance(sysType, $"Data Source={file};Journal Mode=WAL;Pooling=True;");
                return conn;
            }

            var msType = Type.GetType("Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite");
            if (msType != null)
            {
                var conn = (IDbConnection)Activator.CreateInstance(msType, $"Data Source={file}");
                return conn;
            }

            throw new InvalidOperationException("No SQLite provider found. Please reference System.Data.SQLite or Microsoft.Data.Sqlite.");
        }

        private void CreateTableIfNotExists(string tableName)
        {
            var type = ResolveEntityType(tableName);
            if (type == null)
            {
                // fallback to generic table
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE TABLE IF NOT EXISTS [{tableName}] (Id INTEGER PRIMARY KEY AUTOINCREMENT);";
                    cmd.ExecuteNonQuery();
                }
                return;
            }

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.CanRead && pi.CanWrite).ToArray();

            var cols = new List<string>();
            foreach (var prop in props)
            {
                var colName = prop.Name;
                var colType = MapClrTypeToSqlite(prop.PropertyType);
                if (string.Equals(colName, "Id", StringComparison.OrdinalIgnoreCase))
                {
                    cols.Add($"Id INTEGER PRIMARY KEY AUTOINCREMENT");
                }
                else
                {
                    cols.Add($"[{colName}] {colType}");
                }
            }

            var create = $"CREATE TABLE IF NOT EXISTS [{tableName}] ({string.Join(", ", cols)});";
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = create;
                cmd.ExecuteNonQuery();
            }

            // create composite index based on CanonicalFields if present
            if (CanonicalFields.TryGetValue(tableName, out var indexCols) && indexCols.Length > 0)
            {
                var idxName = $"idx_{tableName}_{string.Join("_", indexCols)}";
                var colsList = string.Join(",", indexCols.Select(c => $"[{c}]"));
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE INDEX IF NOT EXISTS {idxName} ON [{tableName}] ({colsList});";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            try { _conn?.Close(); } catch { }
            try { _conn?.Dispose(); } catch { }
        }

        private static string ToKeyString(object o)
        {
            if (o == null) return string.Empty;
            if (o is DateTime dt) return dt.ToString("yyyyMMddHHmmssffff");
            if (o is long || o is int || o is short || o is byte || o is uint || o is ulong)
            {
                return string.Format("{0:D20}", Convert.ToInt64(o));
            }
            return o.ToString();
        }

        private string ComputeKeyText(string tableName, object[] values)
        {
            if (values == null || values.Length == 0) return string.Empty;
            var parts = values.Select(v => ToKeyString(v));
            return string.Join("_", parts);
        }

        private string ComputeKeyTextFromEntity(string tableName, object entity)
        {
            if (!CanonicalFields.TryGetValue(tableName, out var props) || props.Length == 0) return string.Empty;
            var t = entity.GetType();
            var values = new List<string>();
            foreach (var p in props)
            {
                var pi = t.GetProperty(p, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                object val = null;
                if (pi != null) val = pi.GetValue(entity);
                values.Add(ToKeyString(val));
            }
            return string.Join("_", values);
        }

        private Type ResolveEntityType(string tableName)
        {
            // try loaded assemblies
            var tname = "AutoTest.Domain.Entity." + tableName;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(tname);
                if (t != null) return t;
            }

            // Try to load the domain assembly explicitly if not already loaded
            try
            {
                var domainAsm = Assembly.Load("AutoTest.Domain");
                if (domainAsm != null)
                {
                    var t = domainAsm.GetType(tname);
                    if (t != null) return t;
                }
            }
            catch
            {
                // ignore load failures
            }

            return null;
        }

        public T FindById<T>(string tableName, int id) where T : class, new()
        {
            var type = ResolveEntityType(tableName) ?? typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanWrite).ToArray();
            using (var cmd = _conn.CreateCommand())
            {
                var cols = string.Join(",", props.Select(prop => $"[{prop.Name}]"));

                cmd.CommandText = $"SELECT {cols} FROM [{tableName}] WHERE Id=@id LIMIT 1;";
                var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = id; cmd.Parameters.Add(p);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    var inst = Activator.CreateInstance(type);
                    foreach (var prop in props)
                    {
                        int ord = -1;
                        try { ord = rdr.GetOrdinal(prop.Name); } catch { ord = -1; }
                        if (ord < 0) continue;
                        if (rdr.IsDBNull(ord)) continue;
                        var val = rdr.GetValue(ord);
                        SetPropertyValue(prop, inst, val);
                    }
                    return inst as T;
                }
            }
        }

        public T Find<T>(string tableName, int id) where T : class, new()
        {
            return FindById<T>(tableName, id);
        }

        public IEnumerable<T> Find<T>(string tableName, string indexName, object[] keys) where T : class, new()
        {
            if (!CanonicalFields.TryGetValue(tableName, out var fields) || fields.Length == 0)
            {
                return Enumerable.Empty<T>();
            }

            var safeKeys = keys ?? new object[0];
            var usedFields = fields.Take(safeKeys.Length).ToArray();
            var whereParts = new List<string>();
            using (var cmd = _conn.CreateCommand())
            {
                for (int i = 0; i < usedFields.Length; i++)
                {
                    var fname = usedFields[i];
                    whereParts.Add($"[{fname}] = @p{i}");
                    var par = cmd.CreateParameter(); par.ParameterName = $"@p{i}"; par.Value = safeKeys[i] ?? DBNull.Value; cmd.Parameters.Add(par);
                }

                var colsType = ResolveEntityType(tableName);
                var props = (colsType ?? typeof(T)).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToArray();
                var cols = string.Join(",", props.Select(prop => $"[{prop.Name}]"));
                var whereClause = whereParts.Count > 0 ? " WHERE " + string.Join(" AND ", whereParts) : string.Empty;
                cmd.CommandText = $"SELECT {cols} FROM [{tableName}]{whereClause};";

                using (var rdr = cmd.ExecuteReader())
                {
                    var list = new List<T>();
                    while (rdr.Read())
                    {
                        var inst = Activator.CreateInstance(colsType ?? typeof(T));
                        foreach (var prop in props)
                        {
                            int ord = -1;
                            try { ord = rdr.GetOrdinal(prop.Name); } catch { ord = -1; }
                            if (ord < 0) continue;
                            if (rdr.IsDBNull(ord)) continue;
                            var val = rdr.GetValue(ord);
                            SetPropertyValue(prop, inst, val);
                        }
                        list.Add((T)inst);
                    }
                    return list;
                }
            }
        }

        public IEnumerable<T> Find<T>(string tableName, Func<T, bool> predicate) where T : class, new()
        {
            if (predicate == null)
            {
                return Enumerable.Empty<T>();
            }

            return List<T>(tableName, 1, int.MaxValue).Where(predicate).ToList();
        }

        public IEnumerable<T> FindBatch<T>(string tableName, IEnumerable<object> keys) where T : class, new()
        {
            if (keys == null)
            {
                return Enumerable.Empty<T>();
            }

            return keys
                .Select(k =>
                {
                    var id = 0;
                    try { id = Convert.ToInt32(k); } catch { id = 0; }
                    return id > 0 ? FindById<T>(tableName, id) : null;
                })
                .Where(p => p != null)
                .ToList();
        }

        public long Count(string tableName, string indexName, object[] keys)
        {
            if (!CanonicalFields.TryGetValue(tableName, out var fields) || fields.Length == 0)
            {
                return 0;
            }

            var usedFields = fields.Take(keys?.Length ?? 0).ToArray();
            var whereParts = new List<string>();
            using (var cmd = _conn.CreateCommand())
            {
                for (int i = 0; i < usedFields.Length; i++)
                {
                    var fname = usedFields[i];
                    whereParts.Add($"[{fname}] = @p{i}");
                    var par = cmd.CreateParameter(); par.ParameterName = $"@p{i}"; par.Value = keys[i] ?? DBNull.Value; cmd.Parameters.Add(par);
                }

                var whereClause = whereParts.Count > 0 ? " WHERE " + string.Join(" AND ", whereParts) : string.Empty;
                cmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}]{whereClause};";
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        public void InsertBatch<T>(string tableName, IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null)
            {
                return;
            }

            foreach (var entity in entities)
            {
                Insert(tableName, entity);
            }
        }

        public IEnumerable<T> List<T>(string tableName, int pageIndex, int pageSize) where T : class, new()
        {
            var type = ResolveEntityType(tableName) ?? typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToArray();
            using (var cmd = _conn.CreateCommand())
            {
                var cols = string.Join(",", props.Select(prop => $"[{prop.Name}]"));
                cmd.CommandText = $"SELECT {cols} FROM [{tableName}] ORDER BY Id LIMIT @limit OFFSET @offset;";
                var p = cmd.CreateParameter(); p.ParameterName = "@limit"; p.Value = pageSize; cmd.Parameters.Add(p);
                var p2 = cmd.CreateParameter(); p2.ParameterName = "@offset"; p2.Value = (pageIndex - 1) * pageSize; cmd.Parameters.Add(p2);
                using (var rdr = cmd.ExecuteReader())
                {
                    var list = new List<T>();
                    while (rdr.Read())
                    {
                        var inst = Activator.CreateInstance(type);
                        foreach (var prop in props)
                        {
                            int ord = -1;
                            try { ord = rdr.GetOrdinal(prop.Name); } catch { ord = -1; }
                            if (ord < 0) continue;
                            if (rdr.IsDBNull(ord)) continue;
                            var val = rdr.GetValue(ord);
                            SetPropertyValue(prop, inst, val);
                        }
                        list.Add((T)inst);
                    }
                    return list;
                }
            }
        }

        public void Insert<T>(string tableName, T entity) where T : class, new()
        {
            var type = ResolveEntityType(tableName) ?? entity.GetType();
            var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanWrite).ToArray();
            // include Id column only when entity has an explicit Id value (>0) to preserve IDs during migration
            var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            object idVal = null;
            if (idProp != null)
            {
                idVal = idProp.GetValue(entity);
            }

            var props = allProps.Where(prop => prop.CanWrite && !(string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase) && (idVal == null || Convert.ToInt64(idVal) == 0))).ToArray();
            using (var cmd = _conn.CreateCommand())
            {
                var colNames = string.Join(",", props.Select(prop => $"[{prop.Name}]"));
                var paramNames = string.Join(",", props.Select((prop, i) => $"@p{i}"));
                cmd.CommandText = $"INSERT INTO [{tableName}] ({colNames}) VALUES ({paramNames});";
                for (int i = 0; i < props.Length; i++)
                {
                    var par = cmd.CreateParameter(); par.ParameterName = $"@p{i}"; par.Value = GetDbValue(props[i].GetValue(entity)); cmd.Parameters.Add(par);
                }
                cmd.ExecuteNonQuery();
            }

            if (idProp != null && (idVal == null || Convert.ToInt64(idVal) == 0))
            {
                using (var idCmd = _conn.CreateCommand())
                {
                    idCmd.CommandText = "SELECT last_insert_rowid();";
                    var newId = Convert.ToInt32(idCmd.ExecuteScalar());
                    idProp.SetValue(entity, Convert.ChangeType(newId, idProp.PropertyType));
                }
            }
        }

        public void Update<T>(string tableName, T entity) where T : class, new()
        {
            var type = ResolveEntityType(tableName) ?? entity.GetType();
            var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (idProp == null) throw new InvalidOperationException("Update requires entity with Id property.");
            var idVal = idProp.GetValue(entity);
            if (idVal == null) throw new InvalidOperationException("Entity Id is null.");

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)).ToArray();
            using (var cmd = _conn.CreateCommand())
            {
                var setParts = string.Join(",", props.Select((p, i) => $"[{p.Name}] = @p{i}"));
                cmd.CommandText = $"UPDATE [{tableName}] SET {setParts} WHERE Id=@id;";
                for (int i = 0; i < props.Length; i++)
                {
                    var par = cmd.CreateParameter(); par.ParameterName = $"@p{i}"; par.Value = GetDbValue(props[i].GetValue(entity)); cmd.Parameters.Add(par);
                }
                var pid = cmd.CreateParameter(); pid.ParameterName = "@id"; pid.Value = Convert.ToInt64(idVal); cmd.Parameters.Add(pid);
                cmd.ExecuteNonQuery();
            }
        }

        public void Upsert<T>(string tableName, T entity) where T : class, new()
        {
            var t = entity.GetType();
            var pi = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null)
            {
                Insert(tableName, entity);
                return;
            }

            var idVal = pi.GetValue(entity);
            if (idVal == null || Convert.ToInt64(idVal) == 0)
            {
                Insert(tableName, entity);
                return;
            }

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}] WHERE Id=@id;";
                var pid = cmd.CreateParameter(); pid.ParameterName = "@id"; pid.Value = Convert.ToInt64(idVal); cmd.Parameters.Add(pid);
                var o = cmd.ExecuteScalar();
                var exists = Convert.ToInt64(o) > 0;
                if (exists) Update(tableName, entity); else Insert(tableName, entity);
            }
        }

        private static object GetDbValue(object val)
        {
            if (val == null) return DBNull.Value;

            var t = val.GetType();
            var nt = Nullable.GetUnderlyingType(t) ?? t;

            if (nt == typeof(DateTime)) return ((DateTime)val).ToString("o");
            if (nt == typeof(bool)) return (bool)val ? 1 : 0;
            if (nt.IsEnum) return Convert.ToInt32(val);
            if (nt == typeof(Guid)) return val.ToString();

            if (IsSimpleType(nt) || val is byte[])
            {
                return val;
            }

            return JsonConvert.SerializeObject(val);
        }

        private static void SetPropertyValue(PropertyInfo prop, object inst, object val)
        {
            if (val == null || val is DBNull) return;
            var pt = prop.PropertyType;
            var nt = Nullable.GetUnderlyingType(pt) ?? pt;

            try
            {
                if (nt == typeof(string))
                {
                    prop.SetValue(inst, Convert.ToString(val));
                }
                else if (nt == typeof(int))
                {
                    prop.SetValue(inst, Convert.ToInt32(val));
                }
                else if (nt == typeof(long))
                {
                    prop.SetValue(inst, Convert.ToInt64(val));
                }
                else if (nt == typeof(bool))
                {
                    if (val is bool vb)
                    {
                        prop.SetValue(inst, vb);
                    }
                    else
                    {
                        var sval = Convert.ToString(val);
                        if (string.Equals(sval, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(sval, "false", StringComparison.OrdinalIgnoreCase))
                        {
                            prop.SetValue(inst, bool.Parse(sval));
                        }
                        else
                        {
                            prop.SetValue(inst, Convert.ToInt32(val) != 0);
                        }
                    }
                }
                else if (nt == typeof(DateTime))
                {
                    prop.SetValue(inst, DateTime.Parse(Convert.ToString(val)));
                }
                else if (nt == typeof(Guid))
                {
                    prop.SetValue(inst, Guid.Parse(Convert.ToString(val)));
                }
                else if (nt.IsEnum)
                {
                    if (val is string es)
                    {
                        prop.SetValue(inst, Enum.Parse(nt, es, true));
                    }
                    else
                    {
                        prop.SetValue(inst, Enum.ToObject(nt, Convert.ToInt32(val)));
                    }
                }
                else if (pt == typeof(byte[]))
                {
                    prop.SetValue(inst, val as byte[]);
                }
                else if (!IsSimpleType(nt))
                {
                    var json = Convert.ToString(val);
                    if (string.IsNullOrWhiteSpace(json)) return;
                    var obj = JsonConvert.DeserializeObject(json, pt);
                    if (obj != null)
                    {
                        prop.SetValue(inst, obj);
                    }
                }
                else
                {
                    prop.SetValue(inst, Convert.ChangeType(val, nt));
                }
            }
            catch { }
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

        private static string MapClrTypeToSqlite(Type t)
        {
            if (t == null) return "TEXT";
            var nt = Nullable.GetUnderlyingType(t) ?? t;
            if (nt == typeof(int) || nt == typeof(long) || nt.IsEnum) return "INTEGER";
            if (nt == typeof(bool)) return "INTEGER";
            if (nt == typeof(float) || nt == typeof(double) || nt == typeof(decimal)) return "REAL";
            if (nt == typeof(byte[])) return "BLOB";
            if (nt == typeof(DateTime)) return "TEXT";
            return "TEXT";
        }

        public void Init()
        {
        }

        private static bool TryCompare(object left, object right, out int result)
        {
            result = 0;
            if (left == null || right == null)
            {
                return false;
            }

            if (left is IComparable lc)
            {
                try
                {
                    var rightType = right.GetType();
                    var leftType = left.GetType();
                    if (leftType != rightType)
                    {
                        right = Convert.ChangeType(right, leftType);
                    }
                    result = lc.CompareTo(right);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
