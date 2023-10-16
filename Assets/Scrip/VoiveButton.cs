using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoiveButton : MonoBehaviour
{
    [SerializeField]
    Image _closeImage;
  
    private void OnEnable()
    {
        UpdataCoinByGM();
        //经过测试，不加入项目设置的脚本enable似乎优先度比GM的awake还高
    }

    public void Onclick()
    {
        GameManager.Instance.IsMusic = !GameManager.Instance.IsMusic;
        UpdataCoinByGM();
    }
    void UpdataCoinByGM()
    {
        _closeImage.gameObject.SetActive(!GameManager.Instance.IsMusic);
        //思路记录，如果有多个类似bool需要关联，则脚本需要可复用
        //这时候可以将存档的值类型装包为引用类型
        //然后【ser】一个action用于在编辑器拖拽
        //返回该引用类型开包当作参数使用

        //或者使用Getbool("string")，这样ser的就是string，也行
    }    
}
