using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Data Config", order = 1)]
public class DataConfig : ScriptableObject
{
    public int sizeX;
    public int sizeY;
    public float animationDuration;
    [Tooltip("Must be correct to CellColor enum")]
    public Color[] cellColors; // Must be correct to CellColor enum
    public Sprite bombPowerUp;
    public Sprite colorPowerUp;

    public DataConfig()
    {
        sizeX = 7;
        sizeY = 9;
        animationDuration = 0.3f;
        cellColors = new Color[] { Color.red, Color.blue, new Color(0, 0.75f, 0), Color.yellow, new Color(1, 0.5f, 0), new Color(0.5f, 0, 0.5f) };
    }
}
