using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;
public class GameItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //��¼�����������ɫ
    Color[] stringArr = new Color[6]
    {
        Color.red, Color.green, Color.blue,Color.gray,Color.cyan,Color.black
    };
    //չʾ��Ʒ����
    Image image;
    //��ǰ��ɫ
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
    //�Ƿ�����ק
    bool isDrag;
    RectTransform rectTransform;
    GameView gameView;
    Transform parent;

    //��¼�����е�λ��
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
        //�漴��Ʒ
        color = stringArr[random.Next(0, 6)];
        image.color = color;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        //����boolֵ
        isDrag = true;
        transform.SetParent(gameView.uiBase.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //�������
        if (isDrag)
        {
            //��ȡ��Ļ���λ��
            Vector3 mousePosition = Input.mousePosition;
            //��������Ļ����ת��Ϊ�ӿ�����
            Vector3 viewportPosition = Camera.main.ScreenToViewportPoint(mousePosition);
            //���ӿ�����ת��Ϊ��������
            Vector3 worldPosition = Camera.main.ViewportToWorldPoint(viewportPosition); 
            //ֻ����x,y
            worldPosition.z = 0;
            //����recttransformλ��
            rectTransform.position = worldPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //�ر�boolֵ
        isDrag = false;
        //��ȡ��ק����
        Vector3 dragDelta = eventData.position - eventData.pressPosition;
        //�ж���ק����
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
        {
            //ˮƽ������ק
            if (dragDelta.x > 0 && col < 7) //����
            {
                gameView.TrySwapItems(this, gameView.GetItem(row, col + 1));
            }
            else if (dragDelta.x < 0 && col > 0) //����
            {
                gameView.TrySwapItems(this, gameView.GetItem(row, col - 1));
            }
        }
        else
        {
            //��ֱ������ק
            if (dragDelta.y > 0 && row > 0)//����
            {
                gameView.TrySwapItems(this, gameView.GetItem(row - 1, col));
            }
            else if (dragDelta.y < 0 && row < 7) //����
            {
                gameView.TrySwapItems(this, gameView.GetItem(row + 1, col));
            }
        }
        //��λ
        transform.SetParent(parent);
        rectTransform.anchoredPosition = Vector2.zero;
        //�ж��������Ҿ����Ƿ���Խ���
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
