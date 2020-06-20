using System.Collections;
using System.Collections.Generic;

public struct ComboAttack           //플레이어, 적 모두 사용 가능
{
    public int Total;
    public int Left;
    public int Right;
}

public struct MouseClick            //플레이어만 쓴다. 아마?
{
    public bool Wheel;
    public bool Left;
    public bool Right;
}

public struct valueBearing          //사방위의 값
{
    public float Front;
    public float Back;
    public float Left;
    public float Right;
    /*
    public valueBearing(float value)
    {
        Front = value;
        Back = value;
        Left = value;
        Right = value;
    }
    */
}

public struct isBearing         //사방위의 bool
{
    public bool Front;
    public bool Back;
    public bool Left;
    public bool Right;
}

public struct Delay
{
    public valueBearing DoubleClick;
    public float Combo;
    public float MouseWheel;
    public float InputMouse;

    public Delay(float v_DoubleClick, float v_Combo, float v_MouseWheel, float v_InputMouse)
    {
        DoubleClick.Front = v_DoubleClick;
        DoubleClick.Back = v_DoubleClick;
        DoubleClick.Left = v_DoubleClick;
        DoubleClick.Right = v_DoubleClick;

        Combo = v_Combo;
        MouseWheel = v_MouseWheel;
        InputMouse = v_InputMouse;
    }
}


public class DataType
{

}
