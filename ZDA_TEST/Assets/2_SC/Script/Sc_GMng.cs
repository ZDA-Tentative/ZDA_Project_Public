using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_GMng : MonoBehaviour
{

    // Start is called before the first frame update
    public static Sc_GMng instance = null;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }






    
}
