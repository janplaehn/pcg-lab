using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CellularAutomataGenerator : MonoBehaviour
{
    public enum BlendMode {Maximum, Minimum};

    [Header("Tiles")]
    public Transform centerTile;

    [Header("Dimensions")]
    public int width = 60;
    public int height = 80;

    [Header("Generation Settings")]
    public string seed = "Mexico";
    public bool useRandomSeed = false;
    [Range(0,100)] public int fillAmount = 56;
    public int birthLimit = 4;
    public int deathLimit = 4;
    public int smoothIterations = 3;
    public BlendMode blendMode = BlendMode.Minimum;
    [Range(1, 20)] public int blendLayers = 5;
    public int centerCircleRadius = 5;

    private int[,] map = null;
    private int[,] layeredMap = null;
    private bool isLayered = false;
    private int currentLayer = 0;
    private string currentSeed = "";

    public void Generate() {
        isLayered = false;
        map = new int[width, height];
        layeredMap = null;
        currentLayer = 0;
        currentSeed = "";
        while (transform.childCount != 0) {
            foreach (Transform child in transform) {
                DestroyImmediate(child.gameObject);
            }
        }
        for (int i = 0; i < blendLayers; i++) {
            GenerateMap();
        }
    }

    public void Analyze() {
        TraversabilityAnalyzer.Analyze(map, width, height, this);
    }

    private void GenerateMap() {
        layeredMap = map;
        map = new int[width, height];
        FillMapRandomly();
        for (int i = 0; i < smoothIterations; i++) {
            SmoothMap();
        }
        if (isLayered) {
            BlendMaps();
        }
        isLayered = true;
        currentLayer++;
    }

    private void BlendMaps() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (blendMode == BlendMode.Maximum) {
                    if (map[x,y] == 1 || layeredMap[x,y] == 1) {
                        map[x, y] = 1;
                    }
                    else {
                        map[x, y] = 0;
                    }
                }
                if (blendMode == BlendMode.Minimum) {
                    if (map[x, y] == 0 || layeredMap[x, y] == 0) {
                        map[x, y] = 0;
                    }
                    else {
                        map[x, y] = 1;
                    }
                }
            }
        }
    }

    void FillMapRandomly() {
        SetSeed();
        System.Random random = new System.Random(currentSeed.GetHashCode());

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                SetTileRandomly(x, y, random);
            }
        }
    }

    private void SetSeed() {
        currentSeed = seed + currentLayer.ToString();
        if (useRandomSeed) {
            currentSeed = Time.time.ToString() + currentLayer.ToString();
        }

    }

    private void SetTileRandomly(int x, int y, System.Random random) {
        if (x == 0 || x == width-1 || y == 0 || y == height-1) { // Sets the edge tiles to Wall tiles
            map[x, y] = 1;
            return;
        }
        if (GetDistanceToCenter(x,y) <= centerCircleRadius) {
            map[x, y] = 0;
            return;
        }

        int r = random.Next(0, 100);
        map[x, y] = (r < fillAmount) ? 1 : 0; //if it is smaller, pick 1, otherwise 0
    }

    private void SmoothMap() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > birthLimit) {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < deathLimit) {
                    map[x, y] = 0;
                }
            }
        }
    }

    private float GetDistanceToCenter(int x, int y) {
        Vector2Int position = new Vector2Int(x, y);
        Vector2Int center = GetCenter();
        float distance = (position - center).magnitude;
        return distance;
    }

    private Vector2Int GetCenter() {
        int centerX = GetWidth() / 2;
        int centerY = GetHeight() / 2;
        return new Vector2Int(centerX, centerY);
    }

    private int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX -1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY -1; neighbourY <= gridY + 1; neighbourY++) {
                if (neighbourX != gridX || neighbourY != gridY) {
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
                        wallCount += map[neighbourX, neighbourY];
                    }
                    else {
                        wallCount++; //If on edge, return wallCount to keep edges walls
                    }
                }
            }
        }
        return wallCount;
    }

    public int GetTile(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) {
            return 1;
        }
        return map[x, y];
    }

    public int GetWidth() {
        return width;
    }
    public int GetHeight() {
        return height;
    }

    public string GetNeighboursAsBinary(int x, int y) {
        string neighbours;
        string left, right, top, bottom;
        left = GetTile(x - 1, y).ToString();
        right = GetTile(x + 1, y).ToString();
        top = GetTile(x, y + 1).ToString();
        bottom = GetTile(x, y - 1).ToString();
        neighbours = left + right + top + bottom;
        return neighbours;
    }
}
