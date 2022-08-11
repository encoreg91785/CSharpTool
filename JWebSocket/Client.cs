using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JLibrary.JWebSocket
{
    /// <summary>
    /// 連線到Server
    /// 必須繼承使用
    /// </summary>
    public abstract class Client
    {
        /// <summary>
        /// 最大接收size
        /// </summary>
        public int receiveChunkSize = 512;
        /// <summary>
        /// 連線的本體
        /// </summary>
        ClientWebSocket ws;
        /// <summary>
        /// 接受服務器資訊
        /// </summary>
        /// <param name="buffer"></param>
        public abstract void OnMessage(byte[] buffer);
        /// <summary>
        /// 關閉時
        /// </summary>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        public abstract void OnClose(WebSocketCloseStatus? status, string msg);
        /// <summary>
        /// 錯誤時
        /// </summary>
        /// <param name="msg"></param>
        public abstract void OnError(string msg);
        /// <summary>
        /// 連線
        /// EX: ws://host:port
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task Connect(string uri)
        {
            try
            {
                ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(uri), CancellationToken.None);
                Receive();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }
        /// <summary>
        /// 接收訊息
        /// </summary>
        async void Receive()
        {
            WebSocketReceiveResult receiveResult = null;
            byte[] receiveBuffer = new byte[receiveChunkSize];
            try
            {
                 while (ws.State == WebSocketState.Open)
                {
                    using (var ms = new MemoryStream())
                    {
                        int currentLength = 0;
                        do
                        {
                            receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                            if (receiveResult.MessageType == WebSocketMessageType.Close)
                            {
                                if (ws.State == WebSocketState.Open) await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                break;
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
                if (ws != null)
                {
                    Console.WriteLine("Client State: {0}", ws.State);
                    if (ws.State != WebSocketState.CloseReceived) ws.Dispose();
                    OnClose(receiveResult.CloseStatus, receiveResult.CloseStatusDescription);
                }
            }
        }
        /// <summary>
        /// 發送訊息到服務器
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task<bool> Send(byte[] buffer)
        {
            if (ws.State != WebSocketState.Open) return false;
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
            return true;
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
    }
}
