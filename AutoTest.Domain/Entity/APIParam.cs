﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCaseParam : IEquatable<TestCaseParam>,IComparable
    {
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// 0-入参 1-出参
        /// </summary>
        public int Type
        {
            get;
            set;
        }

        public int APISourceId
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        public int Sort
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequried
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Desc
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestCaseParam)
            {
                return this.Id.CompareTo(((TestCaseParam)obj).Id);
            }

            return 1;
        }

        public bool Equals(TestCaseParam other)
        {
            if (other == null)
            {
                return false;
            }

            return this.TestCaseId == other.TestCaseId &&
                this.APISourceId == other.APISourceId &&
                this.Desc == other.Desc &&
                this.Id == other.Id &&
                this.IsRequried == other.IsRequried &&
                this.Name == other.Name &&
                this.Sort == other.Sort &&
                this.Type == other.Type &&
                this.TypeName == other.TypeName;
        }
    }
}
