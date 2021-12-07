using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Model
{
    public class WebVar
    {
        public static string VarName = "$VewBag";

        /// <summary>
        /// 数据
        /// </summary>
        public List<WebRequestData> WebRequestDatas
        {
            get;
            set;
        }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object Bag
        {
            get;
            set;
        }
    }
}
