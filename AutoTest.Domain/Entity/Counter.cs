﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class Counter
    {
        public int Id
        {
            get;
            set;
        }

        public string CounterName
        {
            get;
            set;
        }

        public long Value
        {
            get;
            set;
        }
    }
}
