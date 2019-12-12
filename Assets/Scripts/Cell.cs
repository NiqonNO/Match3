﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    GridManager gm;
    [SerializeField] SpriteRenderer colorsr;
    [SerializeField] SpriteRenderer typesr;
    [SerializeField] SpriteRenderer selectionsr;
    [HideInInspector] public int color;
    [HideInInspector] public CellType type;
    [HideInInspector] public Coordinates coordinates;
    int moveDistance = 0; //distance to move when tweening position

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        gm.SelectCell(this);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < 8; i++)
        {
            Cell neighbor = GetNeighbor((Direction)i);
            if (neighbor!= null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }

    public void Initialize(GridManager gm, Coordinates coordinates)
    {
        this.gm = gm;
        this.coordinates = coordinates;
        SetColor(Random.Range(0, gm.data.cellColors.Length));
        float rand = Random.Range(0f, 100f);
        SetType(rand < gm.data.GetStandardCellChance ? 0 : rand < gm.data.GetBombCellChance ? 1 : 2);
    }
    public void ReInitialize(float offsetY, int order) //used to reuse cell after destroying
    {
        coordinates.y = gm.data.sizeY + order;
        SetColor(Random.Range(0, gm.data.cellColors.Length));
        float rand = Random.Range(0f, 100f);
        SetType(rand < gm.data.GetStandardCellChance ? 0 : rand < gm.data.GetBombCellChance ? 1 : 2);
        transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(transform.localPosition.x, offsetY * (gm.data.sizeY + order), 0);
    }
    public Cell GetNeighbor(Direction direction)
    {
        switch(direction)
        {
            case Direction.W:
                {
                    if(coordinates.x>0)
                        return gm.GetCell(coordinates.x - 1, coordinates.y);
                    return null;
                }
            case Direction.NW:
                {
                    if (coordinates.x > 0 && coordinates.y < gm.data.sizeY-1)
                        return gm.GetCell(coordinates.x - 1, coordinates.y + 1);
                    return null;
                }
            case Direction.N:
                {
                    if (coordinates.y < gm.data.sizeY - 1)
                        return gm.GetCell(coordinates.x, coordinates.y + 1);
                    return null;
                }
            case Direction.NE:
                {
                    if (coordinates.x < gm.data.sizeX - 1 && coordinates.y < gm.data.sizeY - 1)
                        return gm.GetCell(coordinates.x + 1, coordinates.y + 1);
                    return null;
                }
            case Direction.E:
                {
                    if (coordinates.x < gm.data.sizeX - 1)
                        return gm.GetCell(coordinates.x + 1, coordinates.y);
                    return null;
                }
            case Direction.SE:
                {
                    if (coordinates.x < gm.data.sizeX - 1 && coordinates.y > 0)
                        return gm.GetCell(coordinates.x + 1, coordinates.y - 1);
                    return null;
                }
            case Direction.S:
                {
                    if (coordinates.y > 0)
                        return gm.GetCell(coordinates.x, coordinates.y - 1);
                    return null;
                }
            case Direction.SW:
                {
                    if (coordinates.x > 0 && coordinates.y > 0)
                        return gm.GetCell(coordinates.x - 1, coordinates.y - 1);
                    return null;
                }
        }
        return null;
    }
    public void Highlight(bool active)
    {
        selectionsr.enabled = active;
    }
    public bool ContainsNeighbor4(Cell cell) //check if cell is neighbor, but is not diagonal
    {
        if (coordinates.DistanceTo(cell.coordinates)==1)
            return true;
        return false;
    }
    public void SwitchPosition(Cell cell) //switch position with other cell
    {
        Coordinates contCoor = coordinates;
        coordinates = cell.coordinates;
        cell.coordinates = contCoor;
    }
    public void AddMoveStep()
    {
        moveDistance++;
    }
    public void SetMoveStep(int move)
    {
        moveDistance = move;
    }
    public void MoveDown(float offset)
    {
        if (moveDistance == 0)
            return;

        LeanTween.moveLocalY(gameObject, transform.localPosition.y - (offset * moveDistance), gm.data.animationDuration * moveDistance).setEase(LeanTweenType.easeOutBounce).setOnComplete(CheckCombo);
        coordinates.y -= moveDistance;
        moveDistance = 0;
    }

    private void SetColor(int idx)
    {
        color = idx;
        colorsr.color = gm.data.cellColors[idx];
    }
    private void SetType(int idx)
    {
        type = (CellType)idx;
        typesr.sprite = (idx == 1) ? gm.data.bombPowerUp : (idx == 2) ? gm.data.colorPowerUp : null;
    }
    private void CheckCombo() //check if there is match after moving 
    {
        if (gm.CheckColors(this))
        {
            gm.DestroyCombo();
        }
    }
}
