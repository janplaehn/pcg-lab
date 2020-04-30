
using System.Collections.Generic;
using UnityEngine;

public class ChunkData {
    public List<TileData> _tiles;
    private float _slopeAngle = 0;
    private string _angleType = "";
    private int _smallDiscontinuities = 0;
    private int _largeDiscontinuities = 0;

    private TileData _first;
    private TileData _last;

    private readonly int MAX_DISCONTINUITY_SMALL = 1;
    public static readonly int CHUNKWIDTH = 7; //Should be an odd number [one in the middle + even amount on each side]
    private static readonly string FLAT = "Flat (0-30°)";
    private static readonly string SLOPED = "Sloped (0-60°)";
    private static readonly string STEEP = "Steep (60°-90°)";

    public ChunkData(List<TileData> tiles) {
        _tiles = tiles;
        _first = _tiles[0];
        _last = _tiles[_tiles.Count - 1];

        if (_tiles.Count < CHUNKWIDTH) {
            return;
        }
        Analyze();
        SaveToExcel();
    }

    private void Analyze() {
        _slopeAngle = GetSlopeAngle();

        if (_slopeAngle <= 30) {
            _angleType = FLAT;
        }
        else if (_slopeAngle <= 60) {
            _angleType = SLOPED;
        }
        else {
            _angleType = STEEP;
        }

        foreach (TileData t in _tiles) {
            AddDiscontinuity(t);
        }
    }

    private float GetSlopeAngle() {
        float yDifference = Mathf.Abs(_last._yPos - _first._yPos);
        float xDifference = Mathf.Abs(_last._xPos - _first._xPos);
        Vector2 slope = new Vector2(xDifference, yDifference);        
        return Vector2.Angle(Vector2.right, slope);
    }

    private void AddDiscontinuity(TileData tile) {
        int lowestY = Mathf.Min(_first._yPos, _last._yPos);
        if (tile._yPos >= lowestY) {
            return;
        }
        if (lowestY - tile._yPos <= MAX_DISCONTINUITY_SMALL) {
            _smallDiscontinuities++;
        }
        else {
            _largeDiscontinuities++;
        }
    }

    private void SaveToExcel() {
        int row = ExcelHelper.GetEmptyRow(ExcelHelper.CHUNKSHEET);

        ExcelHelper.WriteData(ExcelHelper.CHUNK_TEST_NUMBER, row, TraversabilityAnalyzer._testNumber.ToString());
        ExcelHelper.WriteData(ExcelHelper.ANGLE, row, _slopeAngle.ToString());
        ExcelHelper.WriteData(ExcelHelper.ANGLE_TYPE, row, _angleType);
        ExcelHelper.WriteData(ExcelHelper.SMALL_DISCONTINUITIES, row, _smallDiscontinuities.ToString());
        ExcelHelper.WriteData(ExcelHelper.LARGE_DISCONTINUITIES, row, _largeDiscontinuities.ToString());

        ExcelHelper.WriteData(ExcelHelper.CHUNK_PCG_TYPE, row, TraversabilityAnalyzer._pcgType.ToString());
        ExcelHelper.WriteData(ExcelHelper.CHUNK_DIMENSIONS, row, TraversabilityAnalyzer._width + "x" + TraversabilityAnalyzer._height);

        if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.CA) {
            CellularAutomataGenerator generator = TraversabilityAnalyzer._caGenerator;
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SEED, row, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_FILLAMOUNT, row, generator.fillAmount.ToString());
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BIRTHLIMIT, row, generator.birthLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.CHUNK_DEATHLIMIT, row, generator.deathLimit.ToString());
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SMOOTHITERATIONS, row, generator.smoothIterations.ToString());
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BLENDLAYERS, row, generator.blendLayers.ToString());
        }
        else if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.WFC) {
            OverlapWFC generator = TraversabilityAnalyzer._wfcGenerator;

            ExcelHelper.WriteData(ExcelHelper.CHUNK_DIMENSIONS, row, generator.width + "x" + generator.depth);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SEED, row, generator.seed.ToString());
            ExcelHelper.WriteData(ExcelHelper.CHUNK_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BLENDLAYERS, row, "—");
        }
        else {            
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SEED, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_FILLAMOUNT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BIRTHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_DEATHLIMIT, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SMOOTHITERATIONS, row, "—");
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BLENDLAYERS, row, "—");
        }

    }

}
