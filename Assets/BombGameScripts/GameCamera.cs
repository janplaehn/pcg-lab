using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameCamera : MonoBehaviour {

    public float _zoom = 1f;
    public float _minZoom = 8f;
    public float _lerpSpeed = 0.2f;
    public Vector2 cameraBoundOffset = new Vector2(5,0);

    private Transform[] _players;

    void Start() {
        GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag("Player");
        _players = new Transform[playerGameObjects.Length];
        for (int i = 0; i < playerGameObjects.Length; i++) {
            _players[i] = playerGameObjects[i].transform;
        }
    }

    void Update() {
        FocusOnPlayers();
    }

    private void FocusOnPlayers() {
        float maxX = -Mathf.Infinity, minX = Mathf.Infinity, maxY = -Mathf.Infinity, minY = Mathf.Infinity;
        foreach (Transform player in _players) {
            maxX = Mathf.Max(player.position.x, maxX);
            maxY = Mathf.Max(player.position.y, maxY);
            minX = Mathf.Min(player.position.x, minX);
            minY = Mathf.Min(player.position.y, minY);
        }
        //minY = Mathf.Max(minY, LevelGenerator.minY - cameraBoundOffset.y);
        //minX = Mathf.Max(minX, LevelGenerator.minX - cameraBoundOffset.x);
        //maxY = Mathf.Min(maxY, LevelGenerator.maxY + cameraBoundOffset.y);
        //maxX = Mathf.Min(maxX, LevelGenerator.maxX + cameraBoundOffset.x);

        Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        float ratio = Screen.height / Screen.width;
        float x = (maxX - minX) * ratio;
        float y = maxY - minY;
        float z = (new Vector2(x, y)).magnitude * _zoom;

        z = Mathf.Max(z, _minZoom);
        Vector3 targetPosition = new Vector3(center.x, center.y, -z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, _lerpSpeed * Time.deltaTime);


        Debug.DrawLine(new Vector2(minX, minY), new Vector2(minX, maxY), Color.cyan);
        Debug.DrawLine(new Vector2(minX, minY), new Vector2(maxX, minY), Color.cyan);
        Debug.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, maxY), Color.cyan);
        Debug.DrawLine(new Vector2(maxX, minY), new Vector2(maxX, maxY), Color.cyan);
    }

    public void Reset() {
            float maxX = -Mathf.Infinity, minX = Mathf.Infinity, maxY = -Mathf.Infinity, minY = Mathf.Infinity;
            foreach (Transform player in _players) {
                maxX = Mathf.Max(player.position.x, maxX);
                maxY = Mathf.Max(player.position.y, maxY);
                minX = Mathf.Min(player.position.x, minX);
                minY = Mathf.Min(player.position.y, minY);
            }
            //minY = Mathf.Max(minY, LevelGenerator.minY - cameraBoundOffset.y);
            //minX = Mathf.Max(minX, LevelGenerator.minX - cameraBoundOffset.x);
            //maxY = Mathf.Min(maxY, LevelGenerator.maxY + cameraBoundOffset.y);
            //maxX = Mathf.Min(maxX, LevelGenerator.maxX + cameraBoundOffset.x);

            Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
            float ratio = Screen.height / Screen.width;
            float x = (maxX - minX) * ratio;
            float y = maxY - minY;
            float z = (new Vector2(x, y)).magnitude * _zoom;

            z = Mathf.Max(z, _minZoom);
            Vector3 targetPosition = new Vector3(center.x, center.y, -z);
            transform.position = targetPosition;
    }
}
