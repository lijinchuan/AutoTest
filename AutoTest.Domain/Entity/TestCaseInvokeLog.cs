using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCaseInvokeLog
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

        public string TestCaseName
        {
            get;
            set;
        }

        public int SourceId
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string OrgPath
        {
            get;
            set;
        }

        public WebMethod APIMethod
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

        public int EnvId
        {
            get;
            set;
        }

        public TestCaseData Data
        {
            get;
            set;
        }

        public TestCaseData OrgData
        {
            get;
            set;
        }

        public APIResonseResult APIResonseResult
        {
            get;
            set;
        }

        public int StatusCode
        {
            get;
            set;
        }

        public string RespMsg
        {
            get;
            set;
        }

        public long RespSize
        {
            get;
            set;
        }

        public long Ms
        {
            get;
            set;
        }

        public string ResponseText
        {
            get;
            set;
        }

        public DateTime CDate
        {
            get;
            set;
        } = new DateTime(1970, 1, 1);

        public StringBuilder GetRequestDetail()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"url:{this.Path}");
            sb.AppendLine();

            sb.AppendLine($"method:{this.APIMethod.ToString()}");

            sb.AppendLine();
            sb.AppendLine("headers:");
            if (this.Data?.Headers?.Count() > 0)
            {
                foreach (var header in this.Data.Headers)
                {
                    if (header.Name.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        if (header.Checked)
                        {
                            sb.AppendLine($" UserAgent:{header.Value}");
                        }
                    }
                    else if (header.Name.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        if (header.Checked)
                        {
                            sb.AppendLine($" Accept:{header.Value}");
                        }
                    }
                    else if (header.Name.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else
                    {
                        if (header.Checked)
                        {
                            sb.AppendLine($" {header.Name}:{header.Value}");
                        }
                    }
                }
            }

            var authtype = this.AuthType;
            if (authtype == AuthType.Bearer)
            {
                sb.AppendLine($" Authorization:Bearer {this.Data?.BearToken}");
            }
            else if (authtype == AuthType.ApiKey)
            {
                if (this.Data?.ApiKeyAddTo == 0)
                {
                    sb.AppendLine($" {this.Data?.ApiKeyName}:{this.Data?.ApiKeyValue}");
                }
            }
            else if (authtype == AuthType.Basic)
            {
                sb.AppendLine($" Authorization:Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.Data?.BasicUserName}:{this.Data?.BasicUserPwd}"))}");
            }

            sb.AppendLine($" content-type:application/{this.ApplicationType}");
            sb.AppendLine();

            if (this.Data.Cookies != null)
            {
                sb.AppendLine("cookie:");
                foreach (var cookie in this.Data.Cookies)
                {
                    sb.AppendLine($" {cookie.Name}:{cookie.Value}");
                }
            }

            sb.AppendLine("body:");
            var bodydataType = this.BodyDataType;
            if (bodydataType == BodyDataType.formdata)
            {
                sb.AppendLine($" formdata:{string.Join("&", this.Data?.FormDatas.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }
            else if (bodydataType == BodyDataType.xwwwformurlencoded)
            {
                sb.AppendLine($" xwwwformurlencoded:{string.Join("&", this.Data?.XWWWFormUrlEncoded.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }
            else if (bodydataType == BodyDataType.raw || bodydataType == BodyDataType.wcf)
            {
                sb.AppendLine($"raw:{this.Data?.RawText}");
            }
            else if (bodydataType == BodyDataType.binary)
            {
                sb.AppendLine($" multipart/form-data:{string.Join("&", this.Data?.Multipart_form_data.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }
            else
            {
                sb.AppendLine($"raw:");
            }

            return sb;
        }

        public StringBuilder GetRequestBody()
        {
            StringBuilder sb = new StringBuilder();

            var bodydataType = this.BodyDataType;
            if (bodydataType == BodyDataType.formdata)
            {
                sb.AppendLine($"{string.Join("&", this.Data?.FormDatas.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }
            else if (bodydataType == BodyDataType.xwwwformurlencoded)
            {
                sb.AppendLine($"{string.Join("&", this.Data?.XWWWFormUrlEncoded.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }
            else if (bodydataType == BodyDataType.raw || bodydataType == BodyDataType.wcf)
            {
                sb.AppendLine($"{this.Data?.RawText}");
            }
            else if (bodydataType == BodyDataType.binary)
            {
                sb.AppendLine($"{string.Join("&", this.Data?.Multipart_form_data.Where(p => p.Checked).Select(p => p.Name + "=" + p.Value))}");
            }

            return sb;
        }

        public StringBuilder GetRespDetail()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"status code:{this.StatusCode}");
            sb.AppendLine($"耗时:{this.Ms}毫秒");
            sb.AppendLine($"响应大小:{this.RespSize}B");
            sb.AppendLine();
            sb.AppendLine("headers:");
            if (this.APIResonseResult?.Headers != null)
            {
                foreach (var h in this.APIResonseResult?.Headers)
                {
                    sb.AppendLine($" {h.Key}:{h.Value}");
                }
                sb.AppendLine();
                sb.AppendLine("body:");
                sb.AppendLine(this.ResponseText);
            }

            return sb;
        }

        public StringBuilder GetRespBody()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.ResponseText);

            return sb;
        }
    }
}
