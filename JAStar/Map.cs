using System;
using System.Collections.Generic;
using System.Text;

namespace JLibrary.JAStar
{
    /// <summary>
    /// 給使用者繼承
    /// 並添加地圖所需要的參數
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Map<T>  where T : PathNode
    {
        /// <summary>
        /// 尚未較驗過的點
        /// </summary>
        private List<T> openList = new List<T>();
        /// <summary>
        /// 以較驗過的點
        /// </summary>
        private List<T> closeList = new List<T>();
        /// <summary>
        /// 開始尋路
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Stack<T> FindPath(T start, T end)
        {
            Rest();
            openList.Add(start);
            start.gCost = 0;
            start.hCost = CalculateDistanceCost(start, end);
            while (openList.Count > 0)
            {
                T currentNode = GetLowestFCosetNode(openList);
                if (currentNode == end)
                {
                    // 最後抵達終點
                    return CalculatePath(end);
                }
                // 當前檢查的節點 移除Open 放入Close
                openList.Remove(currentNode);
                closeList.Add(currentNode);

                var neighbourNodes = GetNeighbourList(currentNode);
                for (int i = 0; i < neighbourNodes.Count; i++)
                {
                    var neighbour = neighbourNodes[i];
                    // 已被檢驗過 不計算
                    if (closeList.Contains(neighbour)) continue;
                    //不可走過 就不計算
                    if (!neighbour.isWalkable)
                    {
                        closeList.Add(neighbour);
                        continue;
                    }
                    // tentative 暫定成本
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbour);
                    //將當前最小GCost放入Open內計算
                    if (tentativeGCost < neighbour.gCost)
                    {
                        neighbour.parentNode = currentNode;
                        neighbour.gCost = tentativeGCost;
                        neighbour.hCost = CalculateDistanceCost(neighbour, end);
                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 實作
        /// 取得目前節點的所有相鄰節點
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract List<T> GetNeighbourList(T node);
        /// <summary>
        /// 實作
        /// 計算兩點距離
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        protected abstract int CalculateDistanceCost(T start, T end);
        /// <summary>
        /// 重置使用過的點
        /// 並清理openList closeList
        /// </summary>
        void Rest()
        {
            closeList.ForEach(n => n.Rest());
            openList.ForEach(n => n.Rest());
            closeList.Clear();
            openList.Clear();
        }
        /// <summary>
        /// 結算路徑
        /// 從終點利用parentNode往回找找到起點
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        Stack<T> CalculatePath(T end)
        {
            Stack<T> path = new Stack<T>();
            path.Push(end);
            PathNode currentNode = end;
            while (currentNode.parentNode != null)
            {
                path.Push((T)currentNode.parentNode);
                currentNode = currentNode.parentNode;
            }
            return path;
        }
        /// <summary>
        /// 取得最少FCost的節點
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        T GetLowestFCosetNode(List<T> nodeList)
        {
            T lowestFCosetNode = nodeList[0];
            for (int i = 1; i < nodeList.Count; i++)
            {
                if (nodeList[i].FCost < lowestFCosetNode.FCost)
                {
                    lowestFCosetNode = nodeList[i];
                }
            }
            return lowestFCosetNode;
        }
    }
}
