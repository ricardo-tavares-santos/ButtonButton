using UnityEngine;
using SgLib;
using UnityEngine.UI;

#if EASY_MOBILE
using EasyMobile;
#endif

public class LevelSelectionUI : MonoBehaviour
{
    public GameObject levelScrollView;
    public GameObject settingUI;
    public Button btnLevelMode;
    public Button btnSurvivalMode;
    public Button btnSoundOn;
    public Button btnSoundOff;
    public Button btnSetting;
    public Button btnLeaderboard;
    public Button btnAchievement;
    public Button btnExitLevelChooser;
    public Image imgStar;
    public Text txtStar;
    public Text txtTitle;

    [Header("Background Colors")]
    public Color[] colors;

    void Awake()
    {
        Camera.main.backgroundColor = colors[Random.Range(0, colors.Length - 1)];
    }

    // Use this for initialization
    void Start()
    {
        levelScrollView.SetActive(false);
        settingUI.SetActive(false);
		
		//???	
		if (!Advertising.IsInterstitialAdReady()) {
			Advertising.LoadInterstitialAd();
		}		
		if (!Advertising.IsRewardedAdReady()) {
			Advertising.LoadRewardedAd();
		}		
		if (!GameServices.IsInitialized()) {
			GameServices.ManagedInit();
		} 		
    }
	
    // Update is called once per frame
    void Update()
    {
        UpdateSoundButtons();
        txtStar.text = CoinManager.Instance.Coins.ToString();
    }

    public void HandleLevelModeButton()
    {
        levelScrollView.GetComponent<Image>().color = Camera.main.backgroundColor;
        levelScrollView.SetActive(true);
    }

    public void HandleExitLeveScrollView()
    {
        levelScrollView.SetActive(false);
    }

    public void HandleSurvivalModeButton()
    {
        GameManager.gameMode = GameMode.Endless;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void HandleSettingButton()
    {
        settingUI.GetComponent<Image>().color = Camera.main.backgroundColor;
        settingUI.SetActive(true);
    }

    public void HandleExitSettingsButton()
    {
        settingUI.SetActive(false);
    }

    public void HandleSoundButton()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void HandleMusicButton()
    {
        SoundManager.Instance.ToggleMusic();
    }


    void UpdateSoundButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            btnSoundOn.gameObject.SetActive(false);
            btnSoundOff.gameObject.SetActive(true);
        }
        else
        {
            btnSoundOn.gameObject.SetActive(true);
            btnSoundOff.gameObject.SetActive(false);
        }
    }

    public void ShowLeaderboardUI()
    {
        #if EASY_MOBILE
        if (GameServices.IsInitialized())
        {
            GameServices.ShowLeaderboardUI();
        }
        else
        {
            #if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
            #elif UNITY_ANDROID
            GameServices.Init();
            #endif
        }
        #endif
    }

    public void ShowAchievementUI()
    {
        #if EASY_MOBILE
        if (GameServices.IsInitialized())
        {
            GameServices.ShowAchievementsUI();
        }
        else
        {
            #if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
            #elif UNITY_ANDROID
            GameServices.Init();
            #endif
        }
        #endif
    }

    public void PurchaseRemoveAds()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
        #endif
    }

    public void RestorePurchase()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.RestorePurchase();
        #endif
    }
       
}
