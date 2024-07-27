using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public Transform finish;

    public bool isAi = false;
    public Rigidbody fakeBall;
    private IEnumerator _aiEnumerator;
    private List<Rigidbody> _fakeBalls;
    private List<Vector4> _fakeBallShots;

    public bool Rolling => !activeInput;
    public bool FinishedRolling { get; private set; } = false;


    public GameObject impact;

    public Arrow arrow;
    public float arrowDistance = 0.2f;
    public float maxDrag = 5.0f;
    public float dragScaleCoefficient = 0.2f;
    public bool activeInput = false;

    public float minForce = 1.0f;
    public float maxForce = 10.0f;

    public float minVelocity = 0.01f;
    public float stabilityTime = 0.2f;

    private bool _dragging = false;
    private float _dragOrigin = 0.0f;

    private float _currentStabilityTime = 0.0f;
    private Rigidbody _rigidbody;
    private Vector3 _respawnPoint;
    private Material _mat;
    private Matrix4x4 _squishMatrix;
    private Vector3 _prevVelocity;


    private void Start() {
        _fakeBalls = null;
        _mat = GetComponent<MeshRenderer>().material;
        _mat.SetFloat("_HasSquishMatrix", 1.0f);

        _rigidbody = GetComponent<Rigidbody>();
        _respawnPoint = transform.position;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.position = _respawnPoint + Vector3.up * 0.5f;

        _dragging = false;
        activeInput = false;
        FinishedRolling = false;
    }

    void Update()
    {
        if (activeInput) {
            if (!isAi) {
                UpdateInputs();
            }

            // update all arrows
            if (GameManager.instance) {
                foreach (var ball in GameManager.instance.balls) {
                    ball.arrow.transform.rotation = arrow.transform.rotation;
                    ball.arrow.transform.localPosition = arrow.transform.localPosition;
                    ball.arrow.transform.localScale = arrow.transform.localScale;
                    ball.arrow.Shift = arrow.Shift;
                }
            }
        }

        CheckForDeath();

        Vector3 scaleVector = _rigidbody.velocity;
        Matrix4x4 lookat = Matrix4x4.LookAt(Vector3.zero, scaleVector, Vector3.up);
        if(scaleVector.x == 0.0f && scaleVector.z == 0.0f) {
            lookat = Matrix4x4.LookAt(Vector3.zero, Vector3.up, Vector3.left);
        }
        float scale = 2.0f - Mathf.Exp(-scaleVector.magnitude * 0.55f);
        _squishMatrix = lookat * Matrix4x4.Scale(new Vector3(1.0f / Mathf.Sqrt(scale), 1.0f / Mathf.Sqrt(scale), scale)) * lookat.inverse;
    }

    public void Shoot(Vector3 direction, float intensity) {
        arrow.Shoot(Mathf.Exp(2.0f * intensity));
        _dragging = false;
        activeInput = false;
        FinishedRolling = false;
        _rigidbody.AddForce(direction * Mathf.Lerp(minForce, maxForce, intensity), ForceMode.VelocityChange);
    }

    private void LateUpdate() {
        transform.rotation = Quaternion.identity;

        if (!activeInput && !FinishedRolling)
            CheckForTurnEnd();

        _mat.SetMatrix("_SquishMatrix", _squishMatrix);
    }

    private void FixedUpdate() {
        if (Vector3.Dot(_prevVelocity, _rigidbody.velocity) < -0.5f) {
            AudioManager.instance.Bonk();

            var obj = Instantiate(impact);
            obj.transform.position = transform.position;
        }

        _prevVelocity = _rigidbody.velocity;
    }

    void CheckForDeath() {
        if(transform.position.y < -10.0f) {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.position = _respawnPoint + Vector3.up * 0.5f;
        }
    }

    void CheckForTurnEnd() {
        Vector3 vel = _rigidbody.velocity;
        if(vel.sqrMagnitude < minVelocity * minVelocity &&
            Physics.Raycast(transform.position, Vector3.down, 0.05f, ~2)) {
            _currentStabilityTime += Time.deltaTime;
        } else {
            _currentStabilityTime = 0.0f;
        }

        if(_currentStabilityTime > stabilityTime) {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            FinishedRolling = true;
        }
    }

    public void ReadyOpponentTurn() {
        arrow.Ready(true);
    }

    public void Ready() {
        arrow.Ready(false);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        activeInput = true;
        _currentStabilityTime = 0.0f;
        FinishedRolling = false;

        if(isAi) {
            StartCoroutine(AiTurn());
        }
    }

    private IEnumerator AiTurn() {
        if(_fakeBalls == null) {
            _fakeBalls = new List<Rigidbody>();
            _fakeBallShots = new List<Vector4>();

            for(int i = 0; i < 100; i++) {
                _fakeBalls.Add(Instantiate(fakeBall));
                _fakeBallShots.Add(Vector4.zero);
            }
        }

        for(int i =0; i < _fakeBalls.Count; i++) {
            float random = Random.Range(0.0f, Mathf.PI * 2.0f);
            float intensity = Random.Range(0.0f, 1.0f);
            if (Random.Range(0.0f, 1.0f) < 0.5f)
                intensity = 1.0f;
            Vector3 targetDirection = new Vector3(Mathf.Sin(random), 0.0f, Mathf.Cos(random));

            _fakeBalls[i].transform.position = transform.position;
            _fakeBalls[i].AddForce(targetDirection * Mathf.Lerp(minForce, maxForce, intensity), ForceMode.VelocityChange);
            _fakeBallShots[i] = new Vector4(targetDirection.x, targetDirection.y, targetDirection.z, intensity);
        }

        Physics.autoSimulation = false;
        for(int i = 0; i < 30; ++i) {
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.Simulate(Time.fixedDeltaTime);
            yield return null;
        }
        Physics.autoSimulation = true;

        float bestScore = float.MaxValue;
        Vector4 bestShot = Vector4.zero;

        for(int i = 0; i < _fakeBalls.Count; i++) {
            float score = (_fakeBalls[i].transform.position - finish.position).sqrMagnitude;
            if(score < bestScore) {
                bestScore = score;
                bestShot = _fakeBallShots[i];
            }
        }
        Debug.Log(bestScore);

        float arrowRotation = Mathf.Atan2(bestShot.x, bestShot.z);
        arrow.transform.rotation = Quaternion.Euler(0.0f, Mathf.Rad2Deg * arrowRotation, 0.0f);
        arrow.transform.localPosition = bestShot * arrowDistance;

        float t = 0.0f;
        while (t < 1.0f) {
            yield return null;
            t += Time.deltaTime;

            arrow.transform.localScale = arrow.Scale * Mathf.Exp(dragScaleCoefficient * bestShot.w * t);
            arrow.Shift = bestShot.w * t;
        }
        yield return new WaitForSeconds(0.2f);

        foreach (var ball in GameManager.instance.balls)
            ball.Shoot(bestShot, bestShot.w);
    }

    void UpdateInputs() {
        // get input direction
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = (transform.position.y - ray.origin.y) / ray.direction.y;
        Vector3 mousePos = ray.origin + ray.direction * distance;
        Vector3 targetDirection = transform.position - mousePos;
        targetDirection.y = 0.0f;
        float dragDistance = targetDirection.magnitude;
        targetDirection /= dragDistance;

        float dragAmount = Mathf.Clamp01((dragDistance - _dragOrigin) / maxDrag);

        // update arrow
        float arrowRotation = Mathf.Atan2(targetDirection.x, targetDirection.z);
        arrow.transform.rotation = Quaternion.Euler(0.0f, Mathf.Rad2Deg * arrowRotation, 0.0f);
        arrow.transform.localPosition = targetDirection * arrowDistance;

        // drag start
        if (!_dragging && Input.GetMouseButtonDown(0)) {
            _dragOrigin = dragDistance;
            _dragging = true;
        }

        // update preview
        if(_dragging) {
            arrow.transform.localScale = arrow.Scale * Mathf.Exp(dragScaleCoefficient * dragAmount);
            arrow.Shift = dragAmount;
        }

        // shoot
        if (_dragging && !Input.GetMouseButton(0)) {
            var obj = Instantiate(impact);
            obj.transform.position = transform.position;

            if(GameManager.instance) {
                foreach (var ball in GameManager.instance.balls)
                    ball.Shoot(targetDirection, dragAmount);

            } else {
                Shoot(targetDirection, dragAmount);
            }

            AudioManager.instance.BonkBalls();
        }
    }
}
