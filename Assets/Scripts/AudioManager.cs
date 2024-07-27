using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bonk1;
    public AudioSource bonk2;
    public AudioSource bonk3;
    public AudioSource bonk4;
    public AudioSource turnswitch;
    public AudioSource win;
    public AudioSource lose;
    public AudioSource tie;
    public AudioSource click;

    void Start()
    {
        if (instance) {
            Destroy(gameObject);
        } else {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void LateUpdate() {
        transform.position = Camera.main.transform.position;
    }

    public void BonkBalls() {
        if(Random.Range(0.0f, 1.0f) < 0.5f) {
            if(!bonk1.isPlaying)
                bonk1.Play();
        } else {
            if (!bonk2.isPlaying)
                bonk2.Play();
        }
    }

    public void Bonk() {
        if (Random.Range(0.0f, 1.0f) < 0.5f) {
            if (!bonk3.isPlaying)
                bonk3.Play();
        } else {
            if (!bonk4.isPlaying)
                bonk4.Play();
        }
    }

    public void Click() {
        click.Play();
    }
}
