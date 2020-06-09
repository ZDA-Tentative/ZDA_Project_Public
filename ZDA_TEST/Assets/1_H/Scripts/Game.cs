using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private GameObject World;       //현재의 월드맵
    public string SceneName;        //월드맵의 이름    
    public List<GameObject> enemy = new List<GameObject>();     //나중에 Obj pool 사용

    private void Awake()
    {
        OnValidate();
        World = this.gameObject;

        SceneName = World.name;
    }

    void OnValidate()
    {

    }

    

}
