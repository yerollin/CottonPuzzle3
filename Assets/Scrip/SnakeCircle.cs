using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using MapSpace;
public class SnakeCircle : MonoBehaviour
{
    #region 自切换身体虚实
    [SerializeField]
    SpriteRenderer _renderer;//预制体管理类必备的Serial

    public int BodyInMap {  get; private set; }
    Sprite colorSprite, dotSprite;
    public MapElement BodyInfo { get; private set; }

    public bool IsHead;
    private void Awake()
    {
        BodyInfo = (MapElement)(-1);//初始化，不建议构造函数
        IsHead = false;
    }
    public void SetImage(MapElement element, int mapid, Sprite color, Sprite dot)
    {
        if ((int)BodyInfo != -1)
        {
            Debug.LogError("身体元素只支持一次初始化");
            return;
        }
        BodyInfo = element;
        colorSprite = color;

        dotSprite =  dot;

        UpdateBody(mapid);
    }
    
    /// <summary>
    /// 传入地图id
    /// </summary>
    public void UpdateBody(int id)
    {
        BodyInMap = id;
        transform.position = GameManager.Instance.Id2Pos(BodyInMap);

        MapElement gird = GameManager.Instance.Map.MapData[BodyInMap];
        _renderer.sprite = 
            gird == BodyInfo ? dotSprite : colorSprite;
    }
    #endregion

  
}
