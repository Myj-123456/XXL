using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameView : ViewBase
{
    const int rows = 8;
    const int cols = 9;

    Transform itemParent;
    public List<GameItem> itemobjs;

    GameItem[,] itemGrid = new GameItem[rows, cols];

    //用于协程的MonoBehaviour引用
    private MonoBehaviour monoHelper;
    public override void Init(UIWindow uiBase)
    {
        base.Init(uiBase);
        //获取MonoBehaviour
        monoHelper = uiBase.GetComponent<MonoBehaviour>();
        itemParent = uiBase.transform.Find("ItemParent");
        itemobjs = new List<GameItem>();
        InstantiateAllItems();
    }

    private void InstantiateAllItems()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var item = GameObject.Instantiate<GameObject>(ResourceMgr.Instance.ResLoadAsset<GameObject>("Type"), GetPositionTransform(row, col));
                var gameItem = item.GetComponent<GameItem>();
                gameItem.Init(this, row, col);
                itemobjs.Add(gameItem);
                itemGrid[row, col] = gameItem;
            }
        }
        //初始检查是否有匹配
        monoHelper.StartCoroutine(CheckMathesAfterDelay(0.1f));
    }
    //获取指定位置的transform
    public Transform GetPositionTransform(int row, int col)
    {
        //计算子对象索引
        int index = row * cols + col;
        return itemParent.GetChild(index);
    }
    //获取指定位置的物品
    public GameItem GetItem(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < cols)
        {
            return itemGrid[row, col];
        }
        return null;
    }
    //尝试交换两个物品
    private bool isSwapping = false;
    public void TrySwapItems(GameItem item1, GameItem item2)
    {
        if (item1 == null || item2 == null || isSwapping) return;
        if (Mathf.Abs(item1.row - item2.row) + Mathf.Abs(item1.col - item2.col) != 1)
        {
            Debug.Log("只能交换相邻的物品");
            return;
        }
        isSwapping = true;
        //记录原始位置
        int originalRow1 = item1.row;
        int originalCol1 = item1.col;
        int originalRow2 = item2.row;
        int originalCol2 = item2.col;
        //交换网格中的位置
        itemGrid[originalRow1, originalCol1] = item2;
        itemGrid[originalRow2, originalCol2] = item1;
        //交换行列信息
        item1.row = originalRow2;
        item1.col = originalCol2;
        item2.row = originalRow1;
        item2.col = originalCol1;
        ////移动物品到新位置
        item1.MoveToNewPosition(item1.row, item1.col);
        item2.MoveToNewPosition(item2.row, item2.col);
        //检查是否有匹配
        monoHelper.StartCoroutine(CheckMathesAfterSwap(item1, item2, originalRow1, originalCol1, originalRow2, originalCol2));
    }
    //交换后检查匹配
    private IEnumerator CheckMathesAfterSwap(GameItem item1, GameItem item2, int originalRow1, int originalCol1, int originalRow2, int originalCol2)
    {
        yield return new WaitForSeconds(0.3f);

        List<GameItem> matchedItems = new List<GameItem>();
        matchedItems.AddRange(FindMatches(item1.row, item1.col));
        matchedItems.AddRange(FindMatches(item2.row, item2.col));
        if (matchedItems.Count >= 3)
        {
            //有匹配，消除物品
            RemoveMatchedItems(matchedItems);
            yield return new WaitForSeconds(0.5f);
            //下落新物品
            FillEmptySpaces();
        }
        else
        {
            //没有匹配，交换回来
            //恢复网格数据
            itemGrid[originalRow1, originalCol1] = item1;
            itemGrid[originalRow2, originalCol2] = item2;
            //恢复原始行列
            item1.row = originalRow1;
            item1.col = originalCol1;
            //这里是originalRow1，因为item2现在item1的原始位置
            item2.row = originalRow2;
            item2.col = originalCol2;
            ////移动物品到新位置
            item1.MoveToNewPosition(item1.row, item1.col);
            item2.MoveToNewPosition(item2.row, item2.col);
        }
        isSwapping = false;
    }
    //查找匹配的物品
    private List<GameItem> FindMatches(int row, int col)
    {
        List<GameItem> matches = new List<GameItem>();
        GameItem originalItem = GetItem(row, col);
        if (originalItem == null)
        {
            return matches;
        }
        //检查水平方向
        List<GameItem> horizontalMatches = new List<GameItem>();
        horizontalMatches.Add(originalItem);
        //向左检查
        for (int i = col - 1; i >= 0; i--)
        {
            GameItem item = GetItem(row, i);
            if (item != null && item.color == originalItem.color)
            {
                horizontalMatches.Add(item);
            }
            else
            {
                break;
            }
        }
        //向右检查
        for (int i = col + 1; i < cols; i++)
        {
            GameItem item = GetItem(row, i);
            if (item != null && item.color == originalItem.color)
            {
                horizontalMatches.Add(item);
            }
            else
            {
                break;
            }
        }
        //如果水平方向有3个或以上匹配
        if (horizontalMatches.Count >= 3)
        {
            matches.AddRange(horizontalMatches);
        }
        //检查垂直方向
        List<GameItem> verticalMatches = new List<GameItem>();
        verticalMatches.Add(originalItem);
        //向上检查
        for (int i = row - 1; i >= 0; i--)
        {
            GameItem item = GetItem(i, col);
            if (item != null && item.color == originalItem.color)
            {
                verticalMatches.Add(item);
            }
            else
            {
                break;
            }
        }
        //向下检查
        for (int i = row + 1; i < rows; i++)
        {
            GameItem item = GetItem(i, col);
            if (item != null && item.color == originalItem.color)
            {
                verticalMatches.Add(item);
            }
            else
            {
                break;
            }
        }
        //如果垂直方向有3个或以上匹配
        if (verticalMatches.Count >= 3)
        {
            matches.AddRange(verticalMatches);
        }
        return matches;
    }
    //移除匹配的物品
    private void RemoveMatchedItems(List<GameItem> matcheditems)
    {
        foreach (GameItem item in matcheditems)
        {
            if (item != null && itemobjs.Contains(item))
            {
                itemGrid[item.row, item.col] = null;
                itemobjs.Remove(item);
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
    }
    private void FillEmptySpaces()
    {
        //下落现有物品
        for (int col = 0; col < cols; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (itemGrid[row, col] == null)
                {
                    //寻找上方第一个非空物品
                    for (int aboveRow = row - 1; aboveRow >= 0; aboveRow--)
                    {
                        if (itemGrid[aboveRow, col] != null)
                        {
                            //下落物品
                            itemGrid[row, col] = itemGrid[aboveRow, col];
                            itemGrid[aboveRow, col] = null;
                            itemGrid[row, col].MoveToNewPosition(row, col);
                            break;

                        }
                    }
                }
            }
        }
        //创建新物品填充顶部空白
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (itemGrid[row, col] == null)
                {
                    CreateNewItem(row, col);
                }
            }
        }
        //再次检查是否匹配
        monoHelper.StartCoroutine(CheckMathesAfterDelay(0.5f));
    }
    //创建新物品
    private void CreateNewItem(int row, int col)
    {
        var item = GameObject.Instantiate<GameObject>(ResourceMgr.Instance.ResLoadAsset<GameObject>("Type"), GetPositionTransform(row, col));
        var gameItem = item.GetComponent<GameItem>();
        gameItem.Init(this, row, col);
        itemobjs.Add(gameItem);
        itemGrid[row, col] = gameItem;
    }
    //延迟检查匹配
    private IEnumerator CheckMathesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        List<GameItem> allMathes = new List<GameItem>();
        //检查整个网格的匹配
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                List<GameItem> matches = FindMatches(row, col);
                if (matches.Count >= 3)
                {
                    foreach (GameItem item in matches)
                    {
                        if (!allMathes.Contains(item))
                        {
                            allMathes.Add(item);
                        }
                    }
                }
            }
        }
        if (allMathes.Count >= 3)
        {
            RemoveMatchedItems(allMathes);
            yield return new WaitForSeconds(0.5f);
            FillEmptySpaces();
        }
    }
}
