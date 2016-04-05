using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOptions : MonoBehaviour
{

    public static GameOptions instance;

    public void Awake()

    {
        DontDestroyOnLoad(gameObject);


        if (instance == null)
            instance = this;

    }


}
