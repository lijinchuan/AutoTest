using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    public class ScriptKeyWord : IEqualityComparer<ScriptKeyWord>
    {
        public string KeyWord
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public Color HighColor
        {
            get;
            internal set;
        }


        public bool Equals(ScriptKeyWord x, ScriptKeyWord y)
        {
            if (x.KeyWord == null && y.KeyWord == null)
            {
                return true;
            }

            if (x.KeyWord == null || y.KeyWord == null)
            {
                return false;
            }

            var ret = x.KeyWord.ToUpper() == y.KeyWord.ToUpper();
            if (ret)
            {
                if (string.IsNullOrEmpty(x.Desc))
                {
                    x.Desc = y.Desc;
                }
                else if (string.IsNullOrEmpty(y.Desc))
                {
                    y.Desc = x.Desc;
                }
            }
            return ret;
        }

        public int GetHashCode(ScriptKeyWord obj)
        {
            if (obj.KeyWord == null)
            {
                return 0;
            }
            return obj.KeyWord.GetHashCode();
        }
    }

    public static class ScriptKeyWordHelper
    {
        private static List<ScriptKeyWord> KeyWordDic = new List<ScriptKeyWord>();

        static ScriptKeyWordHelper()
        {
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "*",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = ",",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "@@error",
                Desc = "全局错误"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "@@fetch_status",
                Desc = "游标状态"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "@@rowcount",
                Desc = "行数"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "[",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "]",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "add",
                Desc = "添加"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "all",
                Desc = "所有,不去重,全"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "alter",
                Desc = "更改"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "and",
                Desc = "与,并且"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ansi_nulls",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "as",
                Desc = "别名"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "asc",
                Desc = "正序"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "avg",
                Desc = "平均"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "begin",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "between",
                Desc = "范围"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "bigint",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "binary",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "bit",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "by",
                Desc = "根据"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "case",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "cast",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "char",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "char(n)",
                Desc = "字符"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "charindex",
                Desc = "查找"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "close",
                Desc = "游标关闭"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "coalesce",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "column",
                Desc = "列"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "commit",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "convert",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "constraint",
                Desc = "约束"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "count",
                Desc = "总计"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "create",
                Desc = "创建"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "cursor",
                Desc = "游标"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "date",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datediff",
                Desc = "计算时间差，DATEDIFF(year|quarter|month|week|day|hour|minute|second|millisecond,开始时间,结束时间)",
                HighColor = Color.Red
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datename",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datepart",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datetime",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datetime2",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "datetimeoffset",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "day",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "deallocate",
                Desc = "游标释放"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "decimal",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "declare",
                Desc = "申明"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "delete",
                Desc = "删除"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "desc",
                Desc = "倒序"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "difference",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "distinct",
                Desc = "去重"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "drop",
                Desc = "移除"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "else",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "end",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "except",
                Desc = "差集"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "exec",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "execute",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "exists",
                Desc = "存在"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "fetch",
                Desc = "游标获取"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "float",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "float(n)",
                Desc = "精度至少位n位的浮点数"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "for",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "foreign key(C) references T",
                Desc = "外键，括号中为外键，references后为外键的表"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "from",
                Desc = "从"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "getdate",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "group",
                Desc = "分组"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "hash",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "having",
                Desc = "对group by产生的分组进行筛选，可以使用聚集函数"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "hour",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "if",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "inner",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "inner",
                Desc = "内联"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "insert",
                Desc = "插入"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "int",
                Desc = "整数类型"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "intersect",
                Desc = "相交"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "into",
                Desc = "到"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "is",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "isdate",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "isnull",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "join",
                Desc = "关联"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "left",
                Desc = "左联"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "len",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "like",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "lower",
                Desc = "小写"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ltrim",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "max",
                Desc = "最大"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "mid",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "millisecond",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "min",
                Desc = "最小"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "minute",
                Desc = "分钟"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "money",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "month",
                Desc = "月"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "nchar",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "nocount",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "nolock",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "not",
                Desc = "非,不"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ntext",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "null",
                Desc = "空"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "numeric",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "numeric(p,d)",
                Desc = "定点数，精度由用户指定"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "nvarchar",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "on",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "open",
                Desc = "游标打开"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "option",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "or",
                Desc = "或者"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "order",
                Desc = "排序"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "output",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "parsename",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "patindex",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "primary key(KEY)",
                Desc = "主键,后面括号中是作为主键的属性"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "print",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "proc",
                Desc = "存储过程"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "procedure",
                Desc = "存储过程"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "quarter",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "quoted_identifier",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "quotename",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "real",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "real",
                Desc = "浮点数和双精度浮点数"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "recompile",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "replace",
                Desc = "替换"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "replicate",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "return",
                Desc = "返回"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "reverse",
                Desc = "反转"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "right",
                Desc = "右联"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "rollback",
                Desc = "回滚"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "round",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "rtrim",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "second",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "select",
                Desc = "查询"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "set",
                Desc = "设置"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "smalldatetime",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "smallint",
                Desc = "小整数类型"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "smallmoney",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "soundex",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "sp_addextendedproperty",
                Desc = "备注,示例：EXEC sp_addextendedproperty N'MS_Description', N'备注内容', N'SCHEMA', N'dbo',N'TABLE', N'表名', N'COLUMN', N'字段名'"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "sp_executesql",
                Desc = "执行SQL语句"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "space",
                Desc = "空格"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "sql_variant",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "stuff",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "sum",
                Desc = "求和"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "table",
                Desc = "表"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "text",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "then",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "time",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "timestamp",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "tinyint",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "top",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "transaction",
                Desc = "事务"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "union",
                Desc = "合并"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "uniqueidentifier",
                Desc = "全局唯一的标识,NewID()"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "update",
                Desc = "更新"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "upper",
                Desc = "大写"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "use",
                Desc = "使用"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "using",
                Desc = "使用"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "values",
                Desc = "插入值"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "values",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "varbinary",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "varchar",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "varchar(n)",
                Desc = "可变字符"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "view",
                Desc = "视图"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "week",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "when",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "where",
                Desc = "条件"
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "while",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "with",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "xml",
                Desc = ""
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "year",
                Desc = ""
            });
        }

        private static void Deal()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kw in KeyWordDic.Distinct().OrderBy(p => p.KeyWord))
            {
                sb.AppendLine(@"KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = """ + kw.KeyWord + @""",Desc = """ + kw.Desc + @"""
            });");
            }

            System.Diagnostics.Trace.WriteLine(sb.ToString());
        }

        public static List<ScriptKeyWord> GetKeyWordList()
        {
            return KeyWordDic;
        }

    }
}
