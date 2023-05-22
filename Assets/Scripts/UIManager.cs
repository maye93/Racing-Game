using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI raceStatusText;
    public TextMeshProUGUI lapText;

    public void UpdateRaceStatus(string text)
    {
        raceStatusText.text = text;
    }

    public void UpdateLapText(int currentLap, int totalLaps)
    {
        lapText.text = "Lap " + currentLap + "/" + totalLaps;
    }
}
