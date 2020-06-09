using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System;

class StrRecInfo {                  //XmlElement.setattribute()를 숫자로 처리하기 위해 만든 클래스
    public const int INFOCOUNT = 4;

    private string[] strRecInfo = new string[INFOCOUNT];

    ////http://www.csharpstudy.com/CSharp/CSharp-indexer.aspx
    public string this[int index]
    {
        get
        {
            if(index<0||index>=INFOCOUNT)
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                return strRecInfo[index];
            }
        }
    }

    //초기값 할당
    public StrRecInfo()                     //저장하는 정보 이름
    {
        this.strRecInfo[0] = "SceneName";
        this.strRecInfo[1] = "CharacterName";
        this.strRecInfo[2] = "ch_pos";
        this.strRecInfo[3] = "ch_rot";
        this.strRecInfo[4] = "cam_pos";
        this.strRecInfo[5] = "cam_rot";

    }
}

public class RecInfo
{
    public string SceneName;        //플레이어가 있는 맵
    //게임의 시간 (솔플인 경우에는 필요할수도...?)

    public string CharacterName;    //플레이어의 캐릭터 이름
    //public                        //캐릭터의 스킨(복장)
    public Vector3 ch_pos;          //캐릭터의 위치값 저장
    public Vector3 ch_rot;          //캐릭터의 회전값 저장


    //아직 미구현 -> 애니메이션 작업 끝나고 작업하자
    public Vector3 cam_pos;
    public Vector3 cam_rot;

}

public sealed class SaveLoadData
{
    public static void Write(RecInfo Info, string filePath)
    {
        StrRecInfo sri = new StrRecInfo();

        XmlDocument Document = new XmlDocument();
        XmlElement InfoElement = Document.CreateElement("Info");
        Document.AppendChild(InfoElement);

        //Info.SceneName을 숫자로 처리하는 방법이 있나?
        /*for(int i = 0; i < StrRecInfo.INFOCOUNT; i++)
        {
            InfoElement.SetAttribute(sri[i], Info.???);
        }*/
        InfoElement.SetAttribute(sri[0],Info.SceneName);
        InfoElement.SetAttribute(sri[1],Info.CharacterName);
        InfoElement.SetAttribute(sri[2],Info.ch_pos.x.ToString()+","+Info.ch_pos.y.ToString()+","+Info.ch_pos.z.ToString());
        InfoElement.SetAttribute(sri[3],Info.ch_rot.x.ToString()+","+Info.ch_rot.y.ToString()+","+Info.ch_rot.z.ToString());
        Document.Save(filePath);
    }

    public static RecInfo Read(string filePath)
    {
        XmlDocument Document = new XmlDocument();
        Document.Load(filePath);
        XmlElement InfoElement = Document["Info"];

        char sp = ',';

        RecInfo Info = new RecInfo();
        Info.SceneName=InfoElement.GetAttribute("SceneName");
        Info.CharacterName=InfoElement.GetAttribute("CharacterName");
        string[] sppos = InfoElement.GetAttribute("ch_pos").Split(sp);
        float[] fpos = new float[sppos.Length];
        for(int i = 0; i<sppos.Length; i++)
        {
            fpos[i] = System.Convert.ToSingle(sppos[i]);
        }
        Info.ch_pos = new Vector3(fpos[0],fpos[1],fpos[2]);
        string[] sprot = InfoElement.GetAttribute("ch_rot").Split(sp);
        float[] frot = new float[sprot.Length];
        for(int i = 0; i<sprot.Length; i++)
        {
            frot[i] = System.Convert.ToSingle(sprot[i]);
        }
        Info.ch_rot = new Vector3(frot[0],frot[1],frot[2]);

        return Info;
    }
}
