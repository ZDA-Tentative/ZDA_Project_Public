using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{

    public delegate void D_ChainFuc(int value);
    public static event D_ChainFuc onStart; // static은 외부에서 접근하기 편하게 하기 위함.
    D_ChainFuc chain;
    // Start is called before the first frame update
    float power=0;
    public void SetPower(int value)
    {
        //Debug.Log(power++);
    }
    public void DePower(int value)
    {
        
        //Debug.Log(--power);
    }
    void Start()
    {
        chain += SetPower; // () 없어야 하는구나 
        chain += DePower;
        StartCoroutine(aaa());
        StartCoroutine(bbb());

        //st aaa a1 a2
        //st bbb b1 b2
        // a1 a23 a3 b1 b2 b3 
        // a1 b1 a2 b2
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    IEnumerator aaa()
    {
        for (int i = 0; i < 100; i++)
        {
            Debug.Log("aaa : " + i);
            yield return null;
        }
            

    }
    IEnumerator bbb()
    {
        for (int i = 0; i < 100; i++)
        {
            Debug.Log("bbb : " + i);
            yield return null;
        }

    }
}
