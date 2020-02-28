using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class CAMapGenerator : MonoBehaviour {

    public CellularAutomataGenerator caGenerator = null;
    public bool buildOnAwake = false;
    public int cellWidth = 1;
    public int cellHeight = 1;

    public Transform backgroundTile = null;
    public Tile[] tiles = new Tile[16];

    [System.Serializable]
    public struct Tile {
        public string name;
        [Header("                                               Left    Right  Top    Bottom")]
        public TileNeighbours neighbours;
        public Transform tilePrefab;
    }

    private int width = 0;
    private int height = 0;

    [System.Serializable]
    public struct TileNeighbours {
        public bool left, right, top, bottom;
    }

    private void Awake() {
       if (buildOnAwake) {
            BuildMap();
        }
    }

    public void BuildMap() {
        caGenerator.Generate();
        width = caGenerator.GetWidth();
        height = caGenerator.GetHeight();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                SetTile(x, y);
            }
        }
    }

    private void SetTile(int x, int y) {
        Vector3 position = new Vector3((-width / 2 + x + .5f) * cellWidth, (-height / 2 + y + .5f) * cellHeight, 0);

        Instantiate(backgroundTile, position, Quaternion.identity, transform);
        if (caGenerator.GetTile(x, y) == 0) {
            return;
        }

        string neighbours = caGenerator.GetNeighboursAsBinary(x, y);
        Transform tile = GetPrefabFromBinary(neighbours);
        Instantiate(tile, position, Quaternion.identity, transform);
    }

    public Transform GetPrefabFromBinary(string neighbours) {
        foreach (Tile t in tiles) {
            if (t.name == neighbours) {
                if (!t.tilePrefab) Debug.LogWarning("Tile Not found: " + neighbours);
                return t.tilePrefab;
            }
        }
        Debug.LogWarning("Tile Not found: " + neighbours);
        return tiles[0].tilePrefab;
    }

    private void OnValidate() {
        for (int i = 0; i < tiles.Length; i++) {
            string binaryString = GetBinaryString(i, 4);
            tiles[i].neighbours.left = (binaryString[0] == "1"[0]);
            tiles[i].neighbours.right = (binaryString[1] == "1"[0]);
            tiles[i].neighbours.top = (binaryString[2] == "1"[0]);
            tiles[i].neighbours.bottom = (binaryString[3] == "1"[0]);
            tiles[i].name = binaryString;
        }
    }

    private string GetBinaryString(int number, int digits) {
        string binaryString = System.Convert.ToString(number, 2);
        int numbersToAdd = digits - binaryString.Length;
        for (int j = 0; j < numbersToAdd; j++) {
            binaryString = "0" + binaryString;
        }
        return binaryString;
    }

    
}
