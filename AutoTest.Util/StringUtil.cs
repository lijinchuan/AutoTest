using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string GetMD5(string str)
        {
            //将字符串编码为字节序列
            byte[] bt = Encoding.UTF8.GetBytes(str);
            //创建默认实现的实例
            var md5 = MD5.Create();
            //计算指定字节数组的哈希值。
            var md5bt = md5.ComputeHash(bt);
            //将byte数组转换为字符串
            StringBuilder builder = new StringBuilder();
            foreach (var item in md5bt)
            {
                _ = builder.Append(item.ToString("X2"));
            }
            return builder.ToString();
        }
    }
}
