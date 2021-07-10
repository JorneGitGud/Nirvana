using GlobalTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layermask;
    public float slopeAngleLimit = 45f;


    // these booleans tell us if the player is in contact with other objects;
    public bool below;
    public GroundType groundType;
    public float downForceAdjustment = 1.2f;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;


    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHit = new RaycastHit2D[3];

    private bool _disableGroundCheck;

    // make private aftes testing
    private Vector2 _slopeNormal;
    private float _slopeAngle;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _lastPosition = _rigidbody.position;

        if (_slopeAngle != 0 && below == true)
        {
            if ((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                
                //cheeky fix, future Jorne please fix this in a proper manner.
                _moveAmount.y *= downForceAdjustment;
            }
        }

        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPosition);

        _moveAmount = Vector2.zero;

        if (!_disableGroundCheck)
        {
            CheckGrounded();
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGrounded() {
        RaycastHit2D hit = Physics2D.CapsuleCast(
            _capsuleCollider.bounds.center, 
            _capsuleCollider.size, 
            CapsuleDirection2D.Vertical, 
            0f,
            Vector2.down,
            raycastDistance,
            layermask);

        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
        }
        else {
            groundType = GroundType.None;
            below = false;
        }

    }

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.GroundType;
        }
        else
        {
            return GroundType.LevelGeometry;
        }
    }

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

    public void DisableGroundCheck()
    {
        below = false;
        _disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disableGroundCheck = false;
    }
}
