using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LapCounter : MonoBehaviour
{
    public PlayerController playerController;
    public NPCController[] npcControllers;

    private int waypointCount;
    private List<RaceParticipant> participants;

    private void Start()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        waypointCount = waypointObjects.Length;

        npcControllers = GameObject.FindGameObjectsWithTag("NPC")
            .Select(go => go.GetComponent<NPCController>())
            .ToArray();

        participants = new List<RaceParticipant>();
        participants.Add(new RaceParticipant("Player", playerController.playerLap, playerController.waypointCounter));

        foreach (NPCController npcController in npcControllers)
        {
            if (npcController != null)
            {
                participants.Add(new RaceParticipant(npcController.gameObject.name, npcController.npcLap, npcController.waypointCounter));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController.waypointCounter == waypointCount - 1)
            {
                playerController.playerLap++;
                playerController.waypointCounter = 0;
            }
        }
        else if (other.CompareTag("NPC"))
        {
            NPCController npcController = other.GetComponent<NPCController>();
            if (npcController != null && npcController.waypointCounter == waypointCount - 1)
            {
                npcController.npcLap++;
                npcController.waypointCounter = 0;
            }
        }

        UpdateRacePositions();
    }

    private void UpdateRacePositions()
    {
        participants.Clear();
        participants.Add(new RaceParticipant("Player", playerController.playerLap, playerController.waypointCounter));

        foreach (NPCController npcController in npcControllers)
        {
            if (npcController != null)
            {
                participants.Add(new RaceParticipant(npcController.gameObject.name, npcController.npcLap, npcController.waypointCounter));
            }
        }

        participants.Sort((x, y) =>
        {
            if (x.Lap != y.Lap)
                return y.Lap.CompareTo(x.Lap);
            else
                return y.WaypointCounter.CompareTo(x.WaypointCounter);
        });
    }

    private string GetPositionSuffix(int position, int totalParticipants)
    {
        if (position == 1)
        {
            return "st";
        }
        else if (position == 2)
        {
            return "nd";
        }
        else if (position == 3)
        {
            return "rd";
        }
        else if (position >= 4 && position <= totalParticipants)
        {
            return "th";
        }
        else
        {
            return "";
        }
    }
    
    public int GetPlayerLap()
    {
        return playerController.playerLap;
    }

    public string GetPlayerPosition()
    {
        int playerPosition = -1;
        int totalParticipants = participants.Count;

        for (int i = 0; i < participants.Count; i++)
        {
            if (participants[i].Name == "Player")
            {
                playerPosition = i + 1;
                break;
            }
        }

        if (playerPosition != -1)
        {
            return playerPosition.ToString() + GetPositionSuffix(playerPosition, totalParticipants);
        }
        else
        {
            return "";
        }
    }

    private class RaceParticipant
    {
        public string Name { get; private set; }
        public int Lap { get; private set; }
        public int WaypointCounter { get; private set; }

        public RaceParticipant(string name, int lap, int waypointCounter)
        {
            Name = name;
            Lap = lap;
            WaypointCounter = waypointCounter;
        }
    }
}
