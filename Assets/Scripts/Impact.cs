using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Impact : MonoBehaviour
{
    public List<Texture> textures = new List<Texture>();
    public float lifeSpan = 1.0f;

    private Material _mat;
    private float _t = 0.0f;

    private void Start() {
        _t = 0.0f;
        _mat = GetComponent<MeshRenderer>().material;
    }

    private void Update() {
        _t += Time.deltaTime / lifeSpan;

        if(_t >= 1.0f) {
            Destroy(gameObject);
        } else {
            int tex = Mathf.FloorToInt(_t * textures.Count);
            _mat.mainTexture = textures[tex];
        }
    }
}
