using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //player properties
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;


    // player state
    public bool isJumping;

    // action booleans/flags
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;


    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection.x = _input.x;
        _moveDirection.x *= walkSpeed;

        if (_moveDirection.x < 0) {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        } else if (_moveDirection.x > 0){
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // this if contains actions while on the ground
        if (_characterController.below)
        {
            _moveDirection.y = 0f;
            isJumping = false;
            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
            }
        }
        // this else contains actions while in the air
        else
        {
            if (_releaseJump)
            {
                _releaseJump = false;

                if (_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }
            GravityCalculations();
        }
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    void GravityCalculations()
    {
        if(_moveDirection.y>0 && _characterController.above){
            _moveDirection.y = 0; //Remove or change if you want character to float up against ceiling e.d.
        }
        _moveDirection.y -= gravity * Time.deltaTime;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
            _releaseJump = false;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
        }
    }
}
