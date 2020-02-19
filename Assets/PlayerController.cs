using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    [SerializeField] private float MaxRunSpeed = 10.0f;

    [SerializeField] private float SecondsToMaxSpeed = 0.5f;
    [SerializeField] private float SecondsToNoSpeed = 0.5f;

    [SerializeField] private float JumpForce = 500.0f;
    [SerializeField] private float AirControlForce = 50.0f;

    [SerializeField] private KeyCode RightKey = KeyCode.D;
    [SerializeField] private KeyCode LeftKey = KeyCode.A;
    [SerializeField] private KeyCode JumpKey = KeyCode.Space;

    [SerializeField] private Rigidbody RB = null;

    private bool OnGround = false;

    void Start()
    {
        
    }

    void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if(OnGround)
        {
            if (Input.GetKey(RightKey))
                Run(Direction.RIGHT);
            if(Input.GetKey(LeftKey))
                Run(Direction.LEFT);
            if (!Input.GetKey(RightKey)
                && !Input.GetKey(LeftKey))
                Decelerate();
            if (Input.GetKeyDown(JumpKey))
                Jump();
        }
        if (Input.GetKey(RightKey))
            AirControls(Direction.RIGHT);
        if (Input.GetKey(LeftKey))
            AirControls(Direction.LEFT);
    }

    private void OnCollisionEnter(Collision pCollision)
    {
        OnGround = true;
    }

    private void OnCollisionExit(Collision pCollision)
    {
        OnGround = false;
    }

    private void Run(Direction pDirection)
    {
        if (RB.velocity.magnitude >= MaxRunSpeed)
            return;
        if (pDirection == Direction.RIGHT)
        {
            RB.velocity += (Vector3.right * (MaxRunSpeed / SecondsToMaxSpeed) * Time.deltaTime);
        }
        else if (pDirection == Direction.LEFT)
        {
            RB.velocity += (Vector3.left * (MaxRunSpeed / SecondsToMaxSpeed) * Time.deltaTime);
        }
    }

    private void AirControls(Direction pDirection)
    {
        if (pDirection == Direction.LEFT
            && (Vector2.Dot(RB.velocity, Vector2.left) < MaxRunSpeed))
        {
            RB.AddForce(Vector2.left * AirControlForce * Time.deltaTime);
        }
        else if (pDirection == Direction.RIGHT
            && (Vector2.Dot(RB.velocity, Vector2.right) < MaxRunSpeed))
        {
            RB.AddForce(Vector2.right * AirControlForce * Time.deltaTime);
        }
    }
    private void Decelerate()
    {
        float velocityMagnitude = RB.velocity.magnitude;
        velocityMagnitude -= (MaxRunSpeed / SecondsToNoSpeed) * Time.deltaTime;
        if (velocityMagnitude < 0)
        {
            velocityMagnitude = 0;
        }
        RB.velocity = RB.velocity.normalized * velocityMagnitude;
    }
    private void Jump()
    {
        RB.AddForce(Vector3.up * JumpForce);
    }
}
