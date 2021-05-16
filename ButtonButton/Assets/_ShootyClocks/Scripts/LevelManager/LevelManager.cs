using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class LevelManager : MonoBehaviour
{
    public static LevelData CurrentLoadedLevelData { get; private set; }

    private int screenshotWidth = 213;
    private int screenshotHeight = 378;
    private int bitType = 16;
    public const string ROOT_FOLDER = "Assets/_ShootyClocks";
    public const string DATA_PATH = ROOT_FOLDER + "/Resources/LevelsData.json";
    public const string SCREEN_SHOT_PATH = ROOT_FOLDER + "/Screenshots";

    /// <summary>
    /// Get the screenshot file path
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns></returns>
    public string ScreenshotPath(int levelNumber)
    {
        return SCREEN_SHOT_PATH + "/" + levelNumber.ToString() + ".png";
    }

    /// <summary>
    /// Check the level has no or many shooting clock
    /// </summary>
    /// <returns></returns>
    public bool AllowSaveLevel()
    {
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();

        //There's just one or zero clock ,the level must has two clocks or greater
        if (clockControllers.Length <= 1)
            return false;

        List<bool> listTemp = new List<bool>();
        foreach (ClockController o in clockControllers)
        {
            if (o.isShootingClock)
                listTemp.Add(o.isShootingClock);
        }

        if (listTemp.Count == 1)
            return true; //Has just one shooting clock -> allow save level
        else
            return false;
    }

    /// <summary>
    /// Get the level data file path
    /// </summary>
    /// <returns></returns>
    public string LevelDataPath()
    {
        return DATA_PATH;
    }

    string[] LoadJsonData()
    {
        StreamReader reader = new StreamReader(LevelDataPath());
        string[] data = reader.ReadToEnd().Split(';');
        reader.Close();
        return data;
    }

    /// <summary>
    /// Get the total level number
    /// </summary>
    public int GetTotalLevelNumber()
    {
        return LoadJsonData().Length - 1;
    }

    /// <summary>
    /// Get the star time of the level
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns></returns>
    public float GetStarTimeOfLevel(int levelNumber)
    {
        string[] data = LoadJsonData();
        for (int i = 0; i < data.Length - 1; i++)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(data[i]);
            if (levelData.levelNumber == levelNumber)
            {
                return levelData.getStarTime;
            }
        }
        return 0;
    }

    /// <summary>
    /// Clear all clocks 
    /// </summary>
    public void ClearScene()
    {
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        foreach (ClockController o in clockControllers)
        {
            DestroyImmediate(o.gameObject);
        }
    }


    /// <summary>
    /// Save the level
    /// </summary>
    /// <param name="levelNumber"></param>
    public void SaveLevel(int levelNumber)
    {
        WriteData(GetLevelData(levelNumber));
    }


    /// <summary>
    /// Load the level
    /// </summary>
    /// <param name="levelNumber"></param>
    public void LoadLevel(int levelNumber)
    {
        GameManager gameManager = FindObjectOfType<GameManager>();

        //Clear scene
        ClearScene();

        //Load the current level data by level number
        CurrentLoadedLevelData = LoadLevelData(levelNumber, LoadJsonData());

        // Display getStarTime
        LevelEditorUI levelEditorUI = FindObjectOfType<LevelEditorUI>();
        levelEditorUI.DisplayGetStarTime(CurrentLoadedLevelData.getStarTime);

        //Create clocks
        foreach (ClockData o in CurrentLoadedLevelData.listClockData)
        {
            gameManager.CreateClockByClockData(o);
        }
    }


    /// <summary>
    /// Override the level
    /// </summary>
    /// <param name="levelNumber"></param>
    public void OverwriteLevel(int levelNumber)
    {
        string[] data = LoadJsonData();
        for (int i = 0; i < data.Length - 1; i++)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(data[i]);
            if (levelData.levelNumber == levelNumber)
            {
                string newData = GetLevelData(levelNumber);
                data[i] = newData;
                break;
            }
        }

        StreamWriter writer = new StreamWriter(LevelDataPath(), false);
        for (int i = 0; i < data.Length - 1; i++)
        {
            string dataTemp = data[i].Trim() + ";";
            writer.WriteLine(dataTemp);
        }
        writer.Close();
        TakeScreenshot(levelNumber);
    }

    /// <summary>
    /// Take screenshot of the level
    /// </summary>
    /// <param name="levelNumber"></param>
    public void TakeScreenshot(int levelNumber)
    {
        StartCoroutine(SnapShot(levelNumber));
    }

    IEnumerator SnapShot(int levelNumber)
    {
        LevelEditorUI levelEditorUi = FindObjectOfType<LevelEditorUI>();
        levelEditorUi.txtGetStarTime.gameObject.SetActive(false);
        levelEditorUi.imgStar.gameObject.SetActive(false);
        RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, bitType, RenderTextureFormat.ARGB32);
        yield return new WaitForEndOfFrame();
        Camera.main.targetTexture = rt;
        Texture2D tx = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.ARGB32, false);
        Camera.main.Render();
        RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        tx.Apply();
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        byte[] bytes = tx.EncodeToPNG();
        File.WriteAllBytes(ScreenshotPath(levelNumber), bytes);
        levelEditorUi.txtGetStarTime.gameObject.SetActive(true);
        levelEditorUi.imgStar.gameObject.SetActive(true);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    void WriteData(string data)
    {
        string dataTemp = data + ";";
        StreamWriter writer = new StreamWriter(LevelDataPath(), true);
        writer.WriteLine(dataTemp);
        writer.Close();
    }

    string GetLevelData(int levelNumber)
    {
        LevelData levelData = new LevelData();
        List<ClockData> listClockData = new List<ClockData>();

        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        foreach (ClockController o in clockControllers)
        {
            ClockData a = new ClockData();
            a.isShootingClock = o.isShootingClock;
            a.clockType = o.clockType;
            a.arrowRoratingDirection = o.arrowRotatingDirection;
            a.arrowRotatingSpeed = o.arrowRotatingSpeed;
            a.position = o.transform.position;
            a.scale = o.transform.localScale;
            listClockData.Add(a);
        }

        levelData.levelNumber = levelNumber;
        levelData.getStarTime = FindObjectOfType<GameManager>().getStarTime;
        levelData.listClockData = listClockData;

        string data = JsonUtility.ToJson(levelData).Trim();
        return data;
    }

    /// <summary>
    /// Load level data by the given level number 
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns></returns>
    public static LevelData LoadLevelData(int levelNumber, string[] levelData)
    {
        for (int i = 0; i < levelData.Length - 1; i++)
        {
            CurrentLoadedLevelData = JsonUtility.FromJson<LevelData>(levelData[i]);
            if (CurrentLoadedLevelData.levelNumber == levelNumber)
            {
                return CurrentLoadedLevelData;
            }
        }
        return null;
    }
}
