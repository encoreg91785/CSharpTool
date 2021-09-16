using System.Threading.Tasks;

namespace JLibrary.JWorkQueue
{
    /// <summary>
    /// 放入WorkQueue內使用
    /// </summary>
    public abstract class Work
    {
        /// <summary>
        /// 是否已經完成
        /// </summary>
        public bool IsDone { get; internal set; }
        /// <summary>
        /// 是否被中止
        /// </summary>
        public bool IsAbort { get; internal set; }
        /// <summary>
        /// Work執行的內容
        /// </summary>
        /// <returns></returns>
        public abstract Task Do();
        /// <summary>
        /// 觸發中止時
        /// Work自行處理中止時需要的清理
        /// </summary>
        public abstract void OnAbort();
    }
}
