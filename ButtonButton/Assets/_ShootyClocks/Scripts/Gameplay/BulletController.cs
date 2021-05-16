using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    public static event System.Action ShotMissed;

    [HideInInspector]
    public Vector2 movingDirection;
    [HideInInspector]
    public bool stop;

    private GameManager gameController;
    private Vector2 bulletPos;
    // Use this for initialization
    void Start()
    {
        gameController = FindObjectOfType<GameManager>();
        bulletPos = transform.position;
    }
	
    // Update is called once per frame
    void Update()
    {
        if (!stop)
        {
            bulletPos += movingDirection * gameController.bulletSpeed * Time.deltaTime;
            transform.position = bulletPos;

            Vector2 checkPos = Camera.main.WorldToViewportPoint(transform.position);
            if (checkPos.x < -0.1f || checkPos.x > 1.1f || checkPos.y < 0.1f || checkPos.y > 1.1f)
            {
                if (ShotMissed != null)
                {
                    ShotMissed();
                }

                gameController.listClocksData.Add(gameController.lastShootingClock);
                ClockController[] clocksController = FindObjectsOfType<ClockController>();
                foreach (ClockController o in clocksController)
                {
                    ClockData a = new ClockData();
                    a.position = o.transform.position;
                    a.scale = o.transform.localScale;
                    a.clockType = o.clockType;
                    a.isShootingClock = o.isShootingClock;
                    a.arrowRoratingDirection = o.arrowRotatingDirection;
                    a.arrowRotatingSpeed = o.arrowRotatingSpeed;
                    gameController.listClocksData.Add(a);
                }
          
                gameController.gameOver = true;
                gameController.passLevel = false;
                stop = true;
                Destroy(gameObject);

                StartCoroutine(CRPlayGameOverParticle(0.3f));
            }

        }
    }

    IEnumerator CRPlayGameOverParticle(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        Vector2 particlePos = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1));
        ParticleSystem par = Instantiate(gameController.gameOverParticle, particlePos, Quaternion.identity) as ParticleSystem;
        var main = par.main;
        par.Play();
        Destroy(par.gameObject, main.startLifetime.constant);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RightWall"))
        {
            movingDirection = Vector2.Reflect(movingDirection.normalized, Vector2.right);
        }
        else if (other.CompareTag("LeftWall"))
        {
            movingDirection = Vector2.Reflect(movingDirection.normalized, Vector2.left);
        }
        else if (other.CompareTag("TopWall"))
        {
            movingDirection = Vector2.Reflect(movingDirection.normalized, Vector2.down);
        }
        else if (other.CompareTag("BottomWall"))
        {
            movingDirection = Vector2.Reflect(movingDirection.normalized, Vector2.up);
        }
    }
}
