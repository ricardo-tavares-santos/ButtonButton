using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelEditorUI : MonoBehaviour
{

    public GameManager levelEditorGameManager;
    public Button btnShoot;
    public Button btnRetry;
    public Image imgStar;
    public Text txtGetStarTime;
    public Text txtLevelComplete;

    private float getStarTime;
    private bool stopCheck;
    private bool stopCountTime;

    // Use this for initialization
    void Start()
    {
        if (levelEditorGameManager.gameEnvironment == GameEnvironment.LEVEL_EDITOR)
        {
            StartCoroutine(CountDownGetStarTime());
            btnShoot.gameObject.SetActive(true);
            btnRetry.gameObject.SetActive(false);
            txtLevelComplete.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (levelEditorGameManager.gameEnvironment == GameEnvironment.LEVEL_EDITOR && !stopCheck)
        {
            if (levelEditorGameManager.gameOver || levelEditorGameManager.passLevel)
            {
                stopCheck = true;
                stopCountTime = true;

                btnShoot.gameObject.SetActive(false);
                btnRetry.gameObject.SetActive(true);

                if (levelEditorGameManager.gameOver)
                {
                    txtLevelComplete.text = "FAILED!";
                }
                else if (levelEditorGameManager.passLevel)
                {
                    txtLevelComplete.text = getStarTime > 0 ? "PASSED WITH STAR!" : "PASSED WITHOUT STAR!";
                }
                txtLevelComplete.gameObject.SetActive(true);
            }

            if (getStarTime <= 0)
            {
                imgStar.gameObject.SetActive(false);
                txtGetStarTime.gameObject.SetActive(false);
            }
            else
            {
                imgStar.gameObject.SetActive(true);
                txtGetStarTime.text = getStarTime.ToString();
            }
        }
    }

    public void DisplayGetStarTime(float time)
    {
        txtGetStarTime.text = (time - 1).ToString() + ".99";
    }

    IEnumerator CountDownGetStarTime()
    {
        //Wait until gameManager load get star time
        while (getStarTime == 0)
        {
            getStarTime = levelEditorGameManager.getStarTime;
            yield return null;
        }

        while (getStarTime > 0)
        {
            getStarTime = getStarTime - 1;
            float milisecond;
            float starMilisecond = 99;
            float endMilisecond = 0;
            float t = 0;
            while (t < 1f)
            {
                if (stopCountTime)
                    yield break;

                t += Time.deltaTime;
                float fraction = t / 1f;
                milisecond = (int)Mathf.Lerp(starMilisecond, endMilisecond, fraction);
                txtGetStarTime.text = getStarTime.ToString() + "." + milisecond.ToString(); 
                yield return null;
            }
        }
        stopCountTime = true;
        yield return new WaitForSeconds(0.5f);
        txtGetStarTime.gameObject.SetActive(false);
        imgStar.gameObject.SetActive(false);
    }

    public void HandleRetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
