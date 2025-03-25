using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class CharacterMovementController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference rotateAction;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float acceleration = 2f;
    public float deceleration = 4f;

    private GameObject currentSelectedCharacter;
    private float currentMoveInput;
    private float currentRotationInput;
    private float currentSpeed;
    private Animator characterAnimator;

    void OnEnable()
    {
        ARGameManager.OnCharacterTapped += HandleCharacterSelection;
        moveAction.action.Enable();
        rotateAction.action.Enable();
    }

    void OnDisable()
    {
        ARGameManager.OnCharacterTapped -= HandleCharacterSelection;
        moveAction.action.Disable();
        rotateAction.action.Disable();
    }

    void HandleCharacterSelection(GameObject selectedCharacter)
    {
        if (selectedCharacter.GetComponent<PhotonView>().IsMine)
        {
            currentSelectedCharacter = selectedCharacter;
            characterAnimator = currentSelectedCharacter.GetComponent<Animator>();
            

            if (characterAnimator == null)
            {
                Debug.LogError("No Animator component found on character!");
            }
        }
    }

    void Update()
    {
        if (currentSelectedCharacter != null && currentSelectedCharacter.GetComponent<PhotonView>().IsMine)
        {
            HandleMovementInput();
            HandleRotationInput();
            ApplyMovement();
            UpdateAnimation();
        }
    }

    //void UpdateAnimation()
    //{
    //    if (characterAnimator != null)
    //    {
    //       // Use absolute value to handle both forward/backward movement
    //      bool isMoving = Mathf.Abs(currentSpeed) > 0.1f;
    //       characterAnimator.SetBool("IsWalking", isMoving);


    // characterAnimator.SetFloat("Speed", Mathf.Abs(currentSpeed));
    //     }
    // }

    void UpdateAnimation()
    {
        if (characterAnimator != null)
        {
            bool isMoving = Mathf.Abs(currentSpeed) > 0.1f;

            if (isMoving && !IsPlaying("Run"))
            {
                characterAnimator.Play("Run");
            }
            else if (!isMoving && !IsPlaying("Idle"))
            {
                characterAnimator.Play("Idle");
            }
        }
    }

    bool IsPlaying(string stateName)
    {
        AnimatorStateInfo currentState = characterAnimator.GetCurrentAnimatorStateInfo(0);
        return currentState.IsName(stateName);
    }

    void HandleMovementInput()
    {
        currentMoveInput = moveAction.action.ReadValue<Vector2>().y;
    }

    void HandleRotationInput()
    {
        currentRotationInput = rotateAction.action.ReadValue<Vector2>().x;
    }


    /*
  void ApplyMovement()
{
    if (currentSelectedCharacter == null) return;

    // Calculate movement direction
    Vector3 moveDirection = new Vector3(currentMoveInput.x, 0f, currentMoveInput.y);

    // Smooth acceleration/deceleration
    currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * moveSpeed, 
        (moveDirection.magnitude > 0 ? acceleration : deceleration) * Time.deltaTime);

    // Apply movement
    currentSelectedCharacter.transform.Translate(
        currentVelocity * Time.deltaTime, 
        Space.World);

    // Handle rotation
    if (currentRotationInput.magnitude > 0.1f)
    {
        Vector3 rotationDirection = new Vector3(currentRotationInput.x, 0f, currentRotationInput.y);
        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
        currentSelectedCharacter.transform.rotation = Quaternion.RotateTowards(
            currentSelectedCharacter.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}
*/
    void ApplyMovement()
    {
        if (currentSelectedCharacter == null) return;

        // Smooth acceleration/deceleration
        currentSpeed = Mathf.Lerp(
            currentSpeed,
            currentMoveInput * moveSpeed,
            (Mathf.Abs(currentMoveInput) > 0.1f ? acceleration : deceleration) * Time.deltaTime
        );

        // Move in facing direction
        currentSelectedCharacter.transform.Translate(
            Vector3.forward * currentSpeed * Time.deltaTime,
            Space.Self
        );

        // Rotate around Y-axis
        currentSelectedCharacter.transform.Rotate(
            Vector3.up,
            currentRotationInput * rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}
