using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TanksGameCanvas : MonoBehaviour
{
    public static TanksGameCanvas singleton;
    public TextMeshProUGUI can;
    
    void Awake() 
    {
        singleton = this;
    }

    public void SetHealth(int i )
    {
        if (i <= 0)
        {
            can.text = "Öldünüz";
            return;
        }
        can.text = new string('*', i);
    }

}
