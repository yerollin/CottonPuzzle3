using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapSpace;
using System.IO;
using UnityEngine.Events;
/*
* 命名规则暂定
* 私有组件  _renderer   下划线 1
* 私有变量  instance    小写 2
* 公开属性  Instance    大写 3 
* 
* 不固定，比如私有属性是小写，则关联的字段降级为下划线
* 
* mapinfo是地图元素，蛇也用这个enum
* 在snakecircle中，color指空心图片，dot指实心图片
* 
*/
public class GameManager : MonoBehaviour
{
  
    const string folderPath = "Assets/Map";
    #region Music
    bool isMusic;

    [SerializeField]
    public class BoolEvent : UnityEvent<bool> { }
    //我记得以前不需要继承和new啊
    public BoolEvent OnMusic = new BoolEvent();
    public bool IsMusic { 
        get => isMusic;  
        set{
            if (isMusic == value)
                return;
            isMusic = value;
            OnMusic?.Invoke(value);
        } 
    }
    #endregion
   
    #region 游戏生成素材和参数
    public Level Map { get; private set; }
   

    static readonly Vector2 zeroPos = new Vector2(-4.23f, 3.59f);
    //左上角初始值，中心偏移值-0.03 -0.01  更好看一点点
    public const float gridGap = 1.2f;//方格间距

    [SerializeField] GameObject GridPrefab;//预制体
    [SerializeField] List<Sprite> GridSprites;//替换的图片，和枚举匹配，0是null


    #endregion
    
    private static GameManager instance;
    public static GameManager Instance { get => instance; }

    #region Mono

    private void Awake()
    {
        instance = this;//通过project setting设置mananger的优先级最高
        CreatMapObj();
    }
    private void Start()
    {
        CreatSnake();
    }
   
    #endregion
    
    #region 自用工具函数

    void CreatGrid(int id, MapElement GridInfo)
    {
        if (GridInfo == MapElement.EMPTY)
            return;

        var b = Instantiate(GridPrefab, transform);
        b.GetComponent<SpriteRenderer>().sprite = 
            GridSprites[(int)GridInfo];
        //复杂结构再采用管理类，简单结构直接代码处理了省事

        b.transform.position = Id2Pos(id);//设置位置
    }
    int nowMapIndex = -1;//每次随机都不能和现在相同
    void CreatMapObj()
    {
        List<Level> _maps = new List<Level>();
        int nums = 0;
        while (true)
        {
            string _path = folderPath + "/" + nums + ".json";
            if (File.Exists(_path))
                nums++;
            else
                break;
        }
        int tarIndex;//和now 做对比

        do
            tarIndex = Random.Range(0, nums);
        while (tarIndex == nowMapIndex);

        string path = folderPath + "/" + tarIndex + ".json";

        Map = MapLoader.MapLoad(File.ReadAllText(path));
        nowMapIndex = tarIndex;

        for (int i = 0; i < Map.height; i++)
        {
            for (int j = 0; j < Map.weight; j++)
            {
                int index = i * Map.weight + j;//编号
                CreatGrid(index, Map.MapData[index]);
            }
        }
    }
    void CreatSnake()
    {
        for (int i = 0; i < Map.SnakeData.Count; i++)
            SnakeManager.Instance.Init(Map.SnakeData[i]);
    }

    #endregion
   
    #region 公开的工具函数 or 接口

    /// <summary>
    /// 通过0-55的地图下标返回地图块的世界坐标
    /// </summary>
    /// <param name="mapId"></param>
    public Vector2 Id2Pos(int mapId)
    {
        if (mapId < 0 || mapId >= Map.weight * Map.height)
        {
            Debug.LogWarning("id非法");
        }
        int row = mapId / Map.weight;//第几行
        int col = mapId % Map.weight;//第几列
        Vector2 pos = zeroPos;
        pos.x += col * gridGap;
        pos.y -= row * gridGap;

        return pos;
    }
    
    /// <summary>
    /// 判断该点在哪个坐标 -1为out range
    /// </summary>
    public int Pos2Id(Vector2 WorldPos)
    {
        //边界测试  ↓↓
        {
            float minX = zeroPos.x - gridGap / 2;
            float maxY = zeroPos.y + gridGap / 2;

            if (WorldPos.x < minX || WorldPos.y > maxY)
                return -1;//out start

            var rightBottom = Id2Pos(Map.height * Map.weight - 1);//右下

            float maxX = rightBottom.x + gridGap / 2;
            float minY = rightBottom.y - gridGap / 2;

            if (WorldPos.x > maxX || WorldPos.y < minY)
                return -1;//out end
        }
        //id 分配
        for (int i = 0; i < Map.height; i++){
            for (int j = 0; j < Map.weight; j++){
                int index = i * Map.weight + j;//遍历地图
                var center = Id2Pos(index);
                /*
                然后判断距离，方案有两种，
                1.x,y的距离矩形   2.(a-b).magnitude 圆形
                其实有比遍历更低开销的办法，类似寻路，
                但这里使用最简洁的遍历思路
                因为遍历性能开销并不大，之后根据开发需求再优化
                也能避免开发初期在细枝末节上消耗无用的功夫
                */
                if (Mathf.Abs(WorldPos.x - center.x) < gridGap / 2)
                    if (Mathf.Abs(WorldPos.y - center.y) < gridGap / 2)
                        return index;
            }
        }


        return -2;// 超出设想范围
    }
    
    /// <summary>
    /// 参数用vector.up代替dir，检测id的四周是否越界
    /// </summary>
    public bool IsIdValid(int id, Vector2 dir)
    {
        if (id < 0 || id >= Map.height * Map.weight)
            return false;

        if (dir==Vector2.up){
            if (id<Map.weight)
                return false;
        }
        else if (dir==Vector2.down){
            if (id >= Map.weight * (Map.height - 1)) 
                return false;
        }
        else if (dir==Vector2.left){
            if (id%Map.weight==0)
                return false;
        }
        else if (dir==Vector2.right){
            if ((id+1) % Map.weight == 0)
                return false;
        }

        return true;
    }
    
    /// <summary>
    /// 检测砖块
    /// </summary>
    public bool IsIdCanMove(int id)
    {
        if (id < 0 || id >= Map.height * Map.weight)
            return false;

        if (Map.MapData[id]==MapElement.BRICK)
            return false;

        return true;
    }

    //场景拖进去的函数
    public void ChangeMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
         CreatMapObj();

        SnakeManager.Instance. ClearAllSnake();
        CreatSnake();
    }
    
    //地图上所有颜色位置都占满了
    public bool IsWin()
    {
        var SnakeGM = SnakeManager.Instance;

        for (int i = 0; i < Map.height*Map.weight-1; i++)
        {
            var mapColor = Map.MapData[i];
            if ((mapColor != MapElement.EMPTY) &&
                (mapColor != MapElement.BRICK)) {
                var mono= SnakeGM.IdGetCIrcle(i);
                if (!mono || mono.BodyInfo != mapColor)
                    return false;//如果地图上有砖没蛇或者蛇色不对

            }
        }

        return true;
    }
    #endregion
}
