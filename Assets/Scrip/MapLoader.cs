using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapSpace;
using System;
using System.IO;

namespace MapSpace
{
    /// <summary>
    /// 地图和蛇的元素用这个
    /// </summary>
    public enum MapElement
    {    //地图有空地 砖块  红蓝黄绿 灰色和橙色可以直接加枚举扩充
        EMPTY = 0,  BRICK = 1,
        RED = 2,    BLUE = 3,
        YELLOW = 4, GREEN = 5,
        GRAY =6
    }
    [Serializable] public class Snake
    {
        public List<int> snake;//蛇在地图的位置，是地图下标
        public List<MapElement> snakeInfo;//长度和上边一致，蛇的内容物
    }
    [Serializable] public class Level  
    {
        public List<MapElement> MapData;
        public int height, weight;//8*7,
        
        public List<Snake> SnakeData;//然后是蛇的数据
    }
    [Serializable] public class MapLoader 
    {
        public Level levelData; //封装，顺带函数

        static public Level MapLoad(string asset)
        {
            MapLoader data=JsonUtility.FromJson<MapLoader>(asset);
            return data.levelData;
        }
        static public void MapWrite(Level data,string path)
        {
            MapLoader levelobj = new MapLoader();
            levelobj.levelData = data;
            string json = JsonUtility.ToJson(levelobj);
            File.WriteAllText(path, json);
        }
    }

}

/*还有一种实现方式，是在其他类中，保留load实体，
 * 只交互load而不管level，算代理者模式
 * 如今的实现是level做核心，load只是工具，算简朴模式*/
