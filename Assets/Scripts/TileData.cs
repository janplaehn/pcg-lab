﻿using System.Collections.Generic;
using UnityEngine;

public class TileData {
    public int _xPos;
    public int _yPos;
    public bool _isWallTile;

    public TileData(int x, int y, bool isWallTile) {
        _xPos = x;
        _yPos = y;
        _isWallTile = isWallTile;
    }

    public Vector2 GetPosition() {
        return new Vector2(_xPos, _yPos);
    }

    public bool IsValidNeighbour(TileData otherTile, List<TileData> existingNeighbours) {
        if(existingNeighbours.Contains(otherTile)) {
            return false;
        }
        int xOffset = _xPos - otherTile._xPos;
        int yOffset = _yPos - otherTile._yPos;        
        if (Mathf.Abs(xOffset) == 1 && yOffset == 1 && otherTile._isWallTile) {
            return false;
        }        
        if (Mathf.Abs(xOffset) == 1 && yOffset == -1 && _isWallTile) {
            return false;
        }
        if (Mathf.Abs(xOffset) <= 1 && Mathf.Abs(yOffset) <= 1) {
            return true;
        }
        return false;
    }
}
