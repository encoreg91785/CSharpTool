using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace JLibrary.JWebSocket
{
    /// <summary>
    /// 定義Server回傳的Socket
    /// 必須繼承使用
    /// </summary>
    public abstract class Socket
    {
        /// <summary>
        /// 最大收取size
        /// </summary>
        public int receiveChunkSize = 512;
        internal WebSocket ws;
        /// <summary>
        /// 發生錯誤時
        /// </summary>
        public abstract void OnError(string msg);
        /// <summary>
        /// 接受訊息時
        /// </summary>
        public abstract void OnMessage(byte[] buffer);
        /// <summary>
        /// 關閉連線時
        /// </summary>
        public abstract void OnClose(WebSocketCloseStatus? status, string msg);
        /// <summary>
        /// 唯一碼
        /// </summary>
        public string id { get; internal set; }
        /// <summary>
        /// socket 當前狀態
        /// </summary>
        /// <returns></returns>
        public WebSocketState State { get { return ws.State; } }
        /// <summary>
        /// 發送訊息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task<bool> Send(byte[] buffer)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
                return true;
            }
            else return false;
        }
        /// <summary>
        /// 關閉連線
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task Close(string msg = "")
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
        }
        /// <summary>
        /// 接收訊息
        /// </summary>
        /// <returns></returns>
        internal async Task Receive()
        {
            WebSocketReceiveResult receiveResult = null;
            byte[] receiveBuffer = new byte[receiveChunkSize];
            try
            {
                using (var ms = new MemoryStream())
                {
                    while (ws.State == WebSocketState.Open)
                    {
                        int currentLength = 0;
                        do
                        {
                            receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                            if (receiveResult.MessageType == WebSocketMessageType.Close)
                            {
                                if (ws.State == WebSocketState.Open) await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                            }
                            else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                            {
                                currentLength += receiveResult.Count;
                                if (ms.Length < currentLength) ms.SetLength(currentLength);
                                ms.Write(receiveBuffer, 0, receiveResult.Count);
                            }
                            else
                            {
                                await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                            }
                        }
                        while (!receiveResult.EndOfMessage);
                        if (ws.State != WebSocketState.Open) break;
                        ms.Seek(0, SeekOrigin.Begin);
                        OnMessage(ms.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                var msg = string.Format("StackTrace : {0} \nMessage : {1}", e.StackTrace, e.Message);
                OnError(msg);
            }
            finally
            {
                var msg = string.Format("Description : {0}", receiveResult.CloseStatusDescription);
                OnClose(receiveResult.CloseStatus, msg);
                if(ws.State != WebSocketState.CloseReceived) ws.Dispose();
            }
        }
    }
}
