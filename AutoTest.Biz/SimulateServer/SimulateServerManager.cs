using AutoTest.Biz.RemoteControl;
using LJC.FrameWorkV3.Net.HTTP.Server;
using System.Collections.Generic;

namespace AutoTest.Biz.SimulateServer
{
    public static class SimulateServerManager
    {
        static HttpServer manhttpserver = null;

        private static void InitServer()
        {
            manhttpserver.Handlers.Add(new RESTfulApiHandlerBase(HMethod.GET, "/index", new List<string>() { }, new DefaultHander()));
            manhttpserver.Handlers.Add(new ApiSimulateHandler());
            manhttpserver.Handlers.Add(new RemoteControlHandler());
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool StartServer(int port)
        {
            if (manhttpserver == null)
            {
                manhttpserver = new HttpServer(new Server(port));
                InitServer();
                return true;
            }
            else if (manhttpserver.Server.Port != port)
            {
                manhttpserver.Server.Close();

                manhttpserver = new HttpServer(new Server(port));

                InitServer();
                return true;
            }

            return false;
        }

        public static bool Stop()
        {
            if (manhttpserver != null && manhttpserver.Server != null)
            {
                manhttpserver.Server.Close();
                manhttpserver = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 配置远程控制截图区域（屏幕坐标）
        /// </summary>
        public static void SetRemoteControlRegion(int x, int y, int width, int height)
        {
            ScreenCaptureService.Instance.SetRegion(x, y, width, height);
        }
    }
}
