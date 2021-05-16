using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class LevelEditor : EditorWindow
{

    private LevelManager levelManager;
    public float getStarTime;
    private int totalLevel;
    private bool createNewLevel = true;
    private bool loadExistingLevel = false;
    private bool groupDisable = false;
    private int levelNumber = 1;
    private float smallButtonWidth = 40f;
    private float controlHeight = 26f;
    private int screenshotWidth = 213;
    private int screenshotHeight = 378;

    private const string LevelEditorScenePath = "Assets/_ShootyClocks/Scenes/LevelEditor.unity";
    //Show window editor
    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        // Ask for a change scene confirmation if not on level editor scene
        if (!EditorSceneManager.GetActiveScene().path.Equals(LevelEditorScenePath))
        {
            if (EditorUtility.DisplayDialog(
                    "Open Level Editor",
                    "Do you want to close the current scene and open LevelEditor scene? Unsaved changes in this scene will be discarded.", "Yes", "No"))
            {
                EditorSceneManager.OpenScene(LevelEditorScenePath);
                GetWindow(typeof(LevelEditor));
            }
        }
        else
        {
            GetWindow(typeof(LevelEditor));
        }
    }

    void Update()
    {
        // Check if is in LevelEditor scene.
        Scene activeScene = EditorSceneManager.GetActiveScene();

        // Auto exit if not in level editor scene.
        if (!activeScene.path.Equals(LevelEditorScenePath))
        {
            this.Close();
            return;
        }
    }

    void OnGUI()
    {

        //Find level controller
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        totalLevel = levelManager.GetTotalLevelNumber();

        // Disable the whole editor window if the game is in playing mode
        EditorGUI.BeginDisabledGroup(Application.isPlaying);

        EditorGUILayout.BeginVertical();

        //Show total level
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("TOTAL LEVEL: " + totalLevel.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.Space();


        ////////////////////////////////////
        // Create new level section
        EditorGUI.BeginDisabledGroup(groupDisable);
        EditorGUI.BeginChangeCheck();


        createNewLevel = EditorGUILayout.BeginToggleGroup("Create New Level", createNewLevel);
        EditorGUILayout.LabelField("The Next Level: " + (totalLevel + 1).ToString());

        getStarTime = EditorGUILayout.FloatField("Time To Get Star:", getStarTime);
        FindObjectOfType<GameManager>().getStarTime = getStarTime;


        if (GUILayout.Button("Clear Scene", GUILayout.Height(controlHeight)))
        {
            levelManager.ClearScene();
        }
        if (GUILayout.Button("Save Level", GUILayout.Height(controlHeight)))
        {
            if (levelManager.AllowSaveLevel())
            {
                levelManager.SaveLevel(totalLevel + 1);
                levelManager.TakeScreenshot(totalLevel + 1);
                EditorUtility.DisplayDialog("Level Saved!!!", "Level " + (totalLevel + 1).ToString() + " is saved!!!", "OK");
            }
            else
                EditorUtility.DisplayDialog("Level unsaved!!!",
                    "Please make sure that the level has more than one clock and has just one shooting clock!!!", "OK");
        }
       

        if (EditorGUI.EndChangeCheck())
        {
            loadExistingLevel = !createNewLevel;
        }
        EditorGUI.EndDisabledGroup();

        ////////////////////////////////////////////
        //Load existing level

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        loadExistingLevel = EditorGUILayout.BeginToggleGroup("Load Existing Level", loadExistingLevel);

        if (levelNumber > 0 && levelNumber <= totalLevel)
        {
            ShowScreenshot(levelNumber, levelManager);
        }
        else
        {
            if (totalLevel <= 0)
            {
                EditorGUILayout.HelpBox("You don't have any level.", MessageType.Warning, true);
            }
            else
            {
                EditorGUILayout.HelpBox("Level not found! Please enter a valid level number.", MessageType.Error, true);
            }
        }


        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(levelNumber <= 1);

        if (GUILayout.Button("←", GUILayout.Width(smallButtonWidth), GUILayout.Height(controlHeight)))
        {
            levelNumber--;
            getStarTime = levelManager.GetStarTimeOfLevel(levelNumber);
            GUI.FocusControl(null);          
        }
        EditorGUI.EndDisabledGroup();

        //Show current level 
        levelNumber = EditorGUILayout.IntField(levelNumber);       

        //Load next level
        EditorGUI.BeginDisabledGroup(levelNumber >= totalLevel);
        if (GUILayout.Button("→", GUILayout.Width(smallButtonWidth), GUILayout.Height(controlHeight)))
        {
            levelNumber++;
            getStarTime = levelManager.GetStarTimeOfLevel(levelNumber);
            GUI.FocusControl(null);         
        }
        EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck())
        {
            if (levelNumber > 0 && levelNumber <= totalLevel)
            {
                levelManager.LoadLevel(levelNumber);
            }
        }
        EditorGUILayout.EndHorizontal();

        //Show time to get star of the level
        getStarTime = EditorGUILayout.FloatField("Time To Get Star:", getStarTime);
        FindObjectOfType<GameManager>().getStarTime = getStarTime;

        EditorGUILayout.Space();
        if (GUILayout.Button("Overwrite Level", GUILayout.Height(controlHeight)))
        {
            // Ask for confirmation
            if (EditorUtility.DisplayDialog(
                    "Overwrite Level?",
                    "Are you sure you want to overwrite this level?",
                    "Yes, overwrite it",
                    "No"))
            {
                if (levelNumber < 1 || levelNumber > totalLevel)
                    EditorUtility.DisplayDialog("Not Overwritten!", "Level number doesn't exist!!!", "OK");
                else if (!levelManager.AllowSaveLevel())
                {
                    EditorUtility.DisplayDialog("Level unsaved!!!",
                        "Please make sure that the level has more than one clock and has just one shooting clock!!!", "OK");
                }
                else
                {
                    levelManager.OverwriteLevel(levelNumber);
                    EditorUtility.DisplayDialog("Level Overwritten!", "Level " + levelNumber.ToString() + " was updated!", "OK");
                }
            }
        }

        EditorGUILayout.EndToggleGroup();
        if (EditorGUI.EndChangeCheck())
        {
            createNewLevel = !loadExistingLevel;
        }

        EditorGUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();
    }


    void ShowScreenshot(int levelNumber, LevelManager levelController)
    {
        Texture tex = EditorGUIUtility.Load(levelController.ScreenshotPath(levelNumber)) as Texture;
        if (tex != null)
        {
            Rect rect = GUILayoutUtility.GetRect(screenshotWidth, screenshotHeight);
            GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
        }    
    }
}
