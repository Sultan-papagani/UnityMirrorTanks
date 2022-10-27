using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Oyuncu Adı")]
    public string playername;

    public static PlayerData singleton;

    void Awake() 
    {
        singleton = this;
        DontDestroyOnLoad(this);
    }
}
