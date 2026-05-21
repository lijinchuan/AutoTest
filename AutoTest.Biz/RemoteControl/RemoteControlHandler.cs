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
                        var frame = ScreenCaptureService.Instance.CaptureJpegFrame();
                        var bytes = frame.Bytes;
                        var metrics = frame.Metrics;
                        response.ContentType = "image/jpeg";
                        response.Header["Cache-Control"] = "no-store, no-cache";
                        response.Header["Pragma"] = "no-cache";
                        response.Header["Server-Timing"] = string.Format("capture;dur={0},overlay;dur={1},encode;dur={2},server;dur={3}", metrics.CaptureMs, metrics.OverlayMs, metrics.EncodeMs, metrics.TotalMs);
                        response.Header["X-Capture-Ms"] = metrics.CaptureMs.ToString();
                        response.Header["X-Overlay-Ms"] = metrics.OverlayMs.ToString();
                        response.Header["X-Encode-Ms"] = metrics.EncodeMs.ToString();
                        response.Header["X-Server-Ms"] = metrics.TotalMs.ToString();
                        response.Header["X-Frame-Bytes"] = metrics.ByteSize.ToString();
                        response.Header["X-Frame-Size"] = string.Format("{0}x{1}", metrics.Width, metrics.Height);
                        response.RawContent = bytes;
                        return true;

                    case "mousedown":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        var hwnd = ScreenCaptureService.Instance.CurrentWindowHandle;
                        Win32MouseSimulator.LeftDownOnWindow(hwnd, pt.X, pt.Y);
                        WriteOk(response);
                        return true;
                    }

                    case "mousemove":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        var hwnd = ScreenCaptureService.Instance.CurrentWindowHandle;
                        Win32MouseSimulator.MoveToOnWindow(hwnd, pt.X, pt.Y);
                        WriteOk(response);
                        return true;
                    }

                    case "mouseup":
                    {
                        var p = ParseMouseRequest(request);
                        var pt = Win32MouseSimulator.NormalizedToScreen(p.x, p.y, ScreenCaptureService.Instance.CaptureRegion);
                        var hwnd = ScreenCaptureService.Instance.CurrentWindowHandle;
                        Win32MouseSimulator.LeftUpOnWindow(hwnd, pt.X, pt.Y);
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
<meta name='viewport' content='width=device-width,initial-scale=1.0,maximum-scale=3.0,user-scalable=yes'>
<title>远程控制</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{background:#1a1a1a;display:flex;flex-direction:column;align-items:center;justify-content:center;min-height:100vh;font-family:monospace}
#info{color:#aaa;font-size:13px;margin-bottom:8px}
#wrap{position:relative;cursor:crosshair;border:2px solid #555;display:inline-block;max-width:100vw;touch-action:none}
#screen{display:block;max-width:100%;height:auto;-webkit-user-select:none;user-select:none;-webkit-touch-callout:none;touch-action:none}
#status{color:#4caf50;font-size:12px;margin-top:6px}
#cfg{margin-top:12px;color:#888;font-size:12px;display:flex;gap:8px;flex-wrap:wrap;justify-content:center}
#cfg input{width:60px;padding:2px 4px;background:#333;border:1px solid #555;color:#ccc;font-size:12px}
#cfg button{padding:2px 8px;background:#444;border:1px solid #666;color:#ccc;cursor:pointer;font-size:12px}
</style>
</head>
<body>
<div id='info'>实时远程控制 &nbsp;|&nbsp; 单指拖动操作，双指捏合缩放</div>
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
      isDown=false,pending=null,moving=false,
      inflight=false,currentUrl='',
      pollDelay=60,minDelay=40,maxDelay=300,
      lastFrameDone=0,
      zoomScale=1,minZoom=1,maxZoom=3,
      panX=0,panY=0,panStartX=0,panStartY=0,startPanX=0,startPanY=0,isPanning=false,panMoved=false,tapMaxMove=8,
      pinchStartDist=0,pinchStartScale=1,isPinching=false,
      metricAvg={server:0,capture:0,overlay:0,encode:0,network:0,decode:0,total:0,bytes:0,fps:0,count:0};

  function toNum(v){var n=parseFloat(v);return isNaN(n)?0:n;}
  function smooth(oldV,newV){return oldV?oldV*0.75+newV*0.25:newV;}
  function updateMetric(frame){
    metricAvg.server=smooth(metricAvg.server,frame.server);
    metricAvg.capture=smooth(metricAvg.capture,frame.capture);
    metricAvg.overlay=smooth(metricAvg.overlay,frame.overlay);
    metricAvg.encode=smooth(metricAvg.encode,frame.encode);
    metricAvg.network=smooth(metricAvg.network,frame.network);
    metricAvg.decode=smooth(metricAvg.decode,frame.decode);
    metricAvg.total=smooth(metricAvg.total,frame.total);
    metricAvg.bytes=smooth(metricAvg.bytes,frame.bytes);
    metricAvg.count++;

    if(lastFrameDone>0){
      var fps=1000/Math.max(1,frame.end-lastFrameDone);
      metricAvg.fps=smooth(metricAvg.fps,fps);
    }
    lastFrameDone=frame.end;
  }

  function renderStatus(frameSize){
    status.textContent='平均 '+metricAvg.fps.toFixed(1)+'fps | 总'+metricAvg.total.toFixed(0)+'ms = 服'+metricAvg.server.toFixed(0)+' + 网'+metricAvg.network.toFixed(0)+' + 解渲'+metricAvg.decode.toFixed(0)+' | 捕'+metricAvg.capture.toFixed(0)+' 编'+metricAvg.encode.toFixed(0)+' | '+(metricAvg.bytes/1024).toFixed(1)+'KB '+(frameSize||'')+' | 缩放 '+zoomScale.toFixed(2)+'x';
  }

  function applyTransform(){
    img.style.transformOrigin='center center';
    img.style.transform='translate('+panX+'px,'+panY+'px) scale('+zoomScale+')';
  }

  function applyZoom(scale){
    zoomScale=Math.max(minZoom,Math.min(maxZoom,scale));
    if(zoomScale<=1.001){
      panX=0;
      panY=0;
    }
    applyTransform();
  }

  function touchDistance(t0,t1){
    var dx=t0.clientX-t1.clientX;
    var dy=t0.clientY-t1.clientY;
    return Math.sqrt(dx*dx+dy*dy);
  }

  function scheduleNext(delay){
    window.setTimeout(refreshScreen,delay);
  }

  function refreshScreen(){
    if(inflight){return;}
    inflight=true;

    var started=performance.now();
    fetch('/remotecontrol/screenshot?_='+Date.now(),{cache:'no-store'})
      .then(function(res){
        if(!res.ok){throw new Error('HTTP '+res.status);}
        var receiveAt=performance.now();
        var captureMs=toNum(res.headers.get('X-Capture-Ms'));
        var overlayMs=toNum(res.headers.get('X-Overlay-Ms'));
        var encodeMs=toNum(res.headers.get('X-Encode-Ms'));
        var serverMs=toNum(res.headers.get('X-Server-Ms'));
        var frameBytes=toNum(res.headers.get('X-Frame-Bytes'));
        var frameSize=res.headers.get('X-Frame-Size')||'';

        return res.blob().then(function(blob){
          var blobAt=performance.now();
          var nextUrl=URL.createObjectURL(blob);

          return new Promise(function(resolve,reject){
            img.onload=function(){
              var done=performance.now();
              if(currentUrl){URL.revokeObjectURL(currentUrl);}
              currentUrl=nextUrl;
              var networkMs=Math.max(0,receiveAt-started-serverMs);
              var decodeMs=Math.max(0,done-blobAt);
              var totalMs=Math.max(0,done-started);

              updateMetric({
                capture:captureMs,
                overlay:overlayMs,
                encode:encodeMs,
                server:serverMs,
                network:networkMs,
                decode:decodeMs,
                total:totalMs,
                bytes:frameBytes||blob.size,
                end:done
              });
              renderStatus(frameSize);

              pollDelay=Math.max(minDelay,Math.min(maxDelay,Math.round(metricAvg.total*0.25)));
              resolve();
            };
            img.onerror=function(){
              URL.revokeObjectURL(nextUrl);
              reject(new Error('图片解码失败'));
            };
            img.src=nextUrl;
          });
        });
      })
      .catch(function(){
        status.textContent='截图失败，重试中...';
        pollDelay=300;
      })
      .finally(function(){
        inflight=false;
        scheduleNext(pollDelay);
      });
  }
  refreshScreen();

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
    if(e.button!==0)return;
    e.preventDefault();
    isDown=true;
    send('mousedown',norm(e));
  });
  window.addEventListener('mousemove',function(e){
    if(!isDown)return;
    sendMove(norm(e));
  });
  window.addEventListener('mouseup',function(e){
    if(!isDown)return;
    isDown=false;
    send('mouseup',norm(e));
  });

  img.addEventListener('touchstart',function(e){
    if(e.touches&&e.touches.length===2){
      e.preventDefault();
      isPinching=true;
      isPanning=false;
      pinchStartDist=touchDistance(e.touches[0],e.touches[1]);
      pinchStartScale=zoomScale;
      if(isDown){isDown=false;}
      return;
    }
    e.preventDefault();
    if(isPinching)return;
    if(zoomScale>1.001&&e.touches&&e.touches.length===1){
      isPanning=true;
      panMoved=false;
      panStartX=e.touches[0].clientX;
      panStartY=e.touches[0].clientY;
      startPanX=panX;
      startPanY=panY;
      if(isDown){isDown=false;}
      return;
    }
    isDown=true;
    send('mousedown',norm(e));
  },{passive:false});

  window.addEventListener('touchmove',function(e){
    if(isPinching){
      if(e.touches&&e.touches.length===2){
        e.preventDefault();
        var d=touchDistance(e.touches[0],e.touches[1]);
        if(pinchStartDist>0){applyZoom(pinchStartScale*(d/pinchStartDist));}
      }
      return;
    }
    if(isPanning){
      if(e.touches&&e.touches.length===1){
        e.preventDefault();
        var moveX=e.touches[0].clientX-panStartX;
        var moveY=e.touches[0].clientY-panStartY;
        if(!panMoved&&(Math.abs(moveX)>tapMaxMove||Math.abs(moveY)>tapMaxMove)){panMoved=true;}
        if(panMoved){
          panX=startPanX+moveX;
          panY=startPanY+moveY;
          applyTransform();
        }
      }
      return;
    }
    if(!isDown)return;
    e.preventDefault();
    sendMove(norm(e));
  },{passive:false});

  window.addEventListener('touchend',function(e){
    if(isPinching){
      e.preventDefault();
      if(!e.touches||e.touches.length<2){isPinching=false;pinchStartDist=0;}
      return;
    }
    if(isPanning){
      e.preventDefault();
      if(!panMoved){
        var tapPos=normEnd(e);
        send('mousedown',tapPos);
        send('mouseup',tapPos);
      }
      if(!e.touches||e.touches.length===0){isPanning=false;panMoved=false;}
      return;
    }
    if(!isDown)return;
    e.preventDefault();
    isDown=false;
    send('mouseup',normEnd(e));
  },{passive:false});

  window.addEventListener('touchcancel',function(e){
    if(isPinching){
      e.preventDefault();
      isPinching=false;
      pinchStartDist=0;
      return;
    }
    if(isPanning){
      e.preventDefault();
      isPanning=false;
      return;
    }
    if(!isDown)return;
    e.preventDefault();
    isDown=false;
    send('mouseup',normEnd(e));
  },{passive:false});

  window.applyRegion=function(){
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
    pollDelay=50;
  };
})();
</script>
</body>
</html>";
        }
    }
}
