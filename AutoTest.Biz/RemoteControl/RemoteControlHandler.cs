using LJC.FrameWorkV3.Net.HTTP.Server;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace AutoTest.Biz.RemoteControl
{
    /// <summary>
    /// 远程控制HTTP处理器，路由前缀: remotecontrol/
    /// </summary>
    public class RemoteControlHandler : IHttpHandler
    {
        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            var url = NormalizeUrl(request.Url);
            if (!url.StartsWith("remotecontrol/", StringComparison.OrdinalIgnoreCase))
                return false;

            var path = url.Substring("remotecontrol/".Length).Split('?')[0].Trim('/').ToLower();

            try
            {
                switch (path)
                {
                    case "view":
                        response.ContentType = "text/html;charset=utf-8";
                        response.Content = BuildViewHtml();
                        return true;

                    case "screenshot":
                        var bytes = ScreenCaptureService.Instance.CaptureJpegBytes();
                        response.ContentType = "image/jpeg";
                        response.Header["Cache-Control"] = "no-store, no-cache";
                        response.Header["Pragma"] = "no-cache";
                        response.RawContent = bytes;
                        return true;

                    case "mousedown":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        Win32MouseSimulator.LeftDown(pt.X, pt.Y);
                        WriteOk(response);
                        return true;
                    }

                    case "mousemove":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        Win32MouseSimulator.MoveTo(pt.X, pt.Y);
                        WriteOk(response);
                        return true;
                    }

                    case "mouseup":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        Win32MouseSimulator.LeftUp(pt.X, pt.Y);
                        WriteOk(response);
                        return true;
                    }

                    case "setregion":
                    {
                        var req = JsonConvert.DeserializeAnonymousType(
                            request.GetContent(),
                            new { x = 0, y = 0, width = 1280, height = 720, quality = 70 });
                        ScreenCaptureService.Instance.SetRegion(req.x, req.y, req.width, req.height);
                        ScreenCaptureService.Instance.JpegQuality = Math.Max(1, Math.Min(100, req.quality));
                        WriteOk(response);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ContentType = "application/json;charset=utf-8";
                response.Content = JsonConvert.SerializeObject(new { code = 500, message = ex.Message });
                return true;
            }

            return false;
        }

        private static (double x, double y) ParseMouseRequest(HttpRequest request)
        {
            if ("get".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                return (double.Parse(request.Query["x"]), double.Parse(request.Query["y"]));
            }
            var req = JsonConvert.DeserializeAnonymousType(request.GetContent(), new { x = 0.0, y = 0.0 });
            return (req.x, req.y);
        }

        private static void WriteOk(HttpResponse response)
        {
            response.ContentType = "application/json;charset=utf-8";
            response.Content = "{\"code\":200}";
        }

        private static string NormalizeUrl(string url)
        {
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var parts = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 2 ? string.Join("/", parts, 2, parts.Length - 2) : string.Empty;
            }
            return string.Join("/", url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string BuildViewHtml()
        {
            return
@"<!DOCTYPE html>
<html lang='zh-CN'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width,initial-scale=1.0,user-scalable=no'>
<title>远程控制</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{background:#1a1a1a;display:flex;flex-direction:column;align-items:center;justify-content:center;min-height:100vh;font-family:monospace}
#info{color:#aaa;font-size:13px;margin-bottom:8px}
#wrap{position:relative;cursor:crosshair;border:2px solid #555;display:inline-block;max-width:100vw}
#screen{display:block;max-width:100%;height:auto;-webkit-user-select:none;user-select:none;-webkit-touch-callout:none}
#status{color:#4caf50;font-size:12px;margin-top:6px}
#cfg{margin-top:12px;color:#888;font-size:12px;display:flex;gap:8px;flex-wrap:wrap;justify-content:center}
#cfg input{width:60px;padding:2px 4px;background:#333;border:1px solid #555;color:#ccc;font-size:12px}
#cfg button{padding:2px 8px;background:#444;border:1px solid #666;color:#ccc;cursor:pointer;font-size:12px}
</style>
</head>
<body>
<div id='info'>实时远程控制 &nbsp;|&nbsp; 鼠标 / 触控拖动以同步操作</div>
<div id='wrap'><img id='screen' src='/remotecontrol/screenshot' alt='screen' draggable='false'></div>
<div id='status'>连接中...</div>
<div id='cfg'>
  截图区域 &nbsp;
  X:<input id='rx' type='number' value='0'>
  Y:<input id='ry' type='number' value='0'>
  W:<input id='rw' type='number' value='1280'>
  H:<input id='rh' type='number' value='720'>
  质量:<input id='rq' type='number' value='70' min='1' max='100'>
  <button onclick='applyRegion()'>应用</button>
</div>
<script>
(function(){
  var img=document.getElementById('screen'),
      status=document.getElementById('status'),
      isDown=false,pending=null,moving=false;

  function refreshScreen(){
    var t=new Image();
    t.onload=function(){img.src=t.src;status.textContent='已连接 '+new Date().toLocaleTimeString();};
    t.onerror=function(){status.textContent='截图失败，重试中...';};
    t.src='/remotecontrol/screenshot?_='+Date.now();
  }
  setInterval(refreshScreen,200);

  function norm(e){
    var r=img.getBoundingClientRect(),
        cx=e.touches?e.touches[0].clientX:e.clientX,
        cy=e.touches?e.touches[0].clientY:e.clientY;
    return{x:Math.max(0,Math.min(1,(cx-r.left)/r.width)),
           y:Math.max(0,Math.min(1,(cy-r.top)/r.height))};
  }
  function normEnd(e){
    var r=img.getBoundingClientRect(),t=e.changedTouches[0];
    return{x:Math.max(0,Math.min(1,(t.clientX-r.left)/r.width)),
           y:Math.max(0,Math.min(1,(t.clientY-r.top)/r.height))};
  }

  function send(path,data){
    var xhr=new XMLHttpRequest();
    xhr.open('POST','/remotecontrol/'+path,true);
    xhr.setRequestHeader('Content-Type','application/json');
    xhr.send(JSON.stringify(data));
  }
  function sendMove(pos){
    if(moving){pending=pos;return;}
    moving=true;
    var xhr=new XMLHttpRequest();
    xhr.open('POST','/remotecontrol/mousemove',true);
    xhr.setRequestHeader('Content-Type','application/json');
    xhr.onreadystatechange=function(){
      if(xhr.readyState!==4)return;
      moving=false;
      if(pending){var p=pending;pending=null;sendMove(p);}
    };
    xhr.send(JSON.stringify(pos));
  }

  img.addEventListener('mousedown',function(e){
    if(e.button!==0)return;e.preventDefault();isDown=true;send('mousedown',norm(e));
  });
  window.addEventListener('mousemove',function(e){if(!isDown)return;sendMove(norm(e));});
  window.addEventListener('mouseup',function(e){if(!isDown)return;isDown=false;send('mouseup',norm(e));});

  img.addEventListener('touchstart',function(e){e.preventDefault();isDown=true;send('mousedown',norm(e));},{passive:false});
  img.addEventListener('touchmove',function(e){e.preventDefault();sendMove(norm(e));},{passive:false});
  img.addEventListener('touchend',function(e){e.preventDefault();isDown=false;send('mouseup',normEnd(e));},{passive:false});
})();

function applyRegion(){
  var d={
    x:parseInt(document.getElementById('rx').value)||0,
    y:parseInt(document.getElementById('ry').value)||0,
    width:parseInt(document.getElementById('rw').value)||1280,
    height:parseInt(document.getElementById('rh').value)||720,
    quality:parseInt(document.getElementById('rq').value)||70
  };
  var xhr=new XMLHttpRequest();
  xhr.open('POST','/remotecontrol/setregion',true);
  xhr.setRequestHeader('Content-Type','application/json');
  xhr.send(JSON.stringify(d));
}
</script>
</body>
</html>";
        }
    }
}
