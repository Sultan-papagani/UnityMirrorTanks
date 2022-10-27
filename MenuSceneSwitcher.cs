using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;

public class MenuSceneSwitcher : MonoBehaviour
{
    public TMP_InputField oyuncu_adi;

    [Scene]
    public string TanksScene;

    [Scene]
    public string SoloTestScene;

    public bool validateName()
    {
        if (oyuncu_adi.text != "" && oyuncu_adi.text.Length < 35)
        {
            string ad = oyuncu_adi.text;
            ad = ad.ToLower();
            if (ad.Contains("mari") || ad.Contains("marı"))
            {
                return false;
            }

            PlayerData.singleton.playername = oyuncu_adi.text;
            return true;
        }
        return false;
    }
    
    public void OnlineTanksScene()
    {
        if (validateName())
        {
            SceneManager.LoadScene(TanksScene);
        }
    }

    public void OpenSoloTest()
    {
        SceneManager.LoadScene(SoloTestScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
