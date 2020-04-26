using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData {

    public List<TileData> _tiles;
    public List<ChunkData> _chunks = new List<ChunkData>();

    public float _platformSlope = 0;

    public List<float> _rockWidths = new List<float>();
    public List<float> _rockDistances = new List<float>();

    public PlatformData(List<TileData> tiles) {
        _tiles = new List<TileData>(tiles);
        if (!HasGroundTile()) return;
        Sort();
        Analyze();
    }

    private void Analyze() {
        for (int i = 0; i < _tiles.Count; i++) {
            List<TileData> chunkTiles = GetChunkTiles(i);
            _chunks.Add(new ChunkData(chunkTiles));
        }
        CalculateRoughness();
        //Todo: Save to Excel
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

    readonly int UNASSIGNED = -1;
    enum AngleOffset {NEUTRAL, POSITIVE, NEGATIVE };
    float _currentRockWidth = -1;
    float _currentRockDistance = -1;
    private void CalculateRoughness() {
        AngleOffset offset = AngleOffset.NEUTRAL;
        TileData startTile = _tiles[0];
        TileData endTile = _tiles[_tiles.Count - 1];
        _platformSlope = SlopeBetweenTiles(startTile, endTile);
        for (int i = 0; i < _tiles.Count - 1; i++) {
            TileData first = _tiles[i];
            TileData second = _tiles[i+1];
            float tileSlope = SlopeBetweenTiles(first, second);
            if (tileSlope > _platformSlope && offset != AngleOffset.POSITIVE) {
                offset = AngleOffset.POSITIVE;
                SaveRock();
                InitRock();
                
            }
            else if (tileSlope == _platformSlope && offset != AngleOffset.NEUTRAL) {
                offset = AngleOffset.NEUTRAL;
                SaveRock();
            }
            else if (tileSlope < _platformSlope) {
                offset = AngleOffset.NEGATIVE;
                ExpandRock();
            }
            else {
                ExpandRock();
            }
        }
    }

    private void SaveRock() {
        if (_currentRockWidth != UNASSIGNED) _rockWidths.Add(_currentRockWidth);
        if (_currentRockDistance != UNASSIGNED) _rockDistances.Add(_currentRockDistance);
        _currentRockWidth = UNASSIGNED;
        _currentRockDistance = UNASSIGNED;
    }

    private void InitRock() {
        _currentRockWidth = 1;
        //Don't keep track of distance if there is no other rock
        if (_rockWidths.Count > 0) {
            _currentRockDistance = 0;
        }
    }

    private void ExpandRock() {
        if (_currentRockWidth != UNASSIGNED) {
            _currentRockWidth++;
        }        
        if (_currentRockDistance != UNASSIGNED) {
            _currentRockDistance++;
        }
    }

    private float SlopeBetweenTiles(TileData first, TileData second) {
        return SlopeBetweenPositions(first.GetPosition(), second.GetPosition());
    }

    private float SlopeBetweenPositions(Vector2 first, Vector2 second) {
        Vector2 direction = second - first;
        float angle = Vector2.Angle(direction, Vector2.right);
        if (second.y < first.y) {
            angle *= -1;
        }
        return angle;
    }

    private void SaveToExcel() {
        int row = ExcelHelper.GetEmptyRow(ExcelHelper.PLATFORMSHEET);

        //Todo: Fill Excel with Platform Data
        
        if (TraversabilityAnalyzer._type == TraversabilityAnalyzer.PCGType.CA) {
            CellularAutomataGenerator generator = TraversabilityAnalyzer._caGenerator;
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DIMENSIONS, row, generator.width + "x" + TraversabilityAnalyzer._caGenerator.height);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SEED, row, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_FILLAMOUNT, row, generator.fillAmount.ToString());
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BIRTHLIMIT, row, generator.birthLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DEATHLIMIT, row, generator.deathLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SMOOTHITERATIONS, row, generator.smoothIterations.ToString());
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BLENDLAYERS, row, generator.blendLayers.ToString());
        }
        else {
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DIMENSIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SEED, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BLENDLAYERS, row, "—");
        }

    }
}
