using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkData {
    public List<TileData> _tiles;
    private float _slopeAngle = 0;
    private int _smallDiscontinuities = 0;
    private int _largeDiscontinuities = 0;

    private TileData _first;
    private TileData _last;

    private readonly int MAX_DISCONTINUITY_SMALL = 1;
    public static readonly int CHUNKWIDTH = 5; //Should be an odd number [one in the middle + even amount on each side]

    //Google Form URL
    private static readonly string CHUNK_FORM_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfihbhg0H--NMTjeRagKCrJlwEEj3taB-PduJnq8TqQQOGwpw/formResponse";

    //The Keys accosiated with eah of the different questions in the form
    private static readonly string PCG_TYPE_KEY = "entry.332652072";
    private static readonly string SLOPE_KEY = "entry.1299467495";
    private static readonly string SMALL_DISCONTINUITY_KEY = "entry.4193981";
    private static readonly string LARGE_DISCONTINUITY_KEY = "entry.1808954004";

    //The Entries and Options in the Google form
    private static readonly string CA_ENTRY = "Cellular Automata";
    private static readonly string WFC_ENTRY = "Wave Function Collapse";
    private static readonly string FLAT_ENTRY = "Flat (0-30°)";
    private static readonly string SLOPED_ENTRY = "Sloped (0-60°)";
    private static readonly string STEEP_ENTRY = "Steep (60°-90°)";

    public ChunkData(List<TileData> tiles) {
        _tiles = tiles;
        _first = _tiles[0];
        _last = _tiles[_tiles.Count - 1];

        if (_tiles.Count < CHUNKWIDTH) {
            return;
        }
        Analyze();
        SendData();
    }

    private void Analyze() {
        _slopeAngle = GetSlopeAngle();
        foreach (TileData t in _tiles) {
            AddDiscontinuity(t);
        }
    }

    private float GetSlopeAngle() {
        float yDifference = Mathf.Abs(_first._yPos - _last._yPos);
        float xDifference = Mathf.Abs(_first._xPos - _last._xPos);
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

    private void SendData() {
        GoogleFormsSubmissionService form = new GoogleFormsSubmissionService(CHUNK_FORM_URL);

        //PCG Type
        if (TraversabilityAnalyzer._type == TraversabilityAnalyzer.PCGType.CA) {
            form.SetCheckboxValues(PCG_TYPE_KEY, CA_ENTRY);
        }
        else if (TraversabilityAnalyzer._type == TraversabilityAnalyzer.PCGType.WFC) {
            form.SetCheckboxValues(PCG_TYPE_KEY, WFC_ENTRY);
        }
        
        //Angle
        if (_slopeAngle <= 30) {
            form.SetCheckboxValues(SLOPE_KEY, FLAT_ENTRY);
        }
        else if (_slopeAngle <= 60) {
            form.SetCheckboxValues(SLOPE_KEY, SLOPED_ENTRY);
        }
       else {
            form.SetCheckboxValues(SLOPE_KEY, STEEP_ENTRY);
        }

        //Discontinuities
        form.SetCheckboxValues(SMALL_DISCONTINUITY_KEY, _smallDiscontinuities.ToString());
        form.SetCheckboxValues(LARGE_DISCONTINUITY_KEY, _largeDiscontinuities.ToString());


        form.Submit();
    }

}
