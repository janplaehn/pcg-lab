using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TraversabilityAnalyzer {
    private int _width, _height = 0;

    private List<TileData> _groundTiles = new List<TileData>();
    private List<PlatformData> _platforms = new List<PlatformData>();

    private int _roomCount = 0;

    public static CellularAutomataGenerator _caGenerator = null;
    public static OverlapWFC _wfcGenerator = null;

    public enum PCGType {
        CA,
        WFC,
        OTHER
    }

    public static PCGType _type;

    public TraversabilityAnalyzer(int[,] map, int width, int height, CellularAutomataGenerator generator) {
        _width = width;
        _height = height;
        _type = PCGType.CA;
        _caGenerator = generator;
        Analyze(map);
    }

    public TraversabilityAnalyzer(int[,] map, int width, int height, OverlapWFC generator) {
        _width = width;
        _height = height;
        _wfcGenerator = generator;
        _type = PCGType.WFC;
        Analyze(map);
    }

    public TraversabilityAnalyzer(int[,] map, int width, int height) {
        _width = width;
        _height = height;
        _type = PCGType.OTHER;
        Analyze(map);
    }

    private void Analyze(int[,] map) {
        ExcelHelper.Init();
        GetRoomCount(map);
        FindGroundTiles(map);
        DivideIntoPlatforms();
        MapDataToExcel();
        ExcelHelper.Save();
        ExcelHelper.StartExcel();
    }

    public void FindGroundTiles(int[,] map) {
        _groundTiles.Clear();
        for (int y = _height - 1; y >= 0; y--) {
            for (int x = 0; x < _width; x++) {
                AddTile(x, y, map);
            }
        }
    }

    public void AddTile(int x, int y, int[,] map) {
        if (map[x, y] == 0) return;
        if (IsFloorTile(x, y, map)) {
            TileData floorTile = new TileData(x, y, false);
            _groundTiles.Add(floorTile);
        }
        else if (IsWallTile(x, y, map)) {
            TileData wallTile = new TileData(x, y, true);
            _groundTiles.Add(wallTile);
        }
    }

    private void DivideIntoPlatforms() {
        _platforms.Clear();
        while (_groundTiles.Count > 0) {
            List<TileData> platformTiles = new List<TileData>();
            platformTiles.Add(_groundTiles[0]);
            GetNeighbourTiles(_groundTiles[0], platformTiles);

            PlatformData newPlatform = new PlatformData(platformTiles);
            if (newPlatform.HasGroundTile()) {
                _platforms.Add(newPlatform);
            }
            _groundTiles = _groundTiles.Except(platformTiles).ToList();
        }
    }

    private void GetNeighbourTiles(TileData tile, List<TileData> existingNeighbours) {
        foreach (TileData groundTile in _groundTiles) {
            if (tile.IsValidNeighbour(groundTile, existingNeighbours)) {
                existingNeighbours.Add(groundTile);
                GetNeighbourTiles(groundTile, existingNeighbours);
            }
        }
    }


    List<Vector2Int> _roomTiles = new List<Vector2Int>();
    private void GetRoomCount(int [,] map) {
        _roomTiles.Clear();
        for (int y = _height - 1; y >= 0; y--) {
            for (int x = 0; x < _width; x++) {
                if (map[x, y] == 0) {
                    _roomTiles.Add(new Vector2Int(x,y));
                }
            }
        }
        while (_roomTiles.Count > 0) {
            AssignToRoomRecursive(_roomTiles[0]);
            _roomCount++;
        }
    }

    private void AssignToRoomRecursive(Vector2Int tile) {
        _roomTiles.Remove(tile);
        List<Vector2Int> neighbours = GetNeighbourTiles(tile);
        foreach (Vector2Int neighbour in neighbours) {
            AssignToRoomRecursive(neighbour);
        }
    }

    private List<Vector2Int> GetNeighbourTiles(Vector2Int tile) {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        foreach (Vector2Int roomTile in _roomTiles) {
            if (AreValidNeighbours(tile, roomTile)) {
                neighbours.Add(roomTile);
            }
        }
        return neighbours;
    }

    public bool AreValidNeighbours(Vector2Int first, Vector2Int second) {
        int xDist = Mathf.Abs(first.x - second.x);
        int yDist = Mathf.Abs(first.y - second.y);
        if (xDist +  yDist > 1) return false;
        return true;
    }

    private bool IsFloorTile(int x, int y, int[,] map) {
        if (y < _height - 1 && map[x,y+1] == 0) return true;
        return false;
    }
    private bool IsWallTile(int x, int y, int[,] map) {
        if (x < _width - 1 && map[x + 1, y] == 0) return true;
        if (x > 0 && map[x - 1, y] == 0) return true;
        return false;
    }

    private void MapDataToExcel() {
        int row = ExcelHelper.GetEmptyRow(ExcelHelper.MAPSHEET);

        ExcelHelper.WriteData(ExcelHelper.PLATFORMCOUNT, row, _platforms.Count.ToString());
        ExcelHelper.WriteData(ExcelHelper.ROOMCOUNT, row, _roomCount.ToString());

        ExcelHelper.WriteData(ExcelHelper.MAP_PCG_TYPE, row, _type.ToString());

        if (_type == PCGType.CA) {
            CellularAutomataGenerator generator = _caGenerator;
            ExcelHelper.WriteData(ExcelHelper.MAP_DIMENSIONS, row, generator.width + "x" + _caGenerator.height);
            ExcelHelper.WriteData(ExcelHelper.MAP_SEED, row, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.MAP_FILLAMOUNT, row, generator.fillAmount.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_BIRTHLIMIT, row, generator.birthLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_DEATHLIMIT, row, generator.deathLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_SMOOTHITERATIONS, row, generator.smoothIterations.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_BLENDLAYERS, row, generator.blendLayers.ToString());
        }
        else {
            ExcelHelper.WriteData(ExcelHelper.MAP_DIMENSIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_SEED, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BLENDLAYERS, row, "—");
        }
    }
}