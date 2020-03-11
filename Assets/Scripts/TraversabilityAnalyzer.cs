using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TraversabilityAnalyzer {
    private int _width, _height = 0;

    private List<TileData> _groundTiles = new List<TileData>();
    private List<PlatformData> _platforms = new List<PlatformData>();

    public TraversabilityAnalyzer(int[,] map, int width, int height) {
        _width = width;
        _height = height;
        Analyze(map);
    }

    public void Analyze(int[,] map) {
        FindGroundTiles(map);
        DivideIntoPlatforms();
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

    private bool IsFloorTile(int x, int y, int[,] map) {
        if (y < _height - 1 && map[x,y+1] == 0) return true;
        return false;
    }
    private bool IsWallTile(int x, int y, int[,] map) {
        if (x < _width - 1 && map[x + 1, y] == 0) return true;
        if (x > 0 && map[x - 1, y] == 0) return true;
        return false;
    }
}