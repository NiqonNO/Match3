using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Data Config", order = 1)]
public class DataConfig : ScriptableObject
{
    public int sizeX;
    public int sizeY;
    public float animationDuration;
    [Tooltip("Bomb spawn chanse in precent")]
    [SerializeField] float bombChanse;
    [Tooltip("Color Seaker spawn chanse in precent")]
    [SerializeField] float colorSeakerChanse;
    public Color[] cellColors;
    public Sprite bombPowerUp;
    public Sprite colorPowerUp;

    public float GetStandardCellChance { get { return 100 - bombChanse - colorSeakerChanse; } }
    public float GetBombCellChance { get { return 100 - colorSeakerChanse; } }

    public DataConfig()
    {
        sizeX = 7;
        sizeY = 9;
        animationDuration = 0.3f;
        bombChanse = 5;
        colorSeakerChanse = 5;
        cellColors = new Color[] { Color.red, Color.blue, new Color(0, 0.75f, 0), Color.yellow, new Color(1, 0.5f, 0), new Color(0.5f, 0, 0.5f) };
    }

    private void OnValidate()
    {
        //Keep values below 100
        float chanseSum = bombChanse + colorSeakerChanse;
        if (chanseSum >= 100)
        {
            bombChanse = bombChanse / chanseSum * 99;
            colorSeakerChanse = colorSeakerChanse / chanseSum * 99;
        }
    }
}