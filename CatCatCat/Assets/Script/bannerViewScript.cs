using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using System;

public class bannerViewScript : MonoBehaviour
{
    string adUnitId;
    private BannerView bannerView;


    public void Start() //���� �ʱ�ȭ
    {
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IOS
        adUnitId = "ca-app-pub-3940256099942544~3347511713";
#else
        adUnitId = "unexpected_platform";
#endif

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }
}