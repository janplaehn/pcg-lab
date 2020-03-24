using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData {

    public List<TileData> _tiles;
    public List<ChunkData> _chunks = new List<ChunkData>();

    public PlatformData(List<TileData> tiles) {
        _tiles = new List<TileData>(tiles);
        if (!HasGroundTile()) return;
        Sort();
        Analyze();
    }

    private void Analyze() {
        //1.Divide into chunks:
        for (int i = 0; i < _tiles.Count; i++) {
            List<TileData> chunkTiles = GetChunkTiles(i);
            _chunks.Add(new ChunkData(chunkTiles));
        }
           

         //2. Get Roughness
            //a. Stones and Sizes
            //b.Stone Distances
    }

    private List<TileData> GetChunkTiles(int index) {
        int start, end, amount;
        start = index;
        end = index + (ChunkData.CHUNKWIDTH - 1);
        end = Mathf.Clamp(end, 0, _tiles.Count - 1);
        amount = (end - start) +1;
        return _tiles.GetRange(start, amount);
    }

    public bool HasGroundTile() {
        foreach (TileData tile in _tiles) {
            if (!tile._isWallTile) return true;
        }
        return false;
    }

    struct Column {
        public List<TileData> _tiles;
        public float averageY;
    };

    private void Sort() {
        List<TileData> tileSortedByX = _tiles;
        tileSortedByX.Sort((s1, s2) => s1._xPos.CompareTo(s2._xPos));
        int minX = _tiles[0]._xPos;
        int maxX = _tiles[_tiles.Count - 1]._xPos;

        List<Column> columns = new List<Column>();
        for (int x = minX; x <= maxX; x++) {
            Column newColumn = GetTilesAtX(tileSortedByX, x);
            columns.Add(newColumn);
        }

        for (int i = 0; i < columns.Count; i++) {
            if (i < columns.Count - 1) {
                SortColumn(columns[i], columns[i + 1].averageY);
            }
            else if (i > 0) {
                SortLastColumn(columns[i], columns[i - 1].averageY);
            }
        }

        _tiles.Clear();
        foreach (Column column in columns) {
            _tiles.AddRange(column._tiles);
        }
    }

    private void SortColumn(Column columnToSort, float nextColumnHeight) {
        columnToSort._tiles.Sort((s1, s2)
            => Mathf.Abs(s2._yPos - nextColumnHeight).CompareTo(Mathf.Abs(s1._yPos - nextColumnHeight)));
    }

    private void SortLastColumn(Column columnToSort, float previousColumnHeight) {
        columnToSort._tiles.Sort((s1, s2)
            => Mathf.Abs(s1._yPos - previousColumnHeight).CompareTo(Mathf.Abs(s2._yPos - previousColumnHeight)));
    }

    private Column GetTilesAtX(List<TileData> tiles, int xPos) {
        Column tilesAtX;
        tilesAtX._tiles = new List<TileData>();
        float averageY = 0;
        foreach (TileData tile in tiles) {
            if (tile._xPos == xPos) {
                tilesAtX._tiles.Add(tile);
                averageY += tile._yPos;
            }
        }
        tilesAtX.averageY = averageY / tilesAtX._tiles.Count;
        return tilesAtX;
    }
}
