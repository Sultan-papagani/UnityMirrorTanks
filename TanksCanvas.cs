using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class TanksCanvas : MonoBehaviour
{
    public TextMeshProUGUI PlayerCountButtonText;
    public int PlayerCountMax = 2;

    [Scene]
    public string MainMenuScene;

    public void PlayerCountSet()
    {
        if (PlayerCountMax < 18)
        {
            PlayerCountMax++;
        }
        else
        {
            PlayerCountMax = 2;
        }

        PlayerCountButtonText.text = $"Max Kişi Sayısı:{PlayerCountMax}";
    }


    public void ReturnMainMenu()
    {
        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.dontDestroyOnLoad = false;
        Destroy(NetworkManager.singleton.gameObject);
        SceneManager.LoadScene(MainMenuScene);
    }

}
