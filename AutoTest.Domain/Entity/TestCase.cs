﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    /// <summary>
    /// 测试实例
    /// </summary>
    public class TestCase
    {
        public int Id
        {
            get;
            set;
        }

        public int PageId
        {
            get;
            set;
        }

        public string CaseName
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }

        public string TestCode
        {
            get;
            set;
        }

        public string ValidCode
        {
            get;
            set;
        }
    }
}
