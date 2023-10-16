using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NotUse
{

public class GlQuad : MonoBehaviour
{
    Material _glmat;
    Color _glColor;
    GameObject start, end;
    [SerializeField]
    Vector2[] QuadPoint4;

    #region 曾经的唯一入口
    //原本在snakemanager中的私有属性，现在不用了，扔回本体，
    //字段加上static，属性加上static public就是单例的另一种形式
    GlQuad _glDraw;
    GlQuad glDraw //非公开，小写，字段降一级为下划线
    {
        get
        {
            if (_glDraw == null)
            {
                var obj = new GameObject("Gl");
                _glDraw = obj.AddComponent<GlQuad>();
            }
            return _glDraw;
        }
    }
    #endregion


    public bool IsDraw { get; set; }
    private void Awake()
    {
        _glmat = new Material(Shader.Find("Sprites/Default"));
        _glColor = Color.black;
        QuadPoint4 = new Vector2[4];
    }

    private void OnRenderObject()
    {
        if (!IsDraw)
            return;

        GL.PushMatrix();

        _glmat.SetPass(0);
        GL.Begin(GL.QUADS);

        GL.Color(_glColor);
        GL.Vertex(QuadPoint4[0]);
        GL.Vertex(QuadPoint4[1]);
        GL.Vertex(QuadPoint4[2]);
        GL.Vertex(QuadPoint4[3]);
        GL.End();

        GL.PopMatrix();
    }
    private void Update()
    {
        if (!start)
            return;
        Vector2 dir = (start.transform.position - end.transform.position).normalized;
        Vector2 nor = new Vector2() { x = -dir.y, y = dir.x };//垂直法线
        QuadPoint4[0] = (Vector2)start.transform.position + nor * 0.4f;
        QuadPoint4[1] = (Vector2)start.transform.position - nor * 0.4f;
        QuadPoint4[2] = (Vector2)end.transform.position - nor * 0.4f;
        QuadPoint4[3] = (Vector2)end.transform.position + nor * 0.4f;
        //，第一方案是设置Pos的时候固定顶点，表现不行，所以实时计算，然而优先度有问题，故而启用
    }
    public void Set2Pos(GameObject a,GameObject b)
    {
        start = a;end = b;
    }
   
}

}