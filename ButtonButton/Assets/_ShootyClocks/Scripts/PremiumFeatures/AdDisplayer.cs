using UnityEngine;
using System.Collections;
using System;


using EasyMobile;


namespace SgLib
{
    public class AdDisplayer : MonoBehaviour
    {
       
        public static AdDisplayer Instance { get; private set; }

        [Header("Banner Ad Display Config")]
        [Tooltip("Whether or not to show banner ad")]
        public bool showBannerAd = true;
        public BannerAdPosition bannerAdPosition = BannerAdPosition.Bottom;

        [Header("Interstitial Ad Display Config")]
        [Tooltip("Whether or not to show interstitial ad")]
        public bool showInterstitialAd = true;
        [Tooltip("Show interstitial ad every [how many] games")]
        public int gamesPerInterstitial = 3;
        [Tooltip("How many seconds after game over that interstitial ad is shown")]
        public float showInterstitialDelay = 2f;

        [Header("Rewarded Ad Display Config")]
        [Tooltip("Check to allow watching ad to retry last shot")]
        public bool useRewardedAd = true;
        [Tooltip("Minimum time (minutes) to wait until serving the next rewarded ad")]
        public float rewardedAdLimitTime = 1;

        private static int gameCount = 0;
        private const string LAST_REWARDED_AD_TIME_PPK = "SGLIB_LAST_REWARDED_AD_TIME";

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

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

        void Start()
        {
			//???	
			if (!Advertising.IsInterstitialAdReady()) {
				Advertising.LoadInterstitialAd();
			}		
			if (!Advertising.IsRewardedAdReady()) {
				Advertising.LoadRewardedAd();
			}			
			
            // Show banner ad
            if (showBannerAd && !Advertising.IsAdRemoved())
            {
                Advertising.ShowBannerAd(bannerAdPosition);
            }
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {       
            if (newState == GameState.GameOver || newState == GameState.LevelCompleted)
            {
                // Show interstitial ad
                if (showInterstitialAd)//??? && !Advertising.IsAdRemoved())
                {
                    gameCount++;

                    if (gameCount >= gamesPerInterstitial)
                    {
                        if (Advertising.IsInterstitialAdReady())
                        {
                            // Show default ad after some optional delay
                            StartCoroutine(ShowInterstitial(showInterstitialDelay));

                            // Reset game count
                            gameCount = 0;
                        }
                    }
                }
            }
        }

        IEnumerator ShowInterstitial(float delay = 0f)
        {        
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            Advertising.ShowInterstitialAd();
        }

        public bool IsRewardedAdLimitTimePast()
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            DateTime lastTime = Utilities.GetTime(LAST_REWARDED_AD_TIME_PPK, epoch);
            TimeSpan timePast = DateTime.Now.Subtract(lastTime);
            return timePast.Minutes >= 1; //??? rewardedAdLimitTime;
        }

        public bool CanShowRewardedAd()
        {
			if (!Advertising.IsRewardedAdReady()) {
				Advertising.LoadRewardedAd();
			}			
            return Advertising.IsRewardedAdReady() && IsRewardedAdLimitTimePast();
        }

        public void ShowRewardedAd()
        {
            if (CanShowRewardedAd())
            {
                Advertising.ShowRewardedAd();
                Utilities.StoreTime(LAST_REWARDED_AD_TIME_PPK, DateTime.Now); 
            }
        }           
       
    }
}
