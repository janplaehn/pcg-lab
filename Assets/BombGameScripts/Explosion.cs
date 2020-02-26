using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    float timeUntilDisapper = 0.1f;

    private void OnEnable() {
        Invoke("Disappear", timeUntilDisapper);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Breakable")) {
            Destroy(collision.gameObject);
        }
    }

    void Disappear() {
        Destroy(gameObject);
    }

}
