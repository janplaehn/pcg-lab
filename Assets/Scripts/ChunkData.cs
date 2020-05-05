
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
        ExcelHelper.SetRow(row);
        ExcelHelper.WriteData(ExcelHelper.CHUNK_TEST_NUMBER, TraversabilityAnalyzer._testNumber);
        ExcelHelper.WriteData(ExcelHelper.ANGLE, _slopeAngle);
        ExcelHelper.WriteData(ExcelHelper.ANGLE_TYPE, _angleType);
        ExcelHelper.WriteData(ExcelHelper.SMALL_DISCONTINUITIES, _smallDiscontinuities);
        ExcelHelper.WriteData(ExcelHelper.LARGE_DISCONTINUITIES, _largeDiscontinuities);

        ExcelHelper.WriteData(ExcelHelper.CHUNK_PCG_TYPE, TraversabilityAnalyzer._pcgType);
        ExcelHelper.WriteData(ExcelHelper.CHUNK_DIMENSIONS, TraversabilityAnalyzer._width + "x" + TraversabilityAnalyzer._height);

        if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.CA) {
            CellularAutomataGenerator generator = TraversabilityAnalyzer._caGenerator;
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SEED, generator.seed);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_FILLAMOUNT, generator.fillAmount);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BIRTHLIMIT, generator.birthLimit);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_DEATHLIMIT, generator.deathLimit);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SMOOTHITERATIONS, generator.smoothIterations);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_BLENDLAYERS, generator.blendLayers);
        }
        else if (TraversabilityAnalyzer._pcgType == TraversabilityAnalyzer.PCGType.WFC) {
            OverlapWFC generator = TraversabilityAnalyzer._wfcGenerator;

            ExcelHelper.WriteData(ExcelHelper.CHUNK_DIMENSIONS, generator.width + "x" + generator.depth);
            ExcelHelper.WriteData(ExcelHelper.CHUNK_SEED, generator.seed);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_FILLAMOUNT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_BIRTHLIMIT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_DEATHLIMIT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_SMOOTHITERATIONS);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_BLENDLAYERS);
        }
        else {            
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_SEED);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_FILLAMOUNT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_BIRTHLIMIT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_DEATHLIMIT);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_SMOOTHITERATIONS);
            ExcelHelper.WriteUnassigned(ExcelHelper.CHUNK_BLENDLAYERS);
        }

    }

}
