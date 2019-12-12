using UnityEngine;

public struct Coordinates
{
    public int x;
    public int y;

    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public float DistanceTo(Coordinates point)
    {
        return Mathf.Sqrt(Mathf.Pow((point.x - x), 2) + Mathf.Pow((point.y - y), 2));
    }
}
