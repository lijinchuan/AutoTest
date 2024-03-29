﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestReport
    {
        public int Id
        {
            get;
            set;
        }

        public string TestName
        {
            get;
            set;
        }

        public DateTime TestTime
        {
            get;
            set;
        }

        public int SourceId
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int PageId
        {
            get;
            set;
        }

        public int CaseId
        {
            get;
            set;
        }
    }
}
