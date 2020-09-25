using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Camera uiCamera;

    public Button createFishBtn;
    public Transform uiGunTrans;

    void Start()
    {
        FishManager.instance.Init();
        GunManager.instance.Init(uiCamera, uiGunTrans);
        BulletManager.instance.Init();

        

        createFishBtn.onClick.AddListener(() => 
        {
            var pathId = Random.Range(1, 4);
            FishManager.instance.CreateFish("Fish/FishModel.prefab", pathId + ".bytes");
        });
    }

    void Update()
    {
        FishManager.instance.Update();
        GunManager.instance.Update();
        BulletManager.instance.Update();
    }
}
