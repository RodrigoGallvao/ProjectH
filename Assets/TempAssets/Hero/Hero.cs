using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    [Header("Controls")]
        Vector2 input;

    [Header("Components")]
        public Camera mCamera;
        public Rigidbody rb;
        public Animator animtr;
        public float playerHeight;
        public Transform orient;
        public Transform player;
        public Transform playerOBJ;
        public float currentSpeed;

    [Header("Movement")]
        public float rotateSpeed = 7;
        public float moveSpeed = 5;
        public float groundDrag = 5;
        public float jumpForce = 7;
        public float jumpCooldown = .25f;
        public float airMultiplier = .4f;
        Vector3 moveForce;
        Vector3 smoothMoveForce;
        Vector3 smoothMoveVel;
        public bool canJump;

    [Header("GroundCheck")]
        public LayerMask groundMask;
        public bool grounded;

    void Start() {
        mCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animtr = GetComponentInChildren<Animator>();

        canJump = true;
    }

    void Update() {
        ControlInputs();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, groundMask);
        if(grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        //Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        //orient.forward = viewDir.normalized;
        orient.eulerAngles = new Vector3(0, mCamera.transform.eulerAngles.y, 0);

        Vector3 inputDir = orient.forward * input.y + orient.right * input.x;
        if (inputDir != Vector3.zero)
            playerOBJ.forward = Vector3.Slerp(playerOBJ.forward, inputDir.normalized, Time.deltaTime * rotateSpeed);

        SpeedControl();
        Animate();
    }

    void FixedUpdate() {
            Move();
    }

    private void ControlInputs() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        //animtr.SetBool("run",input.x + input.y != 0);
    }

    private void Move() {
        moveForce = orient.forward * input.y + orient.right * input.x;
        if (currentSpeed < moveSpeed)
            smoothMoveForce = Vector3.SmoothDamp(smoothMoveForce, moveForce, ref smoothMoveVel, .4f * input.magnitude + .05f);
        else
            smoothMoveForce = moveForce;

        if (grounded)
            rb.AddForce(smoothMoveForce * moveSpeed * 10f, ForceMode.Force);
        
        if(Input.GetKey("space") && canJump && grounded) {
            canJump = false;
            Jump();
            Invoke(nameof(JumpReset), jumpCooldown);
        }

        if (!grounded)
            rb.AddForce(moveForce * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        currentSpeed = flatVel.magnitude;
        if (currentSpeed > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpReset() {
        canJump = true;
    }

    public void Animate() {
        float speedRange = currentSpeed / moveSpeed;
        animtr.SetFloat("MoveSpeed", speedRange);

        animtr.SetBool("Grounded", grounded);
    }

}
