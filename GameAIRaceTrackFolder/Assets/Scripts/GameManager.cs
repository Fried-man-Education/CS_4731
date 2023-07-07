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

    public enum SimulationMode
    {
        FPS_60_1X_RealTime,
        FPS_60_1X_SimTime,
    };

    [SerializeField]
    protected SimulationMode simulationMode = SimulationMode.FPS_60_1X_RealTime;

    public static SimulationMode? INTERNAL_overrideSimulationMode = null;

    private void Awake()
    {


    }

    // Start is called before the first frame update
    void Start()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;

        if (INTERNAL_overrideSimulationMode != null)
            simulationMode = (SimulationMode)INTERNAL_overrideSimulationMode;


        Debug.Log($"SimulationMode: {simulationMode}");


        switch (simulationMode)
        {
            case SimulationMode.FPS_60_1X_RealTime:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                break;
            case SimulationMode.FPS_60_1X_SimTime:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                Time.captureFramerate = 60;
                break;
        }

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
