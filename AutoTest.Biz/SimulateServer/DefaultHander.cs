﻿using LJC.FrameWorkV3.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz.SimulateServer
{
    public class DefaultHander : IRESTfulHandler
    {
        public bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param)
        {
            if (request.Url.EndsWith("index"))
            {
                response.ContentType = "text/html;charset=utf-8;";
                response.Content = "好的，我正在工作。";
                return true;
            }
            throw new NotImplementedException();
        }
    }
}
