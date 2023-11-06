using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFloat : MonoBehaviour
{
    public bool IsFloat
    {
        set
        {
            isFloat = value;
            if (value)
                Init();
        }
    }
    public Vector3 PosOffest;
    public Vector3 FloatValue;
    public float Speed = 1f;

    private bool isFloat = false;

    // Start is called before the first frame update
    void Init()
    {
        transform.GetChild(0).transform.position += PosOffest;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFloat) 
            transform.rotation = Quaternion.Euler(FloatValue * Time.time * Speed);
    }
}
