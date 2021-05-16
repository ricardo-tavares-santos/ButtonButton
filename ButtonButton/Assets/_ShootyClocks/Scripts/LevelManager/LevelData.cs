using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{

    public int levelNumber;
    public float getStarTime;
    public List<ClockData> listClockData = new List<ClockData>();

}
