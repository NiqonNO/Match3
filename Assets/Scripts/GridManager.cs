using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public DataConfig data;
    [SerializeField] Cell cellPrefab;

    Cell[] cellGrid;
    Cell selectedCell = null;
    List<Cell>[] cellsToDestroy;
    float offset;
    bool combo = false;

    private void Awake()
    {
        cellGrid = new Cell[data.sizeX * data.sizeY];
        cellsToDestroy = new List<Cell>[data.sizeX];

        offset = cellPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        transform.position = new Vector3(-offset * (data.sizeX - 1) / 2f, -offset * (data.sizeY - 1) / 2f, 0);

        InitializeGrid();
    }
    private void Update()
    {
        if(combo && LeanTween.tweensRunning == 0)
        {
            StartCoroutine("DestroyCells");
            combo = false;
        }
    }

    private void InitializeGrid()
    {
        for (int y = 0; y < data.sizeY; y++)
        {
            for (int x = 0; x < data.sizeX; x++)
            {
                int idx = x + (y * data.sizeX);
                Vector3 position = transform.TransformPoint(new Vector3(offset * x, offset * y, 0));
                cellGrid[idx] = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cellGrid[idx].Initialize(this, new Coordinates(x, y));
                /*if (x != 0)
                {
                    int neighborIdx = (x - 1) + (y * data.sizeX);
                    cellGrid[idx].SetNeighbor(Direction.W, cellGrid[neighborIdx]);
                    if (y != 0)
                    {
                        neighborIdx = (x - 1) + ((y - 1) * data.sizeX);
                        cellGrid[idx].SetNeighbor(Direction.SW, cellGrid[neighborIdx]);
                    }
                }
                if (y != 0)
                {
                    int neighborIdx = x + ((y - 1) * data.sizeX);
                    cellGrid[idx].SetNeighbor(Direction.S, cellGrid[neighborIdx]);
                    if (x != data.sizeX - 1)
                    {
                        neighborIdx = (x + 1) + ((y - 1) * data.sizeX);
                        cellGrid[idx].SetNeighbor(Direction.SE, cellGrid[neighborIdx]);
                    }
                }*/

            }
        }
    }
    public bool CheckColors(Cell cell)
    {
        List<Cell> matchingCells = new List<Cell>();
        List<Cell> matchingCellsHorizontal = new List<Cell>();
        List<Cell> matchingCellsVertical = new List<Cell>();

        Cell neighbor = cell.GetNeighbor(Direction.E);//.neighbors[(int)Direction.E];
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsHorizontal.AddRange(CheckColorsInLine(neighbor, Direction.E));
        neighbor = cell.GetNeighbor(Direction.W);//.neighbors[(int)Direction.W];
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsHorizontal.AddRange(CheckColorsInLine(neighbor, Direction.W));
        if (matchingCellsHorizontal.Count < 2)
            matchingCellsHorizontal.Clear();

        neighbor = cell.GetNeighbor(Direction.N);//.neighbors[(int)Direction.N];
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsVertical.AddRange(CheckColorsInLine(neighbor, Direction.N));
        neighbor = cell.GetNeighbor(Direction.S);//.neighbors[(int)Direction.S];
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsVertical.AddRange(CheckColorsInLine(neighbor, Direction.S));
        if (matchingCellsVertical.Count < 2)
            matchingCellsVertical.Clear();

        matchingCells.AddRange(matchingCellsHorizontal);
        matchingCells.AddRange(matchingCellsVertical);

        if (matchingCells.Count > 0)
        {
            matchingCells.Add(cell);
            for (int i = 0; i < matchingCells.Count; i++)
            {
                AddToDestroy(matchingCells[i]);
            }
            return true;
        }
        return false;
    }
    private List<Cell> CheckColorsInLine(Cell cell, Direction direction)
    {
        List<Cell> matchingCells = new List<Cell>();
        matchingCells.Add(cell);

        Cell neighbor = cell.GetNeighbor(direction);//.neighbors[(int)direction];
        if (neighbor == null)
            return matchingCells;
        if (neighbor.color == cell.color)
            matchingCells.AddRange(CheckColorsInLine(neighbor, direction));

        return matchingCells;
    }
    private void UpdateColumns()
    {

    }
    private void CheckForPowerUp(Cell cell)
    {
        switch (cell.type)
        {
            case CellType.DestroyAround:
                {
                    AddAroundToDestroy(cell);
                    return;
                }
            case CellType.DestroyColor:
                {
                    AddColorToDestroy(cell.color);
                    return;
                }
            default:
                return;
        }
    }
    private void AddAroundToDestroy(Cell cell)
    {
        for (Direction d = Direction.W; d <= Direction.SW; d++)
        {
            Cell neighbor = cell.GetNeighbor(d);

            if (neighbor is null)
                continue;

            AddToDestroy(neighbor);
        }
    }
    private void AddColorToDestroy(CellColor color)
    {
        for(int i =0; i < cellGrid.Length; i++)
        {
            if (cellGrid[i].color == color)
                AddToDestroy(cellGrid[i]);
        }
    }
    private void AddToDestroy(Cell cell)
    {
        if (cellsToDestroy[cell.coordinates.x] is null)
            cellsToDestroy[cell.coordinates.x] = new List<Cell>();

        if (!cellsToDestroy[cell.coordinates.x].Contains(cell))
        {
            cellsToDestroy[cell.coordinates.x].Add(cell);
            CheckForPowerUp(cell);
        }
    }

    public void SelectCell(Cell cell)
    {
        if (LeanTween.tweensRunning != 0)
            return;

        if (selectedCell == null)
        {
            selectedCell = cell;
            selectedCell.Highlight(true);
        }
        else if (selectedCell == cell)
        {
            selectedCell.Highlight(false);
            selectedCell = null;
        }
        else
        {
            if (selectedCell.ContainsNeighbor4(cell))
                StartCoroutine("DoCheck", cell);
            else
            {
                selectedCell.Highlight(false);
                selectedCell = cell;
                selectedCell.Highlight(true);
            }
        }
    }
    public Cell GetCell(int x, int y)
    {
        return cellGrid[x + (y * data.sizeX)];
    }
    public int GetCellIdx(int x, int y)
    {
        return x + (y * data.sizeX);
    }
    public int GetCellIdx(Coordinates coord)
    {
        return coord.x + (coord.y * data.sizeX);
    }
    public void DestroyCombo()
    {
        combo = true;
    }

    private IEnumerator DoCheck(Cell cell)
    {
        selectedCell.Highlight(false);
        LeanTween.move(selectedCell.gameObject, cell.transform.position, data.animationDuration);
        LeanTween.move(cell.gameObject, selectedCell.transform.position, data.animationDuration);

        cellGrid[GetCellIdx(cell.coordinates.x, cell.coordinates.y)] = selectedCell;
        cellGrid[GetCellIdx(selectedCell.coordinates.x, selectedCell.coordinates.y)] = cell;
        selectedCell.SwitchPosition(cell);
        yield return new WaitForSeconds(data.animationDuration);

        bool match = CheckColors(selectedCell);
        if (CheckColors(cell) || match)
        {
            StartCoroutine("DestroyCells");
            selectedCell = null;
            yield break;
        }

        LeanTween.move(selectedCell.gameObject, cell.transform.position, data.animationDuration);
        LeanTween.move(cell.gameObject, selectedCell.transform.position, data.animationDuration);

        cellGrid[GetCellIdx(cell.coordinates.x, cell.coordinates.y)] = selectedCell;
        cellGrid[GetCellIdx(selectedCell.coordinates.x, selectedCell.coordinates.y)] = cell;
        selectedCell.SwitchPosition(cell);
        yield return new WaitForSeconds(data.animationDuration);
        selectedCell = null;
    }
    public IEnumerator DestroyCells()
    {
        List<Cell> cellsToMove = new List<Cell>();
        int[][] destroyedCountPerColumn = new int[2][];
        destroyedCountPerColumn[0] = new int[data.sizeX];
        destroyedCountPerColumn[1] = new int[data.sizeX];
        for (int i = 0; i < cellsToDestroy.Length; i++)
        {
            if (cellsToDestroy[i] is null)
                continue;

            for (int j = 0; j < cellsToDestroy[i].Count; j++)
            {
                LeanTween.scale(cellsToDestroy[i][j].gameObject, Vector3.zero, data.animationDuration);
                destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x]++;
                for (int k = cellsToDestroy[i][j].coordinates.y+1; k < data.sizeY; k++)
                {
                    cellsToMove.Add(GetCell(cellsToDestroy[i][j].coordinates.x, k));
                    cellsToMove[cellsToMove.Count-1].AddMoveStep();
                }
            }
        }
        Debug.Log("Before");
        yield return new WaitForSeconds(data.animationDuration);
        Debug.Log("After");
        for (int i = 0; i < cellsToDestroy.Length; i++)
        {
            if (cellsToDestroy[i] is null)
                continue;

            for (int j = 0; j < cellsToDestroy[i].Count; j++)
            {
                cellsToDestroy[i][j].SetMoveStep(destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x] + destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]);
                cellsToDestroy[i][j].ReInitialize(offset, destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]);
                cellsToMove.Add(cellsToDestroy[i][j]);
                destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x]--;
                destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]++;
            }
        }
        for (int i = 0; i < cellsToMove.Count; i++)
        {
            cellsToMove[i].MoveDown(offset);
            cellGrid[GetCellIdx(cellsToMove[i].coordinates)] = cellsToMove[i];
        }
        Debug.Log("After2");
        cellsToDestroy = new List<Cell>[data.sizeX];
        Debug.Log("After3");
    }
}