using UnityEngine;
using System.Collections;

public class LevelButtonController : MonoBehaviour
{

    public void HandleOnClick()
    {
        GameManager.levelLoaded = int.Parse(GetComponentInChildren<UnityEngine.UI.Text>().text);
        GameManager.gameMode = GameMode.Quest;
        SgLib.SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.button);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
