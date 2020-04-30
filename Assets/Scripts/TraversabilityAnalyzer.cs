using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public static class TraversabilityAnalyzer {
    public static int _width, _height = 0;

    private static List<TileData> _groundTiles = new List<TileData>();
    private static List<PlatformData> _platforms = new List<PlatformData>();

    private static int _roomCount = 0;
    public static int _testNumber = 0;

    public static CellularAutomataGenerator _caGenerator = null;
    public static OverlapWFC _wfcGenerator = null;

    public enum PCGType {
        CA,
        WFC,
        MARIO_1_1,
        OTHER
    }

    public static PCGType _pcgType;

    public static void Analyze(int[,] map, int width, int height, CellularAutomataGenerator generator) {
        _width = width;
        _height = height;
        _pcgType = PCGType.CA;
        _caGenerator = generator;
        Analyze(map);
    }

    public static void Analyze(int[,] map, int width, int height, OverlapWFC generator) {
        _width = width;
        _height = height;
        _wfcGenerator = generator;
        _pcgType = PCGType.WFC;
        Analyze(map);
    }

    public static void Analyze(int[,] map, int width, int height, PCGType type) {
        _width = width;
        _height = height;
        _pcgType = type;
        Analyze(map);
    }

    private static void Analyze(int[,] map) {
        if (!ExcelHelper.Init()) {
            return;
        }
        _testNumber = ExcelHelper.GetTestNumber();
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Calculating Room Count", 0.2f);
        GetRoomCount(map);
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Finding Ground Tiles", 0.3f);
        FindGroundTiles(map);
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Dividing Platforms", 0.4f);
        DivideIntoPlatforms();
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Saving To Excel", 0.5f);
        MapDataToExcel();
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Saving Input Data", 0.6f);
        if (_pcgType == PCGType.WFC) {
            UnityEngine.Object.FindObjectOfType<HiResScreenshots>().TakeHiResShot(_testNumber);
        }
        EditorUtility.DisplayProgressBar("Analyzing Traversability", "Done!", 1f);
        ExcelHelper.Save();
        ExcelHelper.StartExcel();
        EditorUtility.ClearProgressBar();
    }

    public static void FindGroundTiles(int[,] map) {
        _groundTiles.Clear();
        for (int y = _height - 1; y >= 0; y--) {
            for (int x = 0; x < _width; x++) {
                AddTile(x, y, map);
            }
        }
    }

    public static void AddTile(int x, int y, int[,] map) {
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

    private static void DivideIntoPlatforms() {
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

    private static void GetNeighbourTiles(TileData tile, List<TileData> existingNeighbours) {
        foreach (TileData groundTile in _groundTiles) {
            if (tile.IsValidNeighbour(groundTile, existingNeighbours)) {
                existingNeighbours.Add(groundTile);
                GetNeighbourTiles(groundTile, existingNeighbours);
            }
        }
    }


    private static List<Vector2Int> _roomTiles = new List<Vector2Int>();
    private static void GetRoomCount(int [,] map) {
        _roomTiles.Clear();
        _roomCount = 0;
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

    private static void AssignToRoomRecursive(Vector2Int tile) {
        _roomTiles.Remove(tile);
        List<Vector2Int> neighbours = GetNeighbourTiles(tile);
        foreach (Vector2Int neighbour in neighbours) {
            AssignToRoomRecursive(neighbour);
        }
    }

    private static List<Vector2Int> GetNeighbourTiles(Vector2Int tile) {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        foreach (Vector2Int roomTile in _roomTiles) {
            if (AreValidNeighbours(tile, roomTile)) {
                neighbours.Add(roomTile);
            }
        }
        return neighbours;
    }

    public static bool AreValidNeighbours(Vector2Int first, Vector2Int second) {
        int xDist = Mathf.Abs(first.x - second.x);
        int yDist = Mathf.Abs(first.y - second.y);
        if (xDist +  yDist > 1) return false;
        return true;
    }

    private static bool IsFloorTile(int x, int y, int[,] map) {
        if (y < _height - 1 && map[x,y+1] == 0) return true;
        return false;
    }
    private static bool IsWallTile(int x, int y, int[,] map) {
        if (x < _width - 1 && map[x + 1, y] == 0) return true;
        if (x > 0 && map[x - 1, y] == 0) return true;
        return false;
    }

    private static void MapDataToExcel() {
        int row = ExcelHelper.GetEmptyRow(ExcelHelper.MAPSHEET);

        ExcelHelper.WriteData(ExcelHelper.MAP_TEST_NUMBER, row, _testNumber.ToString());
        ExcelHelper.WriteData(ExcelHelper.PLATFORMCOUNT, row, _platforms.Count.ToString());
        ExcelHelper.WriteData(ExcelHelper.ROOMCOUNT, row, _roomCount.ToString());

        ExcelHelper.WriteData(ExcelHelper.MAP_PCG_TYPE, row, _pcgType.ToString());
        ExcelHelper.WriteData(ExcelHelper.MAP_DIMENSIONS, row, _width + "x" + _height);

        if (_pcgType == PCGType.CA) {
            CellularAutomataGenerator generator = _caGenerator;
            ExcelHelper.WriteData(ExcelHelper.MAP_SEED, row, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.MAP_FILLAMOUNT, row, generator.fillAmount.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_BIRTHLIMIT, row, generator.birthLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_DEATHLIMIT, row, generator.deathLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_SMOOTHITERATIONS, row, generator.smoothIterations.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_BLENDLAYERS, row, generator.blendLayers.ToString());
        }
        else if (_pcgType == PCGType.WFC) {
            ExcelHelper.WriteData(ExcelHelper.MAP_SEED, row, _wfcGenerator.seed.ToString());
            ExcelHelper.WriteData(ExcelHelper.MAP_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BLENDLAYERS, row, "—");
        }
        else {
            ExcelHelper.WriteData(ExcelHelper.MAP_SEED, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.MAP_BLENDLAYERS, row, "—");
        }
    }
}