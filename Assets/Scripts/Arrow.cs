using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Arrow : MonoBehaviour {

    public float Shift { get => _mat.GetFloat("_Shift"); set => _mat.SetFloat("_Shift", value); }
    public Vector3 Scale { get; private set; }

    public float shootAnimationDuration = 1.0f;
    public float shootAnimationDistance = 1.0f;

    private Material _mat;
    private MeshRenderer _meshRenderer;
    private bool _isReady = false;
    private bool _isFake;

    void Start() {
        _meshRenderer = GetComponent<MeshRenderer>();
        _mat = _meshRenderer.material;
        Scale = transform.localScale;

        _mat.color = Color.clear;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void Shoot(float speed) {
        if (_isReady) {
            _isReady = false;
            StartCoroutine(ShootAnimation(speed));
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public void Ready(bool fake) {
        _isFake = fake;
        _mat.color = fake ? new Color(1.0f, 1.0f, 1.0f, 0.5f) : Color.white;
        _mat.SetFloat("_Shift", 0.0f);
        _mat.SetFloat("_Fade", fake ? 1.0f : 0.0f);
        transform.localScale = Scale;
        _meshRenderer.shadowCastingMode = fake ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
        _isReady = true;
    }

    private IEnumerator ShootAnimation(float speed) {
        float t = 0.0f;
        Vector3 pos = transform.position;
        Vector3 direction = transform.forward;

        while (t < 1.0f) {
            _mat.color = new Color(1.0f, 1.0f, 1.0f, (_isFake ? 0.5f : 1.0f) * 1.0f - t);
            transform.position = pos + direction * shootAnimationDistance * (1.0f - (1.0f - t) * (1.0f - t));

            yield return null;
            t += speed * Time.deltaTime / shootAnimationDuration;
        }

        _mat.color = Color.clear;
    }
}
