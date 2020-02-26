using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public float _timeUntilExplode = 3f;
    public Transform _explosion = null;

    private float _currentTime;
    private void OnEnable() {
        _currentTime = _timeUntilExplode;
    }

    private void FixedUpdate() {
        _currentTime -= Time.deltaTime;
        if (_currentTime < 0) {
            _currentTime = 0;
            Explode();
        }
    }

    private void Explode() {
        Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Bomb")) {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Explosion")) {
            Explode();
        }
    }
}
