using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WriteTest : MonoBehaviour
{
    [SerializeField] private PlayerCtrl playerCtrl;
    [SerializeField] private Game game;

    void Start()
    {
        OnValidate();
    }

    void OnValidate()
    {    
        /*
        if(playerCtrl == null)
        {
            playerCtrl = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>();
        }
        if(playerCtrl == null)
        {
            game = GameObject.FindWithTag("World").GetComponent<Game>();
        }
        */
    }

    public void OnClickSave()
    {
        playerCtrl = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>();
        game = GameObject.FindWithTag("World").GetComponent<Game>();

        RecInfo info = new RecInfo();
        info.SceneName = game.SceneName;
        info.CharacterName = playerCtrl.CharacterName;
        info.ch_pos = playerCtrl.ch_pos;
        info.ch_rot = playerCtrl.ch_rot;

        SaveLoadData.Write(info, Application.dataPath + "/Output/Info_Attributes.xml");

    }
}
