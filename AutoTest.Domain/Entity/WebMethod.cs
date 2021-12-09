﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public enum WebMethod
    {
        POST = 0,
        GET = 1,
        HEAD = 2,
        PUT = 3,
        XPUT = 4,
        DELETE = 5,
        XDELETE = 6,
        TRACE = 7,
        OPTIONS = 8,
        PATCH = 9
    }
}
