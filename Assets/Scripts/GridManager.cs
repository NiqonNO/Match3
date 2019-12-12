using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public DataConfig data;
    [SerializeField] Cell cellPrefab;
    [SerializeField] AudioManager am;

    Cell[] cellGrid; //every cell in grid
    Cell selectedCell = null;
    List<Cell>[] cellsToDestroy; // cells lists to destroy, for every x coordinate
    float offset; //size of cells
    bool combo = false;
    bool blockInput = false;

    private void Awake()
    {
        LeanTween.init(data.sizeX * data.sizeY);
        cellGrid = new Cell[data.sizeX * data.sizeY];
        cellsToDestroy = new List<Cell>[data.sizeX]; 

        offset = cellPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        transform.position = new Vector3(-offset * (data.sizeX - 1) / 2f, -offset * (data.sizeY - 1) / 2f, 0); // move grid so spawning cells start at 0,0

        InitializeGrid();
    }
    private void Update()
    {
        // if combo is matched, wait for end of tweening so cells don't alter if they are alredy moving
        if (combo && LeanTween.tweensRunning == 0)
        {
            StartCoroutine("DestroyAndMoveCells");
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
            }
        }
    }
    private List<Cell> CheckColorsInLine(Cell cell, Direction direction)
    {
        List<Cell> matchingCells = new List<Cell>();
        matchingCells.Add(cell);

        Cell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
            return matchingCells;
        if (neighbor.color == cell.color)
            matchingCells.AddRange(CheckColorsInLine(neighbor, direction));

        return matchingCells;
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
    private void AddColorToDestroy(int color)
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

        //check if cell is alredy in list
        if (!cellsToDestroy[cell.coordinates.x].Contains(cell))
        {
            cellsToDestroy[cell.coordinates.x].Add(cell);
            CheckForPowerUp(cell);
        }
    }

    public bool CheckColors(Cell cell)
    {
        List<Cell> matchingCells = new List<Cell>();
        List<Cell> matchingCellsHorizontal = new List<Cell>();
        List<Cell> matchingCellsVertical = new List<Cell>();

        //check horizontal
        Cell neighbor = cell.GetNeighbor(Direction.E);
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsHorizontal.AddRange(CheckColorsInLine(neighbor, Direction.E));
        neighbor = cell.GetNeighbor(Direction.W);
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsHorizontal.AddRange(CheckColorsInLine(neighbor, Direction.W));
        if (matchingCellsHorizontal.Count < 2)
            matchingCellsHorizontal.Clear();

        //check vertical
        neighbor = cell.GetNeighbor(Direction.N);
        if (neighbor != null && neighbor.color == cell.color)
            matchingCellsVertical.AddRange(CheckColorsInLine(neighbor, Direction.N));
        neighbor = cell.GetNeighbor(Direction.S);
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
                AddToDestroy(matchingCells[i]); // don't add directly, check if cell is alredy in list first
            }
            return true;
        }
        return false;
    }
    public void SelectCell(Cell cell)
    {
        // block input when cells are in move
        if (LeanTween.tweensRunning != 0 || blockInput)
            return;

        if (selectedCell == null)
        {
            am.PlaySelect();
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
                am.PlaySelect();
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
        blockInput = true;
    }

    private IEnumerator DoCheck(Cell cell)
    {
        am.PlaySwap();
        blockInput = true;
        //Switch cells position, coordinates and index for easier comparision.
        selectedCell.Highlight(false);
        LeanTween.move(selectedCell.gameObject, cell.transform.position, data.animationDuration);
        LeanTween.move(cell.gameObject, selectedCell.transform.position, data.animationDuration);

        cellGrid[GetCellIdx(cell.coordinates.x, cell.coordinates.y)] = selectedCell;
        cellGrid[GetCellIdx(selectedCell.coordinates.x, selectedCell.coordinates.y)] = cell;
        selectedCell.SwitchPosition(cell);
        yield return new WaitForSeconds(data.animationDuration);

        //Check both cells match
        bool match = CheckColors(selectedCell);
        if (CheckColors(cell) || match)
        {
            StartCoroutine("DestroyAndMoveCells");
            selectedCell = null;
            yield break;
        }

        //Switch cells back.
        LeanTween.move(selectedCell.gameObject, cell.transform.position, data.animationDuration);
        LeanTween.move(cell.gameObject, selectedCell.transform.position, data.animationDuration);

        cellGrid[GetCellIdx(cell.coordinates.x, cell.coordinates.y)] = selectedCell;
        cellGrid[GetCellIdx(selectedCell.coordinates.x, selectedCell.coordinates.y)] = cell;
        selectedCell.SwitchPosition(cell);
        yield return new WaitForSeconds(data.animationDuration);
        selectedCell = null;
        blockInput = false;
    }
    private IEnumerator DestroyAndMoveCells()
    {
        blockInput = true;
        List<Cell> cellsToMove = new List<Cell>();
        int[][] destroyedCountPerColumn = new int[2][];
        destroyedCountPerColumn[0] = new int[data.sizeX]; // Contains info how much move object down (+ value from seccond array)
        destroyedCountPerColumn[1] = new int[data.sizeX]; // How much to move up so cells don't start from the same y position
        am.PlayClear();
        for (int i = 0; i < cellsToDestroy.Length; i++)
        {
            if (cellsToDestroy[i] is null)
                continue;

            for (int j = 0; j < cellsToDestroy[i].Count; j++)
            {
                LeanTween.scale(cellsToDestroy[i][j].gameObject, Vector3.one * 0.025f, data.animationDuration).setEase(LeanTweenType.easeInBack);
                destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x]++;
                for (int k = cellsToDestroy[i][j].coordinates.y+1; k < data.sizeY; k++) // add cells above destroyed cell to move, and +1 move step for every destroyed cell below
                {
                    Cell cell = GetCell(cellsToDestroy[i][j].coordinates.x, k);
                    if (!cellsToMove.Contains(cell))
                        cellsToMove.Add(cell);
                    cell.AddMoveStep();
                }
            }
        }
        yield return new WaitForSeconds(data.animationDuration); // wait until scaling end

        for (int i = 0; i < cellsToDestroy.Length; i++)
        {
            if (cellsToDestroy[i] is null)
                continue;

            for (int j = 0; j < cellsToDestroy[i].Count; j++)
            {
                //reinitialize destroyed cell, and set move step for moving it down
                cellsToDestroy[i][j].SetMoveStep(destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x] + destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]);
                cellsToDestroy[i][j].ReInitialize(offset, destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]);
                if (!cellsToMove.Contains(cellsToDestroy[i][j]))
                    cellsToMove.Add(cellsToDestroy[i][j]);
                destroyedCountPerColumn[0][cellsToDestroy[i][j].coordinates.x]--;
                destroyedCountPerColumn[1][cellsToDestroy[i][j].coordinates.x]++;
            }
        }
        // move cells down
        for (int i = 0; i < cellsToMove.Count; i++)
        {
            cellsToMove[i].MoveDown(offset);
            cellGrid[GetCellIdx(cellsToMove[i].coordinates)] = cellsToMove[i];
        }
        cellsToDestroy = new List<Cell>[data.sizeX];
        blockInput = false;
    }
}