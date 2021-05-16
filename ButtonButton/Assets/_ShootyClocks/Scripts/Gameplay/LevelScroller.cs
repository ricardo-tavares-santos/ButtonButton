using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SgLib;

public class LevelScroller : MonoBehaviour
{

    public static string[] data;
    public static float anchoredPositionX = 0f;

    public GameObject content;
    public GameObject levelButtonPrefab;
    public GameObject levelGroupPrefab;

    [Header("Level Scroller Config")]
    public int levelNumberInAGroup;
    public Color disableColor;

    // Use this for initialization
    void Start()
    {
        //Get levels data
        if (data == null)
            data = Resources.Load<TextAsset>("LevelsData").ToString().Split(';');


        //Caculate how many levelGroup 
        int levelGroupNumber = Mathf.CeilToInt((data.Length - 1) / (float)levelNumberInAGroup);
        int currentLevel = 0;
        Color color = Camera.main.backgroundColor;

        //Generate levels
        for (int i = 0; i < levelGroupNumber; i++)
        {
            GameObject levelGroup = Instantiate(levelGroupPrefab) as GameObject;
            levelGroup.transform.SetParent(content.transform);
            levelGroup.transform.localScale = new Vector2(0.9F, 0.9F);
                 
            for (int j = 0; j < levelNumberInAGroup; j++)
            {
                currentLevel++;
                if (currentLevel > data.Length - 1)
                    break;
                GameObject levelButton = Instantiate(levelButtonPrefab) as GameObject;
                levelButton.transform.SetParent(levelGroup.transform);
                levelButton.transform.localScale = new Vector2(1, 1);

                levelButton.transform.Find("Star").GetComponent<Outline>().effectColor = color;

                Text levelButtonText = levelButton.GetComponentInChildren<Text>();
                levelButtonText.text = currentLevel.ToString();
                levelButtonText.color = color;

                Button button = levelButton.GetComponent<Button>();
                if (IsTheLevelSolved(currentLevel) || currentLevel == PlayerPrefs.GetInt(GameManager.MAX_LEVEL_SOLVED) + 1)
                {
                    button.interactable = true;
                    //levelButton.GetComponent<Image>().color = disableColor;
                }
                else
                {
                    button.interactable = false;
                    levelButton.GetComponent<Image>().color = disableColor;
                }
     
                GameObject star = levelButton.transform.Find("Star").gameObject;
                if (IsTheLevelHasStar(currentLevel))
                    star.SetActive(true);
                else
                    star.SetActive(false);
            }
        }


        //Adjust content
        GridLayoutGroup contentGrid = content.GetComponent<GridLayoutGroup>();
        float factor = (levelGroupNumber - 1) * contentGrid.cellSize.x + (levelGroupNumber - 1) * contentGrid.spacing.x;
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(factor, 0);
        contentRect.anchoredPosition = new Vector2(anchoredPositionX, 0);
    }

    bool IsTheLevelSolved(int levelNumber)
    {
        string[] levelSolvedData = PlayerPrefs.GetString(GameManager.LEVEL_SOLVED).Split(';');
        if (levelSolvedData.Length > 1)
        {
            for (int i = 0; i < levelSolvedData.Length - 1; i++)
            {
                if (int.Parse(levelSolvedData[i]) == levelNumber)
                    return true;
            }
        }       
        return false;
    }

    bool IsTheLevelHasStar(int levelNumber)
    {
        string[] levelHasStarData = PlayerPrefs.GetString(GameManager.LEVEL_HAS_STAR).Split(';');
        if (levelHasStarData.Length > 1)
        {
            for (int i = 0; i < levelHasStarData.Length - 1; i++)
            {
                if (int.Parse(levelHasStarData[i]) == levelNumber)
                    return true;
            }
        }   
        return false;
    }
}
