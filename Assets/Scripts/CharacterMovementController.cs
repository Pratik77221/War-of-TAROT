using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterMovementController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference rotateAction;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    private GameObject currentSelectedCharacter;
    private float currentMoveInput;
    private float currentRotationInput;
    private float currentSpeed;
    private Animator characterAnimator;

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    public Button hookPunchButton;
    public Button heavyPunchButton;

    private CharacterHealth characterHealth;
    private bool isAttacking;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        characterAnimator = GetComponent<Animator>();
        characterHealth = GetComponent<CharacterHealth>();

        // Remove button interactivity logic from here. 
        // We'll set button interactivity in HandleCharacterSelection() instead.

        // Add button listeners (buttons exist in the scene UI)
        hookPunchButton.onClick.AddListener(PerformHookPunch);
        heavyPunchButton.onClick.AddListener(PerformHeavyPunch);
    }

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
        // Check if the tapped character is owned by this local player
        PhotonView pv = selectedCharacter.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            currentSelectedCharacter = selectedCharacter;
            characterAnimator = currentSelectedCharacter.GetComponent<Animator>();
            characterHealth = currentSelectedCharacter.GetComponent<CharacterHealth>();

            // Make sure we have a valid Animator
            if (characterAnimator == null)
            {
                Debug.LogError("No Animator component found on the selected character!");
            }

            // Enable the attack buttons for our local player
            hookPunchButton.interactable = true;
            heavyPunchButton.interactable = true;
        }
        else
        {
            // If we tapped a character that isn't ours, disable the attack buttons
            hookPunchButton.interactable = false;
            heavyPunchButton.interactable = false;
        }
    }

    void Update()
    {
        // Move/Animate only if we have a selected character that belongs to us
        if (currentSelectedCharacter != null &&
            currentSelectedCharacter.GetComponent<PhotonView>().IsMine)
        {
            HandleMovementInput();
            HandleRotationInput();
            ApplyMovement();
            UpdateAnimation();
        }
    }

    void HandleMovementInput()
    {
        // Only need the vertical axis (y in the Vector2) for forward/back movement
        currentMoveInput = moveAction.action.ReadValue<Vector2>().y;
    }

    void HandleRotationInput()
    {
        // Only need the horizontal axis (x in the Vector2) for rotation
        currentRotationInput = rotateAction.action.ReadValue<Vector2>().x;
    }

    void ApplyMovement()
    {
        if (currentSelectedCharacter == null) return;

        // Instantly set the current speed (no acceleration/deceleration)
        currentSpeed = currentMoveInput * moveSpeed;

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

    void UpdateAnimation()
    {
        if (characterAnimator == null) return;

        // If we are attacking or the punch animation is still playing, don't override it
        if (isAttacking || IsPlaying("HookPunch") || IsPlaying("HeavyPunch"))
            return;

        bool isMoving = Mathf.Abs(currentSpeed) > 0f;

        if (isMoving && !IsPlaying("Run"))
        {
            characterAnimator.Play("Run");
        }
        else if (!isMoving && !IsPlaying("Idle"))
        {
            characterAnimator.Play("Idle");
        }
    }


    bool IsPlaying(string stateName)
    {
        if (characterAnimator == null) return false;
        AnimatorStateInfo currentState = characterAnimator.GetCurrentAnimatorStateInfo(0);
        return currentState.IsName(stateName);
    }

    // --------------------------------------------------------
    // ATTACK LOGIC (Local-only animation play)
    // --------------------------------------------------------

    public void PerformHookPunch()
    {
        // Only attack if not already attacking, and we own this character
        if (!isAttacking && currentSelectedCharacter != null)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                StartAttack("HookPunch", characterHealth.hookPunchDamage);
            }
        }
    }

    public void PerformHeavyPunch()
    {
        // Only attack if not already attacking, and we own this character
        if (!isAttacking && currentSelectedCharacter != null)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                StartAttack("HeavyPunch", characterHealth.heavyPunchDamage);
            }
        }
    }

    void StartAttack(string animationName, float damage)
    {
        isAttacking = true;

        // Play the punch animation locally (no RPC)
        if (characterAnimator != null)
        {
            characterAnimator.Play(animationName);
        }

        // If using a separate script for hit detection (e.g., AttackHitDetector)
        // pass the damage value so hits can be processed
        GetComponent<AttackHitDetector>()?.SetCurrentDamage(damage);

        // Reset attack after cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}
