using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAnalyzer : MonoBehaviour {

    public struct Tile {
        public int _xPos;
        public int _yPos;
        public bool _isWallTile;
    }

    public struct Platform {
        public List<Tile> _tiles;
    }

    private int _mapWidth = 0;
    private int _mapHeight = 0;

    private List<Tile> _groundTiles = new List<Tile>();
    private List<Tile> _currentPlatformTiles = new List<Tile>();
    private List<Platform> _platforms = new List<Platform>();

    public void Analyze(int[,] map, int width, int height) {
        _mapWidth = width;
        _mapHeight = height;
        ValidateGroundTiles(map);
        DivideIntoPlatforms();
        ClearWallPlatforms();
    }

    private void ClearWallPlatforms() {
        List<Platform> platformsToRemove = new List<Platform>();
        foreach (Platform platform in _platforms) {
            if (!HasGroundTile(platform)) {
                platformsToRemove.Add(platform);
            }
        }
        foreach (Platform platformToRemove in platformsToRemove) {
            _platforms.Remove(platformToRemove);
        }
    }

    private bool HasGroundTile(Platform platform) {
        foreach (Tile platformTile in platform._tiles) {
            if (!platformTile._isWallTile) {
                return true;
            }
        }
        return false;
    }

    public void ValidateGroundTiles(int[,] map) {
        _groundTiles.Clear();
        for (int y = _mapHeight - 1; y >= 0; y--) {
            for (int x = 0; x < _mapWidth; x++) {
                if (map[x, y] == 0) {
                    continue;
                }
                if (IsFloorTile(x, y, map)) {
                    AddGroundTile(x, y, false);
                }
                else if (IsWallTile(x, y, map)) {
                    AddGroundTile(x, y, true);
                }
            }
        }
    }

    private void DivideIntoPlatforms() {
        _currentPlatformTiles.Clear();
        _platforms.Clear();
        while (_groundTiles.Count > 0) {
            //Get All Neighbouring Tiles of First Tile
            _currentPlatformTiles.Add(_groundTiles[0]);
            GetNextTile(_groundTiles[0]);

            //Make Platform
            Platform newPlatform;
            newPlatform._tiles = new List<Tile>(_currentPlatformTiles);
            _platforms.Add(newPlatform);

            //Remove Tiles in Platform
            foreach (Tile tileToRemove in _currentPlatformTiles) {
                _groundTiles.Remove(tileToRemove);
            }
            _currentPlatformTiles.Clear();           
        }
    }

    private void GetNextTile(Tile tile) {
        for (int i = 0; i < _groundTiles.Count; i++) {

            if (AreTilesValidNeighbours(tile, _groundTiles[i])) {
                if (!_currentPlatformTiles.Contains(_groundTiles[i])) {
                    _currentPlatformTiles.Add(_groundTiles[i]);
                    GetNextTile(_groundTiles[i]);
                }
            }
        }
    }

    private bool AreTilesValidNeighbours(Tile tileA, Tile tileB) {
        int xOffset = tileA._xPos - tileB._xPos;
        int yOffset = tileA._yPos - tileB._yPos;


        //If Diagonal and a on top, b mustnt be walltile
        if (Mathf.Abs(xOffset) == 1 & yOffset == 1) {
            if (tileB._isWallTile) {
                return false;
            }
        }

        //If Diagonal and b on top, a mustnt be walltile
        if (Mathf.Abs(xOffset) == 1 & yOffset == -1) {
            if (tileA._isWallTile) {
                return false;
            }
        }

        //If neighbours
        if (Mathf.Abs(xOffset) <= 1 && Mathf.Abs(yOffset) <= 1) {        
            return true;
        }

        return false;
    }

    private void AddGroundTile(int x, int y, bool isWallTile) {
        Tile newTile;
        newTile._xPos = x;
        newTile._yPos = y;
        newTile._isWallTile = isWallTile;
        _groundTiles.Add(newTile);
    }

    private bool IsFloorTile(int x, int y, int[,] map) {
        if (y < _mapHeight - 1 && map[x,y+1] == 0) {
            return true;
        }
        return false;
    }

    private bool IsWallTile(int x, int y, int[,] map) {
        if (x < _mapWidth - 1 && map[x + 1, y] == 0) {
            return true;
        }
        if (x > 0 && map[x - 1, y] == 0) {
            return true;
        }
        return false;
    }
}
