using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public string _name = "Player";
    [HideInInspector]
    public bool _isAlive = true;
    public float _groundAcceleration = 50f;
    public float _airAcceleration = 25f;
    public Vector2 _maxVelocity = new Vector2(5f, 8f);
    public Rigidbody2D _bomb = null;
    public float _throwForce = 300f;
    public float _throwCooldown = 0.1f;
    private float _currentThrowCooldownTime = 0f;
    public float _jumpForce = 500f;
    public LayerMask jumpMask;
    public string _horizontalAxis = "Horizontal";
    public string _verticalAxis = "Vertical";
    public string _fireButton = "Fire1";
    public string _jumpButton = "Jump";
    public static int _initialLives = 3;
    public int _lives = 3;
    private bool isRespawning = false;
    public float _maxRespawnTime = 5f;
    private float _currentRespawnTime = 0f;
    public Vector2 _bombBounceForce = new Vector2(200, 200);
    public float _playerBounceForce = 200;
    public float _bombThrowJumpForce = 350;

    private Rigidbody2D _rb2D;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Vector3 initialPosition = Vector3.zero;

    private void Awake() {
        _initialLives = _lives;
        _rb2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        initialPosition = transform.position;
    }

    void FixedUpdate() {
        if (Input.GetButton(_jumpButton)) {
            Jump();
        }
        if (isRespawning) {
            InitiateSpawn();
            return;
        }
        _currentThrowCooldownTime -= Time.deltaTime;
        Move();
        if (Input.GetButtonDown(_fireButton) && _currentThrowCooldownTime < 0) {
            ThrowBomb();
        }
        FlipSprite();
        ClampVelocity();
        PreventWallCollision();
    }

    private void Move() {
        if (IsOnGround()) {
            _rb2D.AddForce(Vector2.right * _groundAcceleration * Input.GetAxis(_horizontalAxis) * Time.deltaTime);
        }
        else {
            _rb2D.AddForce(Vector2.right * _airAcceleration * Input.GetAxis(_horizontalAxis) * Time.deltaTime);
        }
    }

    private void ClampVelocity() {
        float velX = Mathf.Clamp(_rb2D.velocity.x, -_maxVelocity.x, _maxVelocity.x);
        float velY = Mathf.Clamp(_rb2D.velocity.y, -_maxVelocity.y, _maxVelocity.y);
        _rb2D.velocity = new Vector2(velX, velY);
    }

    private void Jump() {
         if (IsOnGround() || isRespawning) {
            Activate();
            _rb2D.AddForce(Vector3.up * _jumpForce);           
        }        
    }

    private void FlipSprite() {
        if (Input.GetAxis(_horizontalAxis) < 0) {
            _spriteRenderer.flipX = true;
        }
        else if (Input.GetAxis(_horizontalAxis) > 0) {
            _spriteRenderer.flipX = false;
        }
    }

    private bool IsOnGround() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 0.51f, jumpMask);
        return hit.collider != null;
    }

    private void ThrowBomb() {
        Vector3 direction = (Vector3.right * Input.GetAxis(_horizontalAxis) - Vector3.up * Input.GetAxis(_verticalAxis)).normalized;
        bool moveUp = false;
        if (direction.y < -0.5 && IsOnGround()) {
            _rb2D.AddForce(Vector3.up * _bombThrowJumpForce);
            moveUp = true;
        }
        transform.position -= direction * (1f - HowCloseIsWall(direction));
        Vector3 spawnPosition = transform.position + direction * 0.9f;
        if (spawnPosition.y < transform.position.y && IsOnGround()) {
            spawnPosition = new Vector3(spawnPosition.x, transform.position.y, spawnPosition.z);
        }
        Rigidbody2D thrownBomb = Instantiate(_bomb, spawnPosition, Quaternion.identity);
        if (moveUp) {
            transform.position += Vector3.up * 0.5f;
        }
        thrownBomb.AddForce(direction * _throwForce);
        _currentThrowCooldownTime = _throwCooldown;
    }

    private void PreventWallCollision() {
        Vector3 horizontalMove = _rb2D.velocity;
        horizontalMove.y = 0;
        float distance = horizontalMove.magnitude * 1 * Time.deltaTime;
        horizontalMove.Normalize();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, horizontalMove, distance, jumpMask);
        if (hit.collider != null) {
            _rb2D.velocity = new Vector3(0, _rb2D.velocity.y, 0);
        }
    }

    private float HowCloseIsWall(Vector3 direction) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1, jumpMask);
        if (hit.collider != null) {
            if (hit.collider.CompareTag("Breakable")) {
                return hit.distance;
            }
        }
        return 1;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Explosion")) {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Bomb")) {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            collision.rigidbody.AddForce(direction * _bombBounceForce.x);
            collision.rigidbody.AddForce(Vector3.up * _bombBounceForce.y);
        }
        else if (collision.gameObject.CompareTag("Player")) {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            collision.rigidbody.AddForce(direction * _playerBounceForce);
        }
    }

    private void Die() {
        Respawn();
    }

    private void InitiateSpawn() {
        _currentRespawnTime -= Time.deltaTime;
        if (_currentRespawnTime <= 0) {
            Activate();
        }
    }

    private void Activate() {
        isRespawning = false;
        _rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        _collider.enabled = true;
    }

    private void Respawn() {
        if (_lives <= 0) {
            Destroy();
        }
        isRespawning = true;
        transform.position = initialPosition;
        _rb2D.velocity = Vector3.zero;
        _rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        _collider.enabled = false;
        _lives--;
        _currentRespawnTime = _maxRespawnTime;
    }

    private void Destroy() {
        _isAlive = false;
        gameObject.SetActive(false);
    }
}
