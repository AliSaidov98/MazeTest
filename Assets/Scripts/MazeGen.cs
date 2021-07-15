using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MazeGen : MonoBehaviour
{
    [SerializeField] private GameObject _cell;
    [SerializeField] private GameObject _whiteSpace;
    
    private int _rows;
    private int _cols;

    private GameObject _mazeParent;
    
    private readonly Dictionary<Vector2, Cell> _allCells = new Dictionary<Vector2, Cell>();
    private readonly List<Cell> _unvisited = new List<Cell>();
    private readonly List<Cell> _stack = new List<Cell>();
    private readonly Vector2[] _neighbourPositions = { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1) };
    
    private Cell _centreCell;
    private Cell _currentCell;
    private Cell _checkCell;
    
    private class Cell
    {
        public Vector2 gridPos;
        public GameObject cellObject;
        public CellScript cScript;
    }
    
    public void GenerateMaze(int rows, int cols)
    {
        //if maze exists, delete
        if (_mazeParent != null) DeleteMaze();
        
        //create the parent object of the maze cells
        SetMazeParent();
        
        _rows = rows;
        _cols = cols;
        
        InitMaze();
        CreateStartPoint();
        RunAlgorithm();
        MakeExit();
    }

    private void DeleteMaze()
    {
        if (_mazeParent != null) Destroy(_mazeParent);
    }

    private void InitMaze()
    {
        //Set the white space suit to the maze
        _whiteSpace.transform.localScale = (Vector3.up  * _rows + Vector3.right * _cols) * 0.875f;
        
        var cellSize = _cell.transform.localScale.x;
        
        //calculate the start position depends on the cell scale, rows and cols
        
        Vector2 startPos = new Vector2(-(cellSize * (_cols / 2)) + (cellSize / 2), -(cellSize * (_rows / 2)) + (cellSize / 2));
        Vector2 spawnPos = startPos;
        
        //generate the cell in the appropriate position
        //with the code above the cells generates in the why that the maze is in the center
        for (int x = 1; x <= _cols; x++)
        {
            for (int y = 1; y <= _rows; y++)
            {
                GenerateCell(spawnPos, new Vector2(x, y));

                spawnPos.y += cellSize;
            }
            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }
    }

    private void RunAlgorithm()
    {
        // Get start cell, make it visited (i.e. remove from unvisited list).
        _unvisited.Remove(_currentCell);

        // While we have unvisited cells.
        while (_unvisited.Count > 0)
        {
            List<Cell> unvisitedNeighbours = GetUnvisitedNeighbours(_currentCell);
            if (unvisitedNeighbours.Count > 0)
            {
                // Get a random unvisited neighbour.
                _checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                // Add current cell to stack.
                _stack.Add(_currentCell);
                // Compare and remove walls.
                CompareWalls(_currentCell, _checkCell);
                // Make currentCell the neighbour cell.
                _currentCell = _checkCell;
                // Mark new current cell as visited.
                _unvisited.Remove(_currentCell);
            }
            else if (_stack.Count > 0)
            {
                // Make current cell the most recently added Cell from the stack.
                _currentCell = _stack[_stack.Count - 1];
                // Remove it from stack.
                _stack.Remove(_currentCell);
            }
        }
    }

    private void MakeExit()
    {
        // Create and populate list of all possible edge cells.
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in _allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == _cols || cell.Key.y == 0 || cell.Key.y == _rows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        // Get edge cell randomly from list.
        Cell newCell = edgeCells[Random.Range(0, edgeCells.Count)];

        // Remove appropriate wall for chosen edge cell.
        if (newCell.gridPos.x == 0) RemoveWall(newCell.cScript, 1);
        else if (newCell.gridPos.x == _cols) RemoveWall(newCell.cScript, 2);
        else if (newCell.gridPos.y == _rows) RemoveWall(newCell.cScript, 3);
        else RemoveWall(newCell.cScript, 4);

        Debug.Log("Maze generation finished.");
    }

    private List<Cell> GetUnvisitedNeighbours(Cell curCell)
    {
        // Create a list to return.
        List<Cell> neighbours = new List<Cell>();
        // Create a Cell object.
        Cell nCell = curCell;
        // Store current cell grid pos.
        Vector2 cPos = curCell.gridPos;

        foreach (Vector2 p in _neighbourPositions)
        {
            // Find position of neighbour on grid, relative to current.
            Vector2 nPos = cPos + p;
            // If cell exists.
            if (_allCells.ContainsKey(nPos)) nCell = _allCells[nPos];
            // If cell is unvisited.
            if (_unvisited.Contains(nCell)) neighbours.Add(nCell);
        }

        return neighbours;
    }

    // Compare neighbour with current and remove appropriate walls.
    private void CompareWalls(Cell curCell, Cell neighbCell)
    {
        // If neighbour is left of current.
        if (neighbCell.gridPos.x < curCell.gridPos.x)
        {
            RemoveWall(neighbCell.cScript, 2);
            RemoveWall(curCell.cScript, 1);
        }
        // Else if neighbour is right of current.
        else if (neighbCell.gridPos.x > curCell.gridPos.x)
        {
            RemoveWall(neighbCell.cScript, 1);
            RemoveWall(curCell.cScript, 2);
        }
        // Else if neighbour is above current.
        else if (neighbCell.gridPos.y > curCell.gridPos.y)
        {
            RemoveWall(neighbCell.cScript, 4);
            RemoveWall(curCell.cScript, 3);
        }
        // Else if neighbour is below current.
        else if (neighbCell.gridPos.y < curCell.gridPos.y)
        {
            RemoveWall(neighbCell.cScript, 3);
            RemoveWall(curCell.cScript, 4);
        }
    }


    private void GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        // Create new Cell object.
        Cell newCell = new Cell();

        // Store reference to position in grid.
        newCell.gridPos = keyPos;
        // Set and instantiate cell GameObject.
        newCell.cellObject = Instantiate(_cell, pos, _cell.transform.rotation);
        // Child new cell to parent.
        newCell.cellObject.transform.SetParent(_mazeParent.transform);
        // Set name of cellObject.
        newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
        // Get reference to attached CellScript.
        newCell.cScript = newCell.cellObject.GetComponent<CellScript>();

        // Add to Lists.
        _allCells[keyPos] = newCell;
        _unvisited.Add(newCell);
    }

    private void CreateStartPoint()
    {
        // Get the StartPoint cell using the rows and columns variables.
        // Remove the required walls.
        _centreCell = _allCells[new Vector2(1, 1)];
        RemoveWall(_centreCell.cScript, 2);
        RemoveWall(_centreCell.cScript, 3);
        RemoveWall(_centreCell.cScript, 4);
        
        // Set centre cell as the current cell 
        _currentCell = _centreCell;
    }

    private void RemoveWall(CellScript cScript, int wallID)
    {
        //Remove the according wall
        if (wallID == 1) cScript.wallL.SetActive(false);
        else if (wallID == 2) cScript.wallR.SetActive(false);
        else if (wallID == 3) cScript.wallU.SetActive(false);
        else if (wallID == 4) cScript.wallD.SetActive(false);
    }

    private void SetMazeParent()
    {
        //create the maze parent object
        _mazeParent = new GameObject();
        _mazeParent.transform.position = Vector2.zero;
        _mazeParent.name = "Maze";
    }
}
