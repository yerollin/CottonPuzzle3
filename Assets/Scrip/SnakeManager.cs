using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using MapInfo = MapSpace.MapElement;//别名缩写，单文件有效

public class SnakeManager : MonoBehaviour
{
    [SerializeField] Sprite black;
    [SerializeField] Sprite red,   rDot;
    [SerializeField] Sprite blue,  bDot;
    [SerializeField] Sprite green, gDot;
    [SerializeField] Sprite yellow,yDot;
    [SerializeField] Sprite gray;
    [SerializeField] GameObject CirclePrefab; //身体组成，替换图片
    [SerializeField] GameObject linkPrefab;

    private static SnakeManager instance;
    public static SnakeManager Instance { get => instance; }
    
    #region 蛇本体
    class SnakeObj
    {
        public List<GameObject> bodys;
        public List<GameObject> links;
        public Transform snakeParent;
        public SnakeObj()
        {
            snakeParent = new GameObject().transform;//没有任何组件
            bodys = new List<GameObject>();
            links = new List<GameObject>();
        }
    }

    List<SnakeObj> snakes;//不只有一条蛇
    #endregion
   
    #region 拖动
    [SerializeField]
    float OutGirdDIs;//出格子后的移动判定距离
    GameObject dragTarget
    {
        get=>snakes[dragSnakeId].bodys[dragTargetId];
    }
    GameObject dragNo2 { 
        get {
                var snakeBodys = snakes[dragSnakeId].bodys;
            if (dragTargetId == 0)
                return snakeBodys[1];
            else
                return snakeBodys[snakeBodys.Count - 2];
        } 
    }

    private bool isDrag;//头尾拖动
    private GameObject dragLink;

 
    int dragSnakeId; 
    int dragTargetId;//snakes[snakeid].bodys[tarid]

    private Vector2 offset;    //目标-鼠标

    readonly Vector2 LinkSizeLR = new Vector2(1.2f,0.8f);
    readonly Vector2 LinkSizeUD = new Vector2(0.8f, 1.2f);
    #endregion

    #region 工具函数
 
    GameObject CreatBody(Sprite color, Sprite dot, int mapid, MapInfo BodyInfo)
    {
        GameObject circle = Instantiate(CirclePrefab);
        SnakeCircle mono = circle.GetComponent<SnakeCircle>();

        mono.SetImage(BodyInfo, mapid, color, dot);//init
        return circle;
    }
    void LinkInit(GameObject link, int id1, int id2)
    {
        link.transform.rotation = Quaternion.identity;

        SpriteRenderer ren = link.GetComponent<SpriteRenderer>();
        Vector2 prepos = GameManager.Instance.Id2Pos(id1);
        Vector2 lastpos = GameManager.Instance.Id2Pos(id2);

        link.transform.position = (prepos + lastpos) / 2;

        ren.size = (bool)(prepos.y == lastpos.y) ? LinkSizeLR : LinkSizeUD;
    }

    #endregion

    #region 公开接口

    public void Init(MapSpace.Snake data)
    {        //根据信息创造身体放在子节点下
        var snake = new SnakeObj();
        snake.snakeParent.name = snakes.Count.ToString();
        snake.snakeParent.SetParent(transform);

        snakes.Add(snake);

        for (int i = 0; i < data.snake.Count; i++)//生成身体
        {
            int mapid = data.snake[i];
            
            switch (data.snakeInfo[i])
            {
                case MapInfo.EMPTY:{
                        snake.bodys.Add(CreatBody(black, black, mapid, MapInfo.EMPTY));
                        break;
                    }
                case MapInfo.RED:{
                        snake.bodys.Add(CreatBody(red, rDot, mapid, MapInfo.RED));
                        break;
                    }
                case MapInfo.BLUE:{
                        snake.bodys.Add(CreatBody(blue, bDot, mapid, MapInfo.BLUE));
                        break;
                    }
                case MapInfo.YELLOW:{
                        snake.bodys.Add(CreatBody(yellow, yDot, mapid, MapInfo.YELLOW));
                        break;
                    }
                case MapInfo.GREEN:{
                        snake.bodys.Add(CreatBody(green, gDot, mapid, MapInfo.GREEN));
                        break;
                    }
                case MapInfo.GRAY:{
                        snake.bodys.Add(CreatBody(gray, gray, mapid, MapInfo.GRAY));
                        break;
                    }
                case MapInfo.BRICK:
                    throw new System.Exception("蛇信息出现砖块");
                default:
                    throw new System.Exception("蛇信息out of enum");
            }
            snake.bodys[i].transform.SetParent(snake.snakeParent);
        }
        snake.bodys[0].GetComponent<SnakeCircle>().IsHead = true;
        snake.bodys[snake.bodys.Count-1].GetComponent<SnakeCircle>().IsHead = true;

        for (int i = 1; i < data.snake.Count; i++)//创建连接
        {
            GameObject link = Instantiate(linkPrefab, snake.snakeParent);
            snake.links.Add(link);

            int pre = snake.bodys[i].GetComponent<SnakeCircle>().BodyInMap;
            int post = snake.bodys[i - 1].GetComponent<SnakeCircle>().BodyInMap;
            LinkInit(link, pre, post);
        }

    }
    public void ClearAllSnake()
    {
        snakes.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region mono函数
    private void Awake()
    {   //通过project setting设置mananger的优先级，这样其他类也能在awake中使用单例
        instance = this;
        snakes = new List<SnakeObj>();

        isDrag = false;
        dragSnakeId = dragTargetId = -1;
    }
    private void Update() => DragSnakeHead();


    #endregion

    void DragSnakeHead()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)//按下 input
        {
            //获取可拖动的body 
            Vector2 MousePos = Mouse.current.position.ReadValue();
            Vector2 WorldPos = Camera.main.ScreenToWorldPoint(MousePos);
            int mapid = GameManager.Instance.Pos2Id(WorldPos);

            for (int i = 0; i < snakes.Count; i++)//遍历所有蛇
            {
                var head = snakes[i].bodys[0].GetComponent<SnakeCircle>();
                int backId = snakes[i].bodys.Count - 1;
                var back = snakes[i].bodys[backId].
                    GetComponent<SnakeCircle>();
                dragTargetId =
                    head.BodyInMap == mapid ? 0 : -1;
                if (dragTargetId == -1)
                    dragTargetId =
                        back.BodyInMap == mapid ? backId : -1;

                dragSnakeId = i;
                if (dragTargetId != -1)
                {
                    InDrag(WorldPos, i);
                    break;
                }
            }

        }
        else if (Mouse.current.leftButton.IsPressed())
            OnDrag();
        else if (Mouse.current.leftButton.wasReleasedThisFrame)//抬起
            OutDrag();

    }

    void InDrag(Vector2 mouse,int snakeInArrId)
    {
        offset = (Vector2)dragTarget.transform.position - mouse;
        //理论xy都不会超过0.6  

        SnakeObj snake = snakes[snakeInArrId];//拿到蛇
        if (dragTarget == snake.bodys[0]) //小教训：gameobj和trans不是一个东西
        {//获取二号位和之间的连接
            dragLink = snake.links[0];
        }
        else if (dragTarget == snake.bodys[snake.bodys.Count - 1])
        {
            dragLink = snake.links[snake.links.Count - 1];
        }
        else
        {
            Debug.Log("出事了");
            return;
        }
            
        isDrag = true;
    }
    #region onDrag
    void SnakeTryMove(Vector2 mouseWorld)
    {
        /* 函数思路是检测鼠标位置和indrag记录的原点做对比，
         * 如果某个方向超出格子，则开始判定该方向的有效性
         * 最后更新蛇的信息
         */
        int _dragInMapId = dragTarget.GetComponent<SnakeCircle>().BodyInMap;

        var center = GameManager.Instance.Id2Pos(_dragInMapId);
        var GM = GameManager.Instance;
         float halfGird = GameManager.gridGap / 2 + OutGirdDIs;

        int endId = -1;
        if (mouseWorld.x - center.x > halfGird) //右跨过中线
        {
            if (false == GM.IsIdValid(_dragInMapId, Vector2.right))//不合法，跳过判定
                return;
            endId = _dragInMapId + 1;
        }
        else if (center.x - mouseWorld.x > halfGird)//左边
        {
            if (!GM.IsIdValid(_dragInMapId, Vector2.left))//不合法，跳过判定
                return;
            endId = _dragInMapId - 1;
        }
        else if (mouseWorld.y - center.y > halfGird)//上边
        {
            if (!GM.IsIdValid(_dragInMapId, Vector2.up))//不合法，跳过判定
                return;
            endId = _dragInMapId - GM.Map.weight;
        }
        else if (center.y - mouseWorld.y > halfGird)//下边
        {
            if (!GM.IsIdValid(_dragInMapId, Vector2.down))//不合法，跳过判定
                return;
            endId = _dragInMapId + GM.Map.weight;
        }
        else
        {
            Debug.Log("没出去");
            return;
        }
           

        if (!GM.IsIdCanMove(endId))
        {
            Debug.Log("砖块");
            return;
        }
         

        int backId;//拿到尾巴在地图的位置
        {
            var bodys = snakes[dragSnakeId].bodys;
            int idInBodys = (bool)(dragTargetId == 0) ? bodys.Count - 1 : 0;//数组下标
            backId = bodys[idInBodys].GetComponent<SnakeCircle>().BodyInMap;
        }
        
        if (IsHaveSnake(endId, backId))
        {
            Debug.Log("snake");
            return;
        }
           

        { //updata snake
            var _bodys = snakes[dragSnakeId].bodys;
            bool isReverse = (bool)(dragTargetId == 0);//从后往前

            for (int i = 0; i < _bodys.Count - 1; i++) //遍历个数为长度-1，留下最后一个免得tar越界
            {
                int index = isReverse ? _bodys.Count - 1 - i : i;
                int targetIndex = isReverse ? _bodys.Count - 2 - i : i + 1;
                //然后让index=tar
                var item = _bodys[index].GetComponent<SnakeCircle>();
                var Next = _bodys[targetIndex].GetComponent<SnakeCircle>();
                item.UpdateBody(Next.BodyInMap);
                //那么，link初始化需要两个id?
            }
            dragTarget.GetComponent<SnakeCircle>().UpdateBody(endId);

            for (int i = 0; i < snakes[dragSnakeId].links.Count; i++)
            { //需要等body的参数整体变更完毕后
                int pre = snakes[dragSnakeId].bodys[i].GetComponent<SnakeCircle>().BodyInMap;
                int post = snakes[dragSnakeId].bodys[i + 1].GetComponent<SnakeCircle>().BodyInMap;
                LinkInit(snakes[dragSnakeId].links[i], pre, post);
            }
        }
    }
    //目标点和尾部额外点
    bool IsHaveSnake(int tarid, int excludingid)
    {

        for (int i = 0; i < snakes.Count; i++)
        {
            var bodys = snakes[i].bodys;
            for (int j = 0; j < bodys.Count; j++)
            {
                var SnakeInMap = bodys[j].GetComponent<SnakeCircle>().BodyInMap;

                if (SnakeInMap == tarid && SnakeInMap != excludingid)
                    return true;//目的地有蛇
            }
        }

        return false;//全部遍历 无
    }


    #endregion
    void OnDrag()
    {
        if (!isDrag)
            return;

        Vector2 MousePos = Mouse.current.position.ReadValue();
        Vector2 mouseWrold = Camera.main.ScreenToWorldPoint(MousePos);

        //则鼠标+偏移=目标位置   
        dragTarget.transform.position = mouseWrold + offset;

        SnakeTryMove(mouseWrold);//数据更新
        
        #region 连接形变
        var ren = dragLink.GetComponent<SpriteRenderer>();
        //修改旋转和size
        Vector3 DragPos = dragTarget.transform.position;
        Vector3 No2Pos = dragNo2.transform.position;

        dragLink.transform.position = (DragPos + No2Pos) / 2;
        Vector3 linkDir = (DragPos - No2Pos);
        ren.size = new Vector2(linkDir.magnitude,0.8f);//长度
        dragLink.transform.right = linkDir;//旋转
        #endregion
    }

    void OutDrag()
    {
        if (isDrag == false) 
            return;

        #region 游离端link恢复
        //指定link初始化
        int bodyid1, bodyid2;
        if (dragLink == snakes[dragSnakeId].links[0])
        {//头
            bodyid1 = 0;
            bodyid2 = 1;
        }
        else
        {
            bodyid1 = snakes[dragSnakeId].bodys.Count - 1;
            bodyid2 = snakes[dragSnakeId].bodys.Count - 2;//数组id
        }
        bodyid1 = snakes[dragSnakeId].bodys[bodyid1].GetComponent<SnakeCircle>().BodyInMap;//地图id
        bodyid2 = snakes[dragSnakeId].bodys[bodyid2].GetComponent<SnakeCircle>().BodyInMap;

        LinkInit(dragLink, bodyid1, bodyid2);

        var mono = dragTarget.GetComponent<SnakeCircle>();
        mono.UpdateBody(mono.BodyInMap);//游离端身体归位
        #endregion


        isDrag = false;
        dragLink = null;
        dragTargetId = dragSnakeId = -1;
        offset = Vector2.zero;//out drag

        if (GameManager.Instance.IsWin())
        {
            Debug.LogError("你赢了");
            GameManager.Instance.ChangeMap();
        } 
    }




    public SnakeCircle IdGetCIrcle(int id)
    {
        for (int i = 0; i < snakes.Count; i++)
        {
            var bodys = snakes[i].bodys;
            for (int j = 0; j < bodys.Count; j++)
            {
                var mono = bodys[j].GetComponent<SnakeCircle>();
                if (mono.BodyInMap == id)
                    return mono;

            }
        }
        Debug.Log("id 没蛇" + id);
        return null;
    }
}
