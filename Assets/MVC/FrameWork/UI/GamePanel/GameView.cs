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

    //����Э�̵�MonoBehaviour����
    private MonoBehaviour monoHelper;
    public override void Init(UIWindow uiBase)
    {
        base.Init(uiBase);
        //��ȡMonoBehaviour
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
        //��ʼ����Ƿ���ƥ��
        monoHelper.StartCoroutine(CheckMathesAfterDelay(0.1f));
    }
    //��ȡָ��λ�õ�transform
    public Transform GetPositionTransform(int row, int col)
    {
        //�����Ӷ�������
        int index = row * cols + col;
        return itemParent.GetChild(index);
    }
    //��ȡָ��λ�õ���Ʒ
    public GameItem GetItem(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < cols)
        {
            return itemGrid[row, col];
        }
        return null;
    }
    //���Խ���������Ʒ
    private bool isSwapping = false;
    public void TrySwapItems(GameItem item1, GameItem item2)
    {
        if (item1 == null || item2 == null || isSwapping) return;
        if (Mathf.Abs(item1.row - item2.row) + Mathf.Abs(item1.col - item2.col) != 1)
        {
            Debug.Log("ֻ�ܽ������ڵ���Ʒ");
            return;
        }
        isSwapping = true;
        //��¼ԭʼλ��
        int originalRow1 = item1.row;
        int originalCol1 = item1.col;
        int originalRow2 = item2.row;
        int originalCol2 = item2.col;
        //���������е�λ��
        itemGrid[originalRow1, originalCol1] = item2;
        itemGrid[originalRow2, originalCol2] = item1;
        //����������Ϣ
        item1.row = originalRow2;
        item1.col = originalCol2;
        item2.row = originalRow1;
        item2.col = originalCol1;
        ////�ƶ���Ʒ����λ��
        item1.MoveToNewPosition(item1.row, item1.col);
        item2.MoveToNewPosition(item2.row, item2.col);
        //����Ƿ���ƥ��
        monoHelper.StartCoroutine(CheckMathesAfterSwap(item1, item2, originalRow1, originalCol1, originalRow2, originalCol2));
    }
    //��������ƥ��
    private IEnumerator CheckMathesAfterSwap(GameItem item1, GameItem item2, int originalRow1, int originalCol1, int originalRow2, int originalCol2)
    {
        yield return new WaitForSeconds(0.3f);

        List<GameItem> matchedItems = new List<GameItem>();
        matchedItems.AddRange(FindMatches(item1.row, item1.col));
        matchedItems.AddRange(FindMatches(item2.row, item2.col));
        if (matchedItems.Count >= 3)
        {
            //��ƥ�䣬������Ʒ
            RemoveMatchedItems(matchedItems);
            yield return new WaitForSeconds(0.5f);
            //��������Ʒ
            FillEmptySpaces();
        }
        else
        {
            //û��ƥ�䣬��������
            //�ָ���������
            itemGrid[originalRow1, originalCol1] = item1;
            itemGrid[originalRow2, originalCol2] = item2;
            //�ָ�ԭʼ����
            item1.row = originalRow1;
            item1.col = originalCol1;
            //������originalRow1����Ϊitem2����item1��ԭʼλ��
            item2.row = originalRow2;
            item2.col = originalCol2;
            ////�ƶ���Ʒ����λ��
            item1.MoveToNewPosition(item1.row, item1.col);
            item2.MoveToNewPosition(item2.row, item2.col);
        }
        isSwapping = false;
    }
    //����ƥ�����Ʒ
    private List<GameItem> FindMatches(int row, int col)
    {
        List<GameItem> matches = new List<GameItem>();
        GameItem originalItem = GetItem(row, col);
        if (originalItem == null)
        {
            return matches;
        }
        //���ˮƽ����
        List<GameItem> horizontalMatches = new List<GameItem>();
        horizontalMatches.Add(originalItem);
        //������
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
        //���Ҽ��
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
        //���ˮƽ������3��������ƥ��
        if (horizontalMatches.Count >= 3)
        {
            matches.AddRange(horizontalMatches);
        }
        //��鴹ֱ����
        List<GameItem> verticalMatches = new List<GameItem>();
        verticalMatches.Add(originalItem);
        //���ϼ��
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
        //���¼��
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
        //�����ֱ������3��������ƥ��
        if (verticalMatches.Count >= 3)
        {
            matches.AddRange(verticalMatches);
        }
        return matches;
    }
    //�Ƴ�ƥ�����Ʒ
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
        //����������Ʒ
        for (int col = 0; col < cols; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (itemGrid[row, col] == null)
                {
                    //Ѱ���Ϸ���һ���ǿ���Ʒ
                    for (int aboveRow = row - 1; aboveRow >= 0; aboveRow--)
                    {
                        if (itemGrid[aboveRow, col] != null)
                        {
                            //������Ʒ
                            itemGrid[row, col] = itemGrid[aboveRow, col];
                            itemGrid[aboveRow, col] = null;
                            itemGrid[row, col].MoveToNewPosition(row, col);
                            break;

                        }
                    }
                }
            }
        }
        //��������Ʒ��䶥���հ�
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
        //�ٴμ���Ƿ�ƥ��
        monoHelper.StartCoroutine(CheckMathesAfterDelay(0.5f));
    }
    //��������Ʒ
    private void CreateNewItem(int row, int col)
    {
        var item = GameObject.Instantiate<GameObject>(ResourceMgr.Instance.ResLoadAsset<GameObject>("Type"), GetPositionTransform(row, col));
        var gameItem = item.GetComponent<GameItem>();
        gameItem.Init(this, row, col);
        itemobjs.Add(gameItem);
        itemGrid[row, col] = gameItem;
    }
    //�ӳټ��ƥ��
    private IEnumerator CheckMathesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        List<GameItem> allMathes = new List<GameItem>();
        //������������ƥ��
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
