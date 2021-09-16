using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JLibrary.JWorkQueue
{
    /// <summary>
    /// Work排程
    /// 每個工作完成後在執行下一個Work
    /// </summary>
    sealed public class WorkQueue
    {
        /// <summary>
        /// 中止時不等待當前Work完成
        /// </summary>
        TaskCompletionSource<bool> abortTask = new TaskCompletionSource<bool>();
        /// <summary>
        /// 所有Work
        /// </summary>
        Queue<Work> works = new Queue<Work>();
        /// <summary>
        /// 當前Work
        /// </summary>
        public Work CurrentWork { get; private set; }
        /// <summary>
        /// 是否正在運行
        /// </summary>
        public bool IsWorking { get; private set; } = false;
        /// <summary>
        /// 當前訊息 Work開始-中止-結束 Error
        /// </summary>
        public Action<string> OnMessage = (s) => { };

        /// <summary>
        /// 開始運行
        /// </summary>
        public async void Start()
        {
            // 運行中部重覆運行
            IsWorking = true;
            string msg = "WorkQueue Start";
            OnMessage(msg);
            while (IsWorking)
            {
                if (works.Count > 0)
                {
                    CurrentWork = works.Dequeue();
                    msg = string.Format("Work {0} Start", CurrentWork.GetType().ToString());
                    OnMessage(msg);
                    if (!CurrentWork.IsAbort && !CurrentWork.IsDone)
                    {
                        try
                        {
                            await Task.WhenAny(CurrentWork.Do(), abortTask.Task);
                            if (CurrentWork.IsAbort)
                            {
                                RestAbort();
                                CurrentWork.OnAbort();
                            }
                        }
                        catch (Exception e)
                        {
                            msg = string.Format("Work {0} ErrorMessage : {1} \n StackTrace: {2}", CurrentWork.GetType().ToString(), e.Message, e.StackTrace);
                            OnMessage(msg);
                            RestAbort();
                            CurrentWork.OnAbort();
                        }
                    }
                    msg = string.Format("Work {0} End", CurrentWork.GetType().ToString());
                    OnMessage(msg);
                    CurrentWork.IsDone = true;
                    CurrentWork = null;
                }
                await Task.Yield();
            }
        }


        void RestAbort()
        {
            if (abortTask == null || abortTask.Task.IsCompleted)
            {
                abortTask = new TaskCompletionSource<bool>();
            }
        }
        /// <summary>
        /// 新增工作
        /// </summary>
        /// <param name="w"></param>
        public void AddWork(Work w)
        {
            works.Enqueue(w);
        }

        /// <summary>
        /// 停止並中斷進行中的work
        /// </summary>
        public void Stop()
        {
            OnMessage("WorkQueue Stop");
            IsWorking = false;
            if (CurrentWork != null)
            {
                CurrentWork.IsAbort = true;
                abortTask.SetResult(true);
            }
        }

        /// <summary>
        /// 清除所有work並中斷進行中的work
        /// </summary>
        public void Clear()
        {
            OnMessage("WorkQueue Clear");
            works.Clear();
            Stop();
        }
    }
}

