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
        LOGINACCOUNTS,
        TESTSITE,
        TESTPAGE,
        TESTCASE,
        SCRIPTPARENT,
        UNKNOWN = 999
    }
}
