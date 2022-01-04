using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity.OldVerion
{
    public class TestCaseV2
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

        public string Url
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

        public WebMethod WebMethod
        {
            get;
            set;
        }

        public BodyDataType BodyDataType
        {
            get;
            set;
        }

        public ApplicationType ApplicationType
        {
            get;
            set;
        }

        public AuthType AuthType
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public int ApiEnvId
        {
            get;
            set;
        }
    }
}
