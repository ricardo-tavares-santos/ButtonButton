using UnityEngine;
using System.Collections;
using SgLib;

public class ClockController : MonoBehaviour
{

    public GameObject circle;
    public GameObject arrow;
    public GameObject other;
    public ClockType clockType;

    [Header("Clock Config")]
    public ArrowRotatingDirection arrowRotatingDirection;
    [Range(100, 400)]
    public float arrowRotatingSpeed;
    // Mark the currently shooting clock, also the first clock to shoot when a level starts
    public bool isShootingClock;
    

    private GameManager gameController;
    private SpriteRenderer clockRenderer;
    private SpriteRenderer circleRenderer;
    private SpriteRenderer arrowRenderer;
    private SpriteRenderer otherRenderer;

    private bool isScalingUp;

    void Awake()
    {
        gameController = FindObjectOfType<GameManager>();

        //Get renderer
        clockRenderer = GetComponent<SpriteRenderer>();
        circleRenderer = circle.GetComponent<SpriteRenderer>();
        arrowRenderer = arrow.GetComponent<SpriteRenderer>();
        otherRenderer = other.GetComponent<SpriteRenderer>();

    }

    void OnTriggerEnter2D(Collider2D other)
    {        
        if (!isShootingClock)
        {
            ClockController[] clockControllers = FindObjectsOfType<ClockController>();
            if (clockControllers.Length == 1)
            {
                if (gameController.gameEnvironment == GameEnvironment.PLAY)
                {
                    gameController.CheckAndSaveLevel(GameManager.LEVEL_SOLVED);
                    if (!gameController.gameplayUI.stopCountTime)
                    {
                        SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.passLevelWithStar);
                        gameController.CheckAndSaveLevel(GameManager.LEVEL_HAS_STAR);
                        gameController.gameplayUI.isCompleteWithStar = true;

                        Vector2 particlePos = new Vector2(0, 0);
                        ParticleSystem par = Instantiate(gameController.passLevelWithStarParticle, particlePos, Quaternion.identity) as ParticleSystem;
                        var main = par.main;
                        par.Play();
                        Destroy(par.gameObject, main.startLifetime.constant);
                    }
                    else
                    {
                        SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.passLevel);
                        gameController.gameplayUI.isCompleteWithStar = false;
                        Vector2 particlePos = new Vector2(0, 0);
                        ParticleSystem par = Instantiate(gameController.passLevelParticle, particlePos, Quaternion.identity) as ParticleSystem;
                        var main = par.main;
                        par.Play();
                        Destroy(par.gameObject, main.startLifetime.constant);
                    }
                
                    gameController.gameplayUI.stopCountTime = true;
                }
                else //On level editor environment
                {
                    //FindObjectOfType<LevelEditorUI>()
                }
                gameController.passLevel = true;
                gameController.gameOver = false;
                gameController.walls.SetActive(false);
                Destroy(gameObject);
                Destroy(other.gameObject);
                return;
            }

