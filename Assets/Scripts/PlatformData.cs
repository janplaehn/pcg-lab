using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData {

    public List<TileData> _tiles;
    public List<ChunkData> _chunks = new List<ChunkData>();

    public float _platformSlope = 0;

    public List<float> _rockWidths = new List<float>();
    public List<float> _rockDistances = new List<float>();
    public int _smallRocks = 0;
    public int _largeRocks = 0;

    public const int MAXSMALLROCKSIZE = 4;

    public enum Roughness {
        SMOOTH,
        ROUGH,
        BUMPY,
        ROCKY
    }
    public Roughness _roughness = Roughness.SMOOTH;

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
        CalculateRockData();
        CalculateRoughness();
        SaveToExcel();
    }

    public enum Density { FEW, MANY };
    public enum Separation { FAR, CLOSE };

    private void CalculateRoughness() {
        Density smallRockDensity, largeRockDensity;
        Separation separationDistance;

        //Small Rock Density
        if (GetSmallRockDensity() < 0.1f) {
            smallRockDensity = Density.FEW;
        }
        else {
            smallRockDensity = Density.MANY;
        }

        //Large Rock Density
        if (GetLargeRockDensity() < 0.1f) {
            largeRockDensity = Density.FEW;
        }
        else {
            largeRockDensity = Density.MANY;
        }

        //Separation
        if (GetAverageRockDistance() == UNASSIGNED) {
            separationDistance = Separation.FAR;
        }
        else if (GetAverageRockDistance() < 7) {
            separationDistance = Separation.CLOSE;
        }
        else {
            separationDistance = Separation.FAR;
        }

        if (smallRockDensity == Density.FEW && largeRockDensity == Density.FEW) {
            _roughness = Roughness.SMOOTH;
        }
        else if (smallRockDensity == Density.MANY && largeRockDensity == Density.FEW) {
            _roughness = Roughness.ROUGH;
        }
        else if (largeRockDensity == Density.MANY && separationDistance == Separation.FAR) {
            _roughness = Roughness.BUMPY;
        }
        else {
            _roughness = Roughness.ROCKY;
        }
    }

    private List<TileData> GetChunkTiles(int index) {
        int start, end, amount;
        start = index;
        end = index + (ChunkData.CHUNKWIDTH - 1);
        end = Mathf.Clamp(end, 0, _tiles.Count - 1);
        amount = (end - start) + 1;
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
    private void CalculateRockData() { //Todo: Clean up!
        AngleOffset offset = AngleOffset.NEUTRAL;
        TileData startTile = _tiles[0];
        TileData endTile = _tiles[_tiles.Count - 1];
        _platformSlope = SlopeBetweenTiles(startTile, endTile);
        for (int i = 0; i < _tiles.Count - 1; i++) {
            float distance = 0;
            TileData first = _tiles[i];
            TileData second = _tiles[i+1];
            if (first != startTile) {
                TileData previous = _tiles[i - 1];
                distance = (first.GetPosition() - previous.GetPosition()).magnitude;
            }
            float tileSlope = SlopeBetweenTiles(first, second);

            AngleOffset previousOffset = offset;
            offset = GetAngleOffset(tileSlope);

            switch (offset) {
                case AngleOffset.NEUTRAL:
                    if (previousOffset == AngleOffset.NEGATIVE) {
                        SaveRock();
                    }                   
                    break;
                case AngleOffset.POSITIVE:
                    if (previousOffset != AngleOffset.POSITIVE) {
                        SaveRock();
                        InitRock();
                    }
                    break;
                case AngleOffset.NEGATIVE:
                    break;
                default:
                    break;
            }

            if (_currentRockWidth != UNASSIGNED) {
                ExpandRock(distance);
            }
            if (_currentRockDistance != UNASSIGNED) {
                ExpandDistance(distance);
            }
        }

        //Remove last distance if no last rock assigned
        if (_rockDistances.Count > 0 && _rockDistances.Count >= _rockWidths.Count) {
            int last = _rockDistances.Count - 1;
            _rockDistances.RemoveAt(last);
        }
    }

    private AngleOffset GetAngleOffset(float tileSlope) {
        if (tileSlope > _platformSlope) {
            return AngleOffset.POSITIVE;
        }
        if (tileSlope < _platformSlope) {
            return AngleOffset.NEGATIVE;
        }
        return AngleOffset.NEUTRAL;
    }

    private void SaveRock() {
        if (_currentRockWidth != UNASSIGNED) {
            _rockWidths.Add(_currentRockWidth);
            if (_rockDistances.Count > 0) {
                _rockDistances[_rockDistances.Count - 1] += _currentRockWidth / 2; //Add the rock radius to the last saved rockdistance;
            }
            if (_currentRockWidth <= MAXSMALLROCKSIZE) {
                _smallRocks++;
            }
            else {
                _largeRocks++;
            }
            _currentRockDistance = 0;
        }
        _currentRockWidth = UNASSIGNED;       
    }

    private void InitRock() {
        if (_currentRockDistance != UNASSIGNED) {
            float distance = (_rockWidths[_rockWidths.Count - 1]/2) + _currentRockDistance; //Add the radius of the last rock
            _rockDistances.Add(distance);
        }
        _currentRockWidth = 0;
        _currentRockDistance = UNASSIGNED;
    }

    private void ExpandRock(float distance) {       
        _currentRockWidth += distance;           
    }

    private void ExpandDistance(float distance) {
        _currentRockDistance += distance;
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

    private float GetAverageRockDistance() {
        if (_rockDistances.Count == 0) {
            return -1;
        }
        float average = 0;
        foreach (float distance in _rockDistances) {
            average += distance;
        }
        return (average / _rockDistances.Count);
    }

    private float GetSmallRockDensity() {
        if (_smallRocks == 0) return 0;
        return (float)_smallRocks / (float)_tiles.Count;
    }

    private float GetLargeRockDensity() {
        if (_largeRocks == 0) return 0;
        return (float)_largeRocks / (float)_tiles.Count;
    }

    private void SaveToExcel() {
        int row = ExcelHelper.GetEmptyRow(ExcelHelper.PLATFORMSHEET);
        ExcelHelper.SetRow(row);
        ExcelHelper.WriteData(ExcelHelper.PLATFORM_TEST_NUMBER, TraversabilityAnalyzer._testNumber);
        ExcelHelper.WriteData(ExcelHelper.PLATFORM_TILECOUNT, _tiles.Count);

        if (_rockDistances.Count > 0) {
            ExcelHelper.WriteData(ExcelHelper.ROCKDISTANCE, GetAverageRockDistance());
        }
        else {
            ExcelHelper.WriteData(ExcelHelper.ROCKDISTANCE, "—");
        }
        ExcelHelper.WriteData(ExcelHelper.SMALLROCKDENSITY, GetSmallRockDensity());
        ExcelHelper.WriteData(ExcelHelper.LARGEROCKDENSITY, GetLargeRockDensity());
        ExcelHelper.WriteData(ExcelHelper.ROUGHNESS, _roughness);

        ExcelHelper.WriteData(ExcelHelper.PLATFORM_PCG_TYPE, TraversabilityAnalyzer._pcgType);
        ExcelHelper.WriteData(ExcelHelper.PLATFORM_DIMENSIONS, TraversabilityAnalyzer._width + "x" + TraversabilityAnalyzer._height);

        if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.CA) {
            CellularAutomataGenerator generator = TraversabilityAnalyzer._caGenerator;
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SEED, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_FILLAMOUNT, generator.fillAmount);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BIRTHLIMIT, generator.birthLimit);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DEATHLIMIT, generator.deathLimit);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SMOOTHITERATIONS, generator.smoothIterations);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BLENDLAYERS, generator.blendLayers);
        }
        else if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.WFC) {
            OverlapWFC generator = TraversabilityAnalyzer._wfcGenerator;
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SEED, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_FILLAMOUNT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BIRTHLIMIT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DEATHLIMIT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SMOOTHITERATIONS, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BLENDLAYERS, "—");
        }
        else {
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SEED, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_FILLAMOUNT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BIRTHLIMIT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_DEATHLIMIT, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_SMOOTHITERATIONS, "—");
            ExcelHelper.WriteData(ExcelHelper.PLATFORM_BLENDLAYERS, "—");
        }

    }
}
