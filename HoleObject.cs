using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HoleObject : MonoBehaviour
{
    public bool taken = false;   // şu anki durumu
    public bool canTaken = true; // kenardaki fazlalıkların alınmaması için
    public Transform piyon;

    public void ChangeMesh(bool h)
    {
        if (h)
        {
            GetComponent<Renderer>().material.SetColor("_Color", Color.cyan); 
            return;
        }
        GetComponent<Renderer>().material.SetColor("_Color", Color.black); 
    }
}