            //Handle score and sounds 
            if (gameController.gameEnvironment == GameEnvironment.PLAY)
            {
                // Only count score in endless mode
                if (GameManager.gameMode == GameMode.Endless)
                    ScoreManager.Instance.AddScore(1);

                if (clockType == ClockType.NORMAL)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitNormalClock);
                else
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitUnNormalClock);                
            }
            
            isShootingClock = true;

            //Add this clock data for list (use for retry last shoot feature)
            ClockData clockData = new ClockData();
            clockData.clockType = clockType;
            clockData.arrowRoratingDirection = arrowRotatingDirection;
            clockData.arrowRotatingSpeed = arrowRotatingSpeed;
            clockData.isShootingClock = isShootingClock;
            clockData.position = transform.position;
            clockData.scale = transform.localScale;
            gameController.lastShootingClock = clockData;

            UpdateClockState();
            if (!isScalingUp)
                StartCoroutine(ScaleBounce());
            if (clockType == ClockType.FILL_GRID)
                gameController.HandleFillGridEvent();
            else if (clockType == ClockType.BIG_BULLET)
                gameController.HandleBigBulletEvent();
            else if (clockType == ClockType.SPEED_DOWN)
                gameController.HandleChangeSpeedEvent(true);
            else if (clockType == ClockType.SPEED_UP)
                gameController.HandleChangeSpeedEvent(false);
            else if (clockType == ClockType.WALL)
                gameController.HandleWallEvent();
            Destroy(other.gameObject);
        }    
    }

    public void UpdateClockState()
    {
        if (isShootingClock)
        {
            other.SetActive(false);
            arrow.SetActive(true);
            circle.SetActive(true);
            clockRenderer.color = Color.black;
            circleRenderer.color = Color.white;
            arrowRenderer.color = Color.white;
            arrow.transform.rotation = other.transform.rotation;
            StartCoroutine(Rotate(arrow));
            StartCoroutine(WaitAndGetClockData());
        }
        else
        {
            arrow.SetActive(false);
            circle.SetActive(false);
            other.SetActive(true);
            clockRenderer.color = Color.white;
            otherRenderer.color = gameController.color;
            if (clockType == ClockType.NORMAL)
                StartCoroutine(Rotate(other));
        }

        
    }

    //Rotate the given object
    IEnumerator Rotate(GameObject rotatingObject)
    {
        int turn = (arrowRotatingDirection == ArrowRotatingDirection.CLOCKWISE) ? (-1) : (1);
        while (true)
        {
            rotatingObject.transform.eulerAngles += new Vector3(0, 0, turn * arrowRotatingSpeed * Time.deltaTime);
            yield return null;
        }
    }


    IEnumerator WaitAndGetClockData()
    {
        yield return new WaitForSeconds(gameController.clockScaleUpTime);
        ClockData lastShoottingClock = new ClockData();
        lastShoottingClock.position = transform.position;
        lastShoottingClock.scale = transform.localScale;
        lastShoottingClock.clockType = clockType;
        lastShoottingClock.isShootingClock = true;
        lastShoottingClock.arrowRoratingDirection = arrowRotatingDirection;
        lastShoottingClock.arrowRotatingSpeed = arrowRotatingSpeed;
        gameController.lastShootingClock = lastShoottingClock;
    }


    //Get the bullet position(top of the arrow)
    public Vector2 BulletPosition()
    {
        return arrow.transform.GetChild(0).transform.position;
    }

    //Get the shooting direction
    public Vector2 ShootingDirection()
    {
        return (arrow.transform.GetChild(0).transform.position - arrow.transform.position).normalized;
    }


    public void ScaleDownAndDestroy()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleDown());
    }
    //Scale the clock down and destroy
    IEnumerator ScaleDown()
    {
        Vector2 startScale = transform.localScale;
        Vector2 endScale = new Vector2(0, 0);
        float t = 0;
        while (t < gameController.clockScaleDownTime)
        {
            t += Time.deltaTime;
            float fraction = t / gameController.clockScaleDownTime;
            transform.localScale = Vector2.Lerp(startScale, endScale, fraction);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void ScaleClockUp()
    {
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        isScalingUp = true;
        Vector2 startScale = new Vector2(0, 0);
        Vector2 endScale = transform.localScale;
        float t = 0;
        while (t < gameController.clockScaleUpTime)
        {
            t += Time.deltaTime;
            float fraction = t / gameController.clockScaleUpTime;
            transform.localScale = Vector2.Lerp(startScale, endScale, fraction);
            yield return null;
        }
        isScalingUp = false;
    }

    IEnumerator ScaleBounce()
    {
        Vector2 scaleFactor = transform.localScale / 10f;
        Vector2 startScale = transform.localScale;
        Vector2 endScale = startScale + scaleFactor;
        float t = 0;
        while (t < gameController.clockScaleBounceTime)
        {
            t += Time.deltaTime;
            float fraction = t / gameController.clockScaleBounceTime;
            transform.localScale = Vector2.Lerp(startScale, endScale, fraction);
            yield return null;
        }

        float r = 0;
        while (r < gameController.clockScaleBounceTime)
        {
            r += Time.deltaTime;
            float fraction = r / gameController.clockScaleBounceTime;
            transform.localScale = Vector2.Lerp(endScale, startScale, fraction);
            yield return null;
        }
    }

}
