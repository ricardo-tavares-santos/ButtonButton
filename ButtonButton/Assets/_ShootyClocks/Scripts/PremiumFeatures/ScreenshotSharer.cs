using UnityEngine;
using System.Collections;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace SgLib
{
    public class ScreenshotSharer : MonoBehaviour
    {
        [Header("Sharing Config")]
        [Tooltip("Any instances of [score] will be replaced by the actual score achieved in the last game, [AppName] will be replaced by the app name declared in AppInfo")]
        [TextArea(3, 3)]
        public string questModeShareMessage = "Awesome! I've just passed level [level] in [AppName]! [#AppName]";
        [TextArea(3, 3)]
        public string endlessModeShareMessage = "Awesome! I've just scored [score] in [AppName]! [#AppName]";
        public string screenshotFilename = "screenshot.png";

        #if EASY_MOBILE
        public static ScreenshotSharer Instance { get; private set; }

        Texture2D capturedScreenshot;

        // On Android, we use a RenderTexture to take screenshot for better performance.
        #if UNITY_ANDROID
        RenderTexture screenshotRT;    
        #endif

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
            BulletController.ShotMissed += BulletController_ShotMissed;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
            BulletController.ShotMissed -= BulletController_ShotMissed;
        }

        void BulletController_ShotMissed()
        {
            if (GameManager.gameMode == GameMode.Endless)
            {
                CaptureScreenshot();
            }
        }

        void GameManager_GameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing && oldState == GameState.Prepare)
            {
                if (GameManager.gameMode == GameMode.Quest)
                {
                    CaptureScreenshot();
                }
            }
        }

        void Start()
        {
            #if UNITY_ANDROID
            screenshotRT = new RenderTexture(Screen.width, Screen.height, 24);
            #endif
        }

        public void CaptureScreenshot()
        {
            StartCoroutine(CRCaptureScreenshot());
        }

        IEnumerator CRCaptureScreenshot()
        {
            // Wait for right timing to take screenshot
            yield return new WaitForEndOfFrame();

            #if UNITY_ANDROID
            if (screenshotRT != null)
            {
            // Temporarily render the camera content to our screenshotRenderTexture.
            // Later we'll share the screenshot from this rendertexture.
            Camera.main.targetTexture = screenshotRT;
            Camera.main.Render();
            yield return null;
            Camera.main.targetTexture = null;
            yield return null;

            // Read the rendertexture contents
            RenderTexture.active = screenshotRT;

            capturedScreenshot = new Texture2D(screenshotRT.width, screenshotRT.height, TextureFormat.RGB24, false);
            capturedScreenshot.ReadPixels(new Rect(0, 0, screenshotRT.width, screenshotRT.height), 0, 0);
            capturedScreenshot.Apply();

            RenderTexture.active = null;
            }
            #else
            capturedScreenshot = Sharing.CaptureScreenshot();
            #endif
        }

        public Texture2D GetScreenshotTexture()
        {
            return capturedScreenshot;
        }

        public void ShareScreenshot()
        {
            if (capturedScreenshot == null)
            {
                Debug.Log("ShareScreenshot: FAIL. No captured screenshot.");
                return;
            } 

            string msg;
            if (GameManager.gameMode == GameMode.Quest)
            {
                msg = questModeShareMessage;
                msg = msg.Replace("[level]", GameManager.levelLoaded.ToString());
            }
            else
            {
                msg = endlessModeShareMessage;
                msg = msg.Replace("[score]", ScoreManager.Instance.Score.ToString());
            }

            msg = msg.Replace("[AppName]", AppInfo.Instance.APP_NAME);
            msg = msg.Replace("[#AppName]", "#" + AppInfo.Instance.APP_NAME.Replace(" ", ""));

            Sharing.ShareTexture2D(capturedScreenshot, screenshotFilename, msg);
        }

        #endif
    }
}
