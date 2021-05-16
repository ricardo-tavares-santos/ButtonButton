using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    LevelCompleted,
    PreGameOver,
    GameOver
}

public enum GameMode
{
    Quest,
    Endless
}

public enum GameEnvironment
{
    PLAY,
    LEVEL_EDITOR
}

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    public static int GameCount
    { 
        get { return _gameCount; } 
        private set { _gameCount = value; } 
    }

    private static int _gameCount = 0;

    [Header("Check to enable premium features (require EasyMobile plugin)")]
    public bool enablePremiumFeatures = true;

    [Header("Set the target frame rate for this game")]
    [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
    public int targetFrameRate = 30;

    [Header("Set game environment")]
    public GameEnvironment gameEnvironment = GameEnvironment.PLAY;

    public const string LEVEL_SOLVED = "LevelSolved";
    public const string LEVEL_HAS_STAR = "LevelHasStar";
    public const string MAX_LEVEL_SOLVED = "MaxLevelSolved";
    public static int levelLoaded = 1;
    public static GameMode gameMode = GameMode.Endless;


    [Header("Object References")]
    public GameplayUI gameplayUI;
    public GameObject bulletPrefab;
    public GameObject normalClockPrefab;
    public GameObject bigBulletClockPrefab;
    public GameObject fillGridClockPrefab;
    public GameObject speedUpClockPrefab;
    public GameObject speedDownClockPrefab;
    public GameObject wallClockPrefab;
    public GameObject walls;
    public ParticleSystem passLevelWithStarParticle;
    public ParticleSystem passLevelParticle;
    public ParticleSystem gameOverParticle;

    [HideInInspector]
    public Color color;
    [HideInInspector]
    public bool gameOver;
    [HideInInspector]
    public bool passLevel;
    [HideInInspector]
    public float getStarTime;
    [HideInInspector]
    public bool stopCheck;
    [HideInInspector]
    public List<ClockData> listClocksData = new List<ClockData>();
    //Use for retry last shoot
    [HideInInspector]
    public List<Vector2> listClockPosition = new List<Vector2>();
    [HideInInspector]
    public ClockData lastShootingClock;

    [Header("Gameplay Config")]

    public float bulletSpeed = 10;
    public float bigBulletScaleUp = 3;
    public int maxClockNumber = 15;
    public float minRotatingSpeed = 50;
    public float maxRotatingSpeed = 300;
    public int bigBulletTime = 5;
    public int showWallsTime = 10;
    public float clockScaleUpTime = 0.3f;
    public float clockScaleDownTime = 0.3f;
    public float clockScaleBounceTime = 0.1f;
    [Range(0, 1)]
    public float unNormalClockFrequency = 0.5f;
    [Range(0, 1)]
    public float bigBulletClockFrequency = 0.5f;
    [Range(0, 1)]
    public float fillGridClockFrequency = 0.5f;
    [Range(0, 1)]
    public float speedUpClockFrequency = 0.5f;
    [Range(0, 1)]
    public float speedDownClockFrequency = 0.5f;
    [Range(0, 1)]
    public float wallClockFrequency = 0.5f;
    public Color[] colors;
    public string[] passLevelWithStarTexts = { "Fantastic!!!", "Incredible!!!", "Impressive!!", "Excellent!!!", "Perfect!!!", "Beautiful!!!" };
    public string[] passLevelTexts = { "Well Done!!!", "Good Job!!!", "Nice!!!", "Alright!!!", "Not Bad!!!" };
    public string[] gameOverTexts = { "Keep Trying!!!", "Ouch!!!", "Errr!!!", "Be Careful!!!", "No!!!" };
    
    private float maxClockScale = 0.7f;
    //Use for Fill_Grid event
    private float minClockScale = 0.4f;
    private bool isBigBulletClock;
    private bool allowShooting;
    private int bigBulletTimeTemp;

    void Awake()
    {
        if (gameEnvironment == GameEnvironment.PLAY)
        {
            color = colors[Random.Range(0, colors.Length - 1)];
        }
    }

    void Start()
    {
        GameState = GameState.Prepare;

        walls.SetActive(false);
        StartCoroutine(WaitForClockScaleUp());

        if (gameEnvironment == GameEnvironment.LEVEL_EDITOR)
        {
            color = Camera.main.backgroundColor;
            ClockController[] clocksController = FindObjectsOfType<ClockController>();
            foreach (ClockController o in clocksController)
            {
                o.UpdateClockState();
            }
            return;
        }
           
        SgLib.ScoreManager.Instance.Reset();                
        Camera.main.backgroundColor = color;

        if (gameEnvironment == GameEnvironment.PLAY)
        {
            if (gameMode == GameMode.Quest) // Level mode
            {
                LevelData currentLevelData = LevelManager.LoadLevelData(levelLoaded, LevelScroller.data);

                //Display get star time
                getStarTime = currentLevelData.getStarTime;
                foreach (ClockData o in currentLevelData.listClockData)
                {
                    CreateClockByClockData(o);
                }
            }
            else // Endless mode
            {
                //Get random list clock position
                while (listClockPosition.Count < maxClockNumber)
                {
                    Vector2 pos = transform.GetChild(Random.Range(0, transform.childCount)).position;
                    if (!listClockPosition.Contains(pos))
                        listClockPosition.Add(pos);
                }

                //Create clocks base on the list
                for (int i = 0; i < maxClockNumber; i++)
                {
                    CreateClockByPosition(listClockPosition[i], GetRandomClockPrefab());
                }

                //Random a shooting clock
                RandomAShootingClock();
            }

            GameState = GameState.Playing;
        }
        else //On editor mode
        {
            ClockController[] clocksController = FindObjectsOfType<ClockController>();
            foreach (ClockController o in clocksController)
            {
                o.UpdateClockState();
            }
        }
    }

    void Update()
    {
        if (gameOver && !stopCheck)
        {
            stopCheck = true;
            GameState = GameState.GameOver;

            if (gameEnvironment == GameEnvironment.PLAY)
                SgLib.SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.gameOver);

            ClockController[] clocksController = FindObjectsOfType<ClockController>();
            foreach (ClockController o in clocksController)
            {
                o.ScaleDownAndDestroy();
            }
        }
        else if (passLevel && !stopCheck)
        {
            stopCheck = true;
            GameState = GameState.LevelCompleted;
        }
    }

    public void HandleShootButton()
    {

        if (!allowShooting)
            return;

        //Find all clocks
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        foreach (ClockController clockController in clockControllers)
        {
            //Find the shooting clock
            if (clockController.isShootingClock)
            {
                //Create bullet
                GameObject bullet = Instantiate(bulletPrefab, clockController.BulletPosition(), Quaternion.identity) as GameObject;
                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.movingDirection = clockController.ShootingDirection();

                //Is on big bullet event
                if (!isBigBulletClock)
                {
                    bigBulletTimeTemp = bigBulletTime;
                }
                else
                {
                    if (bigBulletTimeTemp == 0)
                        isBigBulletClock = false;
                    else
                    {
                        bigBulletTimeTemp--;
                        bullet.transform.localScale *= bigBulletScaleUp;
                    }
                }

                //On survical mode
                if (gameMode == GameMode.Endless && gameEnvironment == GameEnvironment.PLAY)
                {
                    if (clockControllers.Length < maxClockNumber)
                        CreateClockByPosition(GetRandomClockPosition(), GetRandomClockPrefab());

                    if (listClockPosition.Contains(clockController.transform.position))
                        listClockPosition.Remove(clockController.transform.position);

                }
                Destroy(clockController.gameObject);

                break;
            }
        }
    }

    IEnumerator WaitForClockScaleUp()
    {
        yield return new WaitForSeconds(clockScaleUpTime);
        allowShooting = true;
    }

   
    GameObject GetClock(ClockType clockType)
    {
        switch (clockType)
        {
            case ClockType.BIG_BULLET:
                return bigBulletClockPrefab;
            case ClockType.FILL_GRID:
                return fillGridClockPrefab;
            case ClockType.SPEED_DOWN:
                return speedDownClockPrefab;
            case ClockType.SPEED_UP:
                return speedUpClockPrefab;
            case ClockType.WALL:
                return wallClockPrefab;
            default:
                return normalClockPrefab;
        }
    }

    /// <summary>
    /// Create clock by clock data
    /// </summary>
    /// <param name="clockData"></param>
    public void CreateClockByClockData(ClockData o)
    {
        GameObject clock = Instantiate(GetClock(o.clockType), o.position, Quaternion.identity) as GameObject;
        clock.transform.localScale = o.scale;
        ClockController clockController = clock.GetComponent<ClockController>();
        clockController.arrowRotatingDirection = o.arrowRoratingDirection;
        clockController.arrowRotatingSpeed = o.arrowRotatingSpeed;
        clockController.isShootingClock = o.isShootingClock;
        if (gameEnvironment == GameEnvironment.PLAY)
            clockController.UpdateClockState();
    }


    //Get a random clock
    GameObject GetRandomClockPrefab()
    {
        if (Random.value >= unNormalClockFrequency)
        {
            return normalClockPrefab;
        }
        else
        {
            int value = Random.Range(1, 6);
            if (value == 1)
            {
                if (Random.value <= bigBulletClockFrequency)
                    return bigBulletClockPrefab;
                else
                    return normalClockPrefab;
            }
            else if (value == 2)
            {
                if (Random.value <= fillGridClockFrequency)
                    return fillGridClockPrefab;
                else
                    return normalClockPrefab;
            }
            else if (value == 3)
            {
                if (Random.value <= speedDownClockFrequency)
                    return speedDownClockPrefab;
                else
                    return normalClockPrefab;
            }
            else if (value == 4)
            {
                if (Random.value <= speedUpClockFrequency)
                    return speedUpClockPrefab;
                else
                    return normalClockPrefab;
            }
            else
            {
                if (Random.value <= wallClockFrequency)
                    return wallClockPrefab;
                else
                    return normalClockPrefab;
            }
        }
    }

    Vector2 GetRandomClockPosition()
    {
        Vector2 pos = transform.GetChild(Random.Range(0, transform.childCount)).transform.position;
        while (listClockPosition.Contains(pos))
        {
            pos = transform.GetChild(Random.Range(0, transform.childCount)).transform.position;
        }
        listClockPosition.Add(pos);
        return pos;
    }

    //Create clock with given position
    void CreateClockByPosition(Vector2 pos, GameObject clockPrefab)
    {
        GameObject clock = Instantiate(clockPrefab, pos, Quaternion.identity) as GameObject;
        float scale = Random.Range(minClockScale, maxClockScale);
        clock.transform.localScale = new Vector3(scale, scale);
        ClockController clockController = clock.GetComponent<ClockController>();
        clockController.arrowRotatingDirection = (Random.value <= 0.5f) ? ArrowRotatingDirection.CLOCKWISE : ArrowRotatingDirection.COUNTER_CLOCKWISE;
        clockController.arrowRotatingSpeed = Random.Range(minRotatingSpeed, maxRotatingSpeed);
        clockController.ScaleClockUp();
        clockController.UpdateClockState();
    }

    void RandomAShootingClock()
    {
        //Random a shooting clock
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        ClockController clockController = clockControllers[Random.Range(0, clockControllers.Length)];
        clockController.isShootingClock = true;
        clockController.UpdateClockState();
    }


    //Use for FillGird_Clock
    public void HandleFillGridEvent()
    {
        if (gameMode == GameMode.Endless)//On survival mode;
            listClockPosition.Clear();

        //Find all clocks and destroy them
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        foreach (ClockController clockController in clockControllers)
        {
            Destroy(clockController.gameObject);
        }

        //Create clock grid
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector2 pos = transform.GetChild(i).transform.position;
            CreateClockByPosition(pos, normalClockPrefab);

            // On survival/endless mode
            if (gameMode == GameMode.Endless)
                listClockPosition.Add(pos);
        }

        StartCoroutine(WaitAndRandomAShootingClock());
    }

    IEnumerator WaitAndRandomAShootingClock()
    {
        yield return new WaitForSeconds(clockScaleUpTime);
        RandomAShootingClock();
    }

    //Use for SpeedUp and SpeedDown clocks
    public void HandleChangeSpeedEvent(bool isTurnDown)
    {
        //Turn down arrow rotating speed
        ClockController[] clockControllers = FindObjectsOfType<ClockController>();
        foreach (ClockController clockController in clockControllers)
        {
            clockController.arrowRotatingSpeed = (isTurnDown) ? (minRotatingSpeed) : (maxRotatingSpeed);
        }
    }

    //Use for BigBullet clock
    public void HandleBigBulletEvent()
    {
        isBigBulletClock = true;
    }

    //Use for Wall clock
    public void HandleWallEvent()
    {
        StartCoroutine(ShowWalls());
    }

    IEnumerator ShowWalls()
    {
        walls.SetActive(true);
        yield return new WaitForSeconds(showWallsTime);
        walls.SetActive(false);
    }

    public void CheckAndSaveLevel(string playerPrefsKey)
    {
        bool isLevelExist = false;
        string[] data = PlayerPrefs.GetString(playerPrefsKey).Split(';');
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (int.Parse(data[i]) == levelLoaded)
            {
                isLevelExist = true;
                break;
            }
        }

        if (!isLevelExist)
        {
            string dataOverride = PlayerPrefs.GetString(playerPrefsKey)
                                  + levelLoaded.ToString() + ";";
            PlayerPrefs.SetString(playerPrefsKey, dataOverride);

            if (playerPrefsKey.Equals(LEVEL_HAS_STAR))
                SgLib.CoinManager.Instance.AddCoins(1);
        }

        if (playerPrefsKey.Equals(LEVEL_SOLVED))
        {
            data = PlayerPrefs.GetString(playerPrefsKey).Split(';');
            int maxLevelSolved = 1;
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (int.Parse(data[i]) > maxLevelSolved)
                    maxLevelSolved = int.Parse(data[i]);
            }
            PlayerPrefs.SetInt(MAX_LEVEL_SOLVED, maxLevelSolved);
        }
    }
}
