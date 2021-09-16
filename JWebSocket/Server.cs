using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JLibrary.JWebSocket
{
    /// <summary>
    /// 定義Server
    /// 需繼承使用
    /// ((UNITY))無法使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Server<T> where T : Socket, new()
    {
        /// <summary>
        /// 監聽
        /// </summary>
        HttpListener listener;
        readonly Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();
        /// <summary>
        /// 是否正在運行
        /// </summary>
        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// 最大收取size
        /// </summary>
        public int maxReceiveBuffer = 1024;
        /// <summary>
        /// 用ID取socket實體
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Socket GetWebSocketById(string id)
        {
            if (sockets.TryGetValue(id, out Socket ws)) return ws;
            else return null;
        }
        /// <summary>
        /// 獲取唯一碼
        /// </summary>
        /// <returns></returns>
        public virtual Task<string> GainSocketID()
        {
            var t = new TaskCompletionSource<string>();
            t.SetResult(Guid.NewGuid().ToString());
            return t.Task;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            listener?.Abort();
        }
        /// <summary>
        /// 啟動
        /// EX: http://localhost:12321/
        /// </summary>
        /// <param name="listenerPrefix"></param>
        public async void Start(string listenerPrefix)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
            IsRunning = true;
            while (IsRunning)
            {

                HttpListenerContext listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(listenerContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }
        /// <summary>
        /// 接收訊息
        /// </summary>
        /// <param name="listenerContext"></param>
        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            }
            catch (Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                var msg = string.Format("StackTrace : {0}\nMessage : {1}", e.StackTrace, e.Message);
                OnError(msg);
                return;
            }
            Socket s = new T() { ws = webSocketContext.WebSocket, id = await GainSocketID() };
            sockets.Add(s.id, s);
            OnOpen(s);
            await s.Receive(); // 持續查看是否有訊息
            sockets.Remove(s.id);
        }
        /// <summary>
        /// 連上線的Socket
        /// </summary>
        /// <param name="ws"></param>
        public abstract void OnOpen(Socket ws);
        /// <summary>
        /// 連線失敗時
        /// </summary>
        /// <param name="msg"></param>
        public abstract void OnError(string msg);
    }
}
