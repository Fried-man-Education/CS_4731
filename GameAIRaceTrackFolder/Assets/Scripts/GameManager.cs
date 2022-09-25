using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameManager : Singleton<GameManager>
{
    protected GameManager() { }

    public TextMeshProUGUI StudentNameTMP;
    public TextMeshProUGUI MetersPerSecTMP;
    public TextMeshProUGUI MetersPerSecLTATMP;
    public TextMeshProUGUI TotalMetersTMP;
    public TextMeshProUGUI ElapsedTMP;
    public TextMeshProUGUI WipeoutsTMP;

    public int Wipeouts = 0;
    public float KpHLTA = 0f;
    public float MetersTravelled = 0f;
    public float MinThrottle = float.MaxValue;
    public float MaxThrottle = 0f;

    private void Awake()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;

    }

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }
}
