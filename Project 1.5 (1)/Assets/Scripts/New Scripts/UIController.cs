using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class UIController : MonoBehaviour
{
    public GameObject UIRacePanel;

    public TMP_Text UITextCurrentLap;
    public TMP_Text UITextCurrentTime;
    public TMP_Text UITextLastLapTime;
    public TMP_Text UITextBestLapTime;

    public Player UpdateUIForPlayer;

    private int currentLap = -1;
    private float currentLapTime;
    private float lastLapTime;
    private float bestLapTime;

    void Update()
    {
        if(UpdateUIForPlayer == null)
            return;
        
        if(UpdateUIForPlayer.CurrentLap != currentLap)
        {
            currentLap = UpdateUIForPlayer.CurrentLap;
            UITextCurrentLap.text = $"Lap: {currentLap}";
        }

        if (UpdateUIForPlayer.CurrentLapTime != currentLapTime)
        {
            currentLapTime = UpdateUIForPlayer.CurrentLapTime;
            UITextCurrentTime.text = $"Time: {(int)currentLapTime / 60}:{(currentLapTime) % 60:00.000} ";
        }

        if (UpdateUIForPlayer.LastLapTime != lastLapTime)
        {
            lastLapTime = UpdateUIForPlayer.LastLapTime;
            UITextLastLapTime.text = $"Last: {(int)lastLapTime / 60}:{(lastLapTime) % 60:00.000} ";
        }

        if (UpdateUIForPlayer.BestLapTime != bestLapTime)
        {
            bestLapTime = UpdateUIForPlayer.BestLapTime;
            UITextBestLapTime.text = bestLapTime < 1000000 ? $"Best: {(int)bestLapTime / 60}:{(bestLapTime) % 60:00.000} " : "Best:--/--/---";
        }


    }
}
