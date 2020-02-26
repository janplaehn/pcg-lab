using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGroup : MonoBehaviour {

    private ParticleSystem[] _particles;
    public KeyCode _key = KeyCode.Space;

    private void Update() {
        if (Input.GetKeyDown(_key)) {
            Play();
        }
    }

    public void Play() {
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in _particles) {
            particle.Play();
        }
    }
}
