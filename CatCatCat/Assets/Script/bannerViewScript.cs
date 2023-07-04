using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using System;

public class bannerViewScript : MonoBehaviour
{
    string adUnitId;
    private BannerView bannerView;


    public void Start() //±¤°í ÃÊ±âÈ­
    {
#if UNITY_ANDROID
        adUnitId = "key"; // 보완상 삭제
#elif UNITY_IOS
        adUnitId = "key";
#else
        adUnitId = "unexpected_platform";
#endif

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }
}
