using GlobalTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layermask;

    // these booleans tell us if the player is in contact with other objects;
    public bool below;
    public GroundType groundType;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;


    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHit = new RaycastHit2D[3];

    private bool _disableGroundCheck;

    // make private aftes testing
    public Vector2 _slopeNormal;
    public float _slopeAngle;


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

    private void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody.position - new Vector2(0, _capsuleCollider.size.y * 0.5f);

        _raycastPosition[0] = raycastOrigin + (Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + (Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);

        //for debug only. remove when done.
        DrawDebugRays(Vector2.down, Color.green);

        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDistance, layermask);

            if (hit.collider)
            {
                _raycastHit[i] = hit;
                numberOfGroundHits++;
            }
        }

        if (numberOfGroundHits > 0)
        {
            if (_raycastHit[1].collider)
            {
                groundType = DetermineGroundType(_raycastHit[1].collider);
                _slopeNormal = _raycastHit[1].normal;
                _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            }
            else
            {
                for (int i = 0; i < _raycastHit.Length; i++)
                {
                    if (_raycastHit[i].collider)
                    {
                        // note that if left and right raycast both hit an object right raucast will be choosen to get GroundType from.
                        groundType = DetermineGroundType(_raycastHit[i].collider);
                        _slopeNormal = _raycastHit[i].normal;
                        _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);
                    }
                }
            }

            below = true;
        }
        else
        {
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
