using System;
using System.Collections.Generic;
using System.Text;

namespace JLibrary.JAStar
{
    /// <summary>
    /// 節點資訊
    /// EX: 權重,X,Y,Z,長,寬,高......等等資訊 
    /// </summary>
    public abstract class PathNode
    {
        /// <summary>
        /// 父節點
        /// </summary>
        public PathNode parentNode;
        /// <summary>
        /// 可不可以走過
        /// </summary>
        public bool isWalkable = true; 
        /// <summary>
        /// 當前點到下個點移動的代價
        /// </summary>
        public int gCost = int.MaxValue;
        /// <summary>
        /// 此點至終點的代價(暫時忽略路徑上的所有障礙)
        /// </summary>
        public int hCost = 0;
        /// <summary>
        /// gCost + hCost 總代價
        /// </summary>
        public int FCost { get { return gCost + hCost; } }
        /// <summary>
        /// 回復初始值
        /// </summary>
        public void Rest()
        {
            parentNode = null;
            hCost = 0;
            gCost = int.MaxValue;
        }
    }
}
