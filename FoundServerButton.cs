using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoundServerButton : MonoBehaviour
{
    public TextMeshProUGUI owner_name;
    public TextMeshProUGUI room_number;
    public Button button;

    public void Setup(string name, int currentNumber, int maxNumber, Transform parent)
    {
        owner_name.text = $"Kurucu:{name}";
        room_number.text = $"{currentNumber}/{maxNumber}";

        transform.SetParent(parent);
        transform.localScale = Vector3.one;
    }

}
