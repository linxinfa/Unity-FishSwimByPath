using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Button createFishBtn;

    void Start()
    {
        createFishBtn.onClick.AddListener(() => 
        {
            var pathId = Random.Range(1, 4);
            FishManager.instance.CreateFish("Fish/FishModel.prefab", pathId + ".bytes");
        });
    }

    void Update()
    {
        FishManager.instance.Update();
    }
}
