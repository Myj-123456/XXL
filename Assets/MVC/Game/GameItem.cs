using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;
public class GameItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //记录可以随机的颜色
    Color[] stringArr = new Color[6]
    {
        Color.red, Color.green, Color.blue,Color.gray,Color.cyan,Color.black
    };
    //展示物品类型
    Image image;
    //当前颜色
    public Color color;
    public Color _color
    {
        get => color;
        set
        {
            color = value;
            GetComponent<Image>().color = value;
        }
    }
    //是否在拖拽
    bool isDrag;
    RectTransform rectTransform;
    GameView gameView;
    Transform parent;

    //记录网格中的位置
    public int row;
    public int col;
    public void Init(GameView gameView, int row, int col)
    {
        Random random = new Random();
        image = transform.GetChild(0).GetComponent<Image>();
        rectTransform = transform.GetComponent<RectTransform>();
        parent = transform.parent;
        this.gameView = gameView;
        this.row = row;
        this.col = col;
        ChangeColor(random);
    }

    private void ChangeColor(Random random)
    {
        //随即物品
        color = stringArr[random.Next(0, 6)];
        image.color = color;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        //设置bool值
        isDrag = true;
        transform.SetParent(gameView.uiBase.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //跟随鼠标
        if (isDrag)
        {
            //获取屏幕鼠标位置
            Vector3 mousePosition = Input.mousePosition;
            //将鼠标的屏幕坐标转换为视口坐标
            Vector3 viewportPosition = Camera.main.ScreenToViewportPoint(mousePosition);
            //将视口坐标转换为世界坐标
            Vector3 worldPosition = Camera.main.ViewportToWorldPoint(viewportPosition); 
            //只设置x,y
            worldPosition.z = 0;
            //更新recttransform位置
            rectTransform.position = worldPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //关闭bool值
        isDrag = false;
        //获取拖拽方向
        Vector3 dragDelta = eventData.position - eventData.pressPosition;
        //判断拖拽方向
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
        {
            //水平方向拖拽
            if (dragDelta.x > 0 && col < 7) //向右
            {
                gameView.TrySwapItems(this, gameView.GetItem(row, col + 1));
            }
            else if (dragDelta.x < 0 && col > 0) //向左
            {
                gameView.TrySwapItems(this, gameView.GetItem(row, col - 1));
            }
        }
        else
        {
            //垂直方向拖拽
            if (dragDelta.y > 0 && row > 0)//向上
            {
                gameView.TrySwapItems(this, gameView.GetItem(row - 1, col));
            }
            else if (dragDelta.y < 0 && row < 7) //向下
            {
                gameView.TrySwapItems(this, gameView.GetItem(row + 1, col));
            }
        }
        //复位
        transform.SetParent(parent);
        rectTransform.anchoredPosition = Vector2.zero;
        //判断上下左右距离是否可以交换
    }
    public void MoveToNewPosition(int newRow, int newCol)
    {
        row = newRow;
        col = newCol;
        parent = gameView.GetPositionTransform(newRow, newCol);
        transform.SetParent(parent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
    public bool IsChange(GameItem item)
    {

        return false;
    }
}
