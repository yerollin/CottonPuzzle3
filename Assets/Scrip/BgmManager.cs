using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmManager : MonoBehaviour
{
    AudioSource _audio;
    void Start()
    {
        if (!TryGetComponent(out _audio))
            Debug.Log("bgm audio source 404");

       GameManager.Instance.OnMusic.AddListener(OnMusic);//联动

        OnMusic(GameManager.Instance.IsMusic);//初始化
    }

    void OnMusic(bool enable)
    {
        if (enable)
            _audio.Play();
        else 
            _audio.Pause();
    }
}
