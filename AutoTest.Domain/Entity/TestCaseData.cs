using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestCaseData
    {
        public int Id
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        public List<ParamInfo> Params
        {
            get;
            set;
        }

        public List<ParamInfo> Headers
        {
            get;
            set;
        }

        public List<ParamInfo> FormDatas
        {
            get;
            set;
        }

        public List<ParamInfo> XWWWFormUrlEncoded
        {
            get;
            set;
        }

        /// <summary>
        /// raw请求的报文
        /// </summary>
        public string RawText
        {
            get;
            set;
        }

        public string BearToken
        {
            get;
            set;
        }

        public string ApiKeyName
        {
            get;
            set;
        }

        public string ApiKeyValue
        {
            get;
            set;
        }

        public int ApiKeyAddTo
        {
            get;
            set;
        }

        public string BasicUserName
        {
            get;
            set;
        }

        public string BasicUserPwd
        {
            get;
            set;
        }

        public List<ParamInfo> Cookies
        {
            get;
            set;
        }

        public List<ParamInfo> Multipart_form_data
        {
            get;
            set;
        }
    }
}
