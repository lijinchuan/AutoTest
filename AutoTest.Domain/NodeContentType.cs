using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain
{
    public enum NodeContentType
    {
        SEVER,
        APISOURCE,
        APIPARENT,
        API,
        ENVPARENT,
        ENV,
        DOCPARENT,
        DOC,
        LOGICMAPParent,
        LOGICMAP,
        LOGINPAGE,
        TESTSITE,
        TESTPAGE,
        TESTCASE,
        UNKNOWN = 999
    }
}
