using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThirdPersonController : MonoBehaviour
{
    private PlayerController playerController;
    public Image HealthBar;
    private InputAction move, look;
    [SerializeField] GameObject canvas;

    private Animator animator;
    private Rigidbody rb;

    [SerializeField]
    private float movementForce = 1f;

    [SerializeField]
    private float jumpForce = 10f;
    public bool jump = false;

    [SerializeField]
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    public bool paused = false;

    private GameObject currentPotion;
    private bool potion = false, cure = false;
    public float currentHealth = 10;
    private float maxHealth = 10;

    public float rotationPower = 3f, rotationLerp = 0.5f;
    public GameObject followTarget;

    [SerializeField]
    private Camera playerCamera;

    private void Awake()
    {
        Cursor.visible = false;
        animator = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();
        playerController = new PlayerController();
    }

    private void OnEnable()
    {
        playerController.Player.Jump.started += DoJump;
        playerController.Player.Interact.started += Interact;
        playerController.Player.Pause.started += PauseGame;
        look = playerController.Player.Look;
        move = playerController.Player.Move;
        playerController.Player.Enable();
    }

    private void OnDisable()
    {
        playerController.Player.Jump.started -= DoJump;
        playerController.Player.Interact.started -= Interact;
        playerController.Player.Pause.started -= PauseGame;
        playerController.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        HealthBar.fillAmount = currentHealth / maxHealth;
        currentHealth -= 1 * Time.deltaTime;
        if(currentHealth <= 0)
        {
            canvas.GetComponent<PauseMenu>().Lose();
        }

        followTarget.transform.position = transform.position;
        //Rotate the Follow Target transform based on the input
        followTarget.transform.rotation *= Quaternion.AngleAxis(look.ReadValue<Vector2>().x * rotationPower, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(-look.ReadValue<Vector2>().y * rotationPower, Vector3.right);

        var angles = followTarget.transform.localEulerAngles;
        angles.z = 0;

        var angle = followTarget.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 350)
        {
            angles.x = 350;
        }
        else if (angle < 180 && angle > 40)
        {
            angles.x = 40;
        }

        followTarget.transform.localEulerAngles = angles;

        //Set the player rotation based on the look transform
        //transform.rotation = Quaternion.Euler(0, followTransform.transform.rotation.eulerAngles.y, 0);
        //reset the y rotation of the look transform
        //followTransform.transform.localEulerAngles = new Vector3(angles.x, 0, 0);

        if (!IsGrounded())
        {
            jump = true;
        }
        else
        {
            jump = false;
        }
        animator.SetFloat("Speed", rb.velocity.magnitude / maxSpeed);
        animator.SetBool("Jump", jump);
    }

    private void FixedUpdate()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Interact"))
        {
            forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
            forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

            rb.AddForce(forceDirection, ForceMode.Impulse);
            forceDirection = Vector3.zero;

            if (rb.velocity.y < 0f)
            {
                rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
            }

            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0;
            if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
            }

            LookAt();
        }
    }
    
    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            SFX.instance.audio.PlayOneShot(SFX.instance.jump);
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.4f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        if (potion == true)
        {
            animator.SetTrigger("Interact");
            currentHealth = 10;
            SFX.instance.audio.PlayOneShot(SFX.instance.interact);
            potion = false;
            Destroy(currentPotion);
        }

        if (cure == true)
        {
            canvas.GetComponent<PauseMenu>().Win();
        }
    }

    private void PauseGame(InputAction.CallbackContext obj)
    {
        if (paused)
        {
            canvas.GetComponent<PauseMenu>().Resume();
        }
        else
        {
            canvas.GetComponent<PauseMenu>().Pause();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Potion")
        {
            currentPotion = collision.gameObject;
            potion = true;
        }
        if (collision.gameObject.tag == "Cure")
        {
            cure = true;
        }
        if (collision.gameObject.tag != "Potion")
        {
            potion = false;
        }

        if (collision.gameObject.tag != "Cure")
        {
            cure = false;
        }
    }
}