using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class RaceGameManager : MonoBehaviour
{
    public UIManager uiManager;
    public LapCounter LapCounter;
    public PlayerController playerController;
    public NPCController[] npcControllers;
    public AudioSource countdownSound;

    public float finishDelay = 1f;
    public int totalLaps = 3;

    private bool countdownFinished = false; // Flag to track countdown completion

    private void Start()
    {
        StartCoroutine(CountdownToStart());
        countdownSound.Stop();

        npcControllers = GameObject.FindGameObjectsWithTag("NPC")
            .Select(go => go.GetComponent<NPCController>())
            .ToArray();
    }

    private IEnumerator CountdownToStart()
    {
        uiManager.UpdateLapText(0, 3);
        uiManager.UpdateRaceStatus("Get Ready...");
        yield return new WaitForSeconds(1f);
        countdownSound.Play();
        uiManager.UpdateRaceStatus("3");
        yield return new WaitForSeconds(1f);
        uiManager.UpdateRaceStatus("2");
        yield return new WaitForSeconds(1f);
        uiManager.UpdateRaceStatus("1");
        yield return new WaitForSeconds(1f);
        uiManager.UpdateRaceStatus("Go!");
        yield return new WaitForSeconds(1f);

        countdownFinished = true; // Set the flag to indicate countdown completion
        StartRace();
    }

    private void StartRace()
    {
        playerController.EnableControl();

        // Enable control for all NPCs
        foreach (NPCController npcController in npcControllers)
        {
            if (npcController != null)
            {
                npcController.EnableControl();
            }
        }
    }

    private void Update()
    {
        if (countdownFinished) // Check if countdown has finished
        {
            uiManager.UpdateRaceStatus(LapCounter.GetPlayerPosition());
            uiManager.UpdateLapText(LapCounter.GetPlayerLap(), totalLaps);
        }

        EndRace();
    }

    private void EndRace()
    {
        if (playerController.playerLap == 3)
        {
            playerController.DisableControl();
        }

        // Enable control for all NPCs
        // foreach (NPCController npcController in npcControllers)
        // {
        //     if (npcController.npcLap == 3)
        //     {
        //         npcController.DisableControl();
        //     }
        // }

    }
}
