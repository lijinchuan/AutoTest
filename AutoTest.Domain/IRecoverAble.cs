﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain
{
    public interface IRecoverAble
    {
        object[] GetRecoverData();

        IRecoverAble Recover(object[] recoverData);
    }
}
