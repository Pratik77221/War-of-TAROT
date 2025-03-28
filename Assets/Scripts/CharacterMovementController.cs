using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Controll the selected character and also attack using the same 
/// </summary>
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
    public Button hookPunchButton;
    public Button heavyPunchButton;

    public Button specialAttackButton;

    public Transform specialAttackSpawnPoint;

    public GameObject Fireball;

    // Cooldown timers (in seconds)
    private float hookPunchCooldownTimer = 0f;
    private float heavyPunchCooldownTimer = 0f;

    private float specialAttackCooldownTimer = 0f;

    // Cooldown durations
    [Tooltip("Minimal cooldown for hook punch. Set to 0 for immediate re-trigger.")]
    public float hookPunchCooldownDuration = 0f;
    [Tooltip("Cooldown for heavy punch.")]
    public float heavyPunchCooldownDuration = 5f;

    [Tooltip("Cooldown for Special Attack.")]
    public float specialAttackCooldownDuration = 15f;

    [Header("Damage Settings")]
    public float hookPunchDamage = 0.1f;
    public float heavyPunchDamage = 0.3f;

    public float specialAttackDamage = 1.0f;

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        characterAnimator = GetComponent<Animator>();

        // Add button listeners
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
        PhotonView pv = selectedCharacter.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            currentSelectedCharacter = selectedCharacter;
            characterAnimator = currentSelectedCharacter.GetComponent<Animator>();

            // Enable attack buttons for our local player
            hookPunchButton.interactable = true;
            heavyPunchButton.interactable = true;
        }
        else
        {
            hookPunchButton.interactable = false;
            heavyPunchButton.interactable = false;
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

        // Update cooldown timers
        if (hookPunchCooldownTimer > 0f)
            hookPunchCooldownTimer -= Time.deltaTime;
        if (heavyPunchCooldownTimer > 0f)
            heavyPunchCooldownTimer -= Time.deltaTime;
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
        if (currentSelectedCharacter == null)
            return;

        currentSpeed = currentMoveInput * moveSpeed;
        currentSelectedCharacter.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);
        currentSelectedCharacter.transform.Rotate(Vector3.up, currentRotationInput * rotationSpeed * Time.deltaTime, Space.Self);
    }

    void UpdateAnimation()
    {
        if (characterAnimator == null)
            return;

        // Retrieve current animation state info from layer 0
        AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);

        // If an attack animation is playing and hasn't finished, do nothing
        if ((stateInfo.IsName("HookPunch") || stateInfo.IsName("HeavyPunch")) && stateInfo.normalizedTime < 1f)
            return;

        // Otherwise, switch to run or idle based on movement
        bool isMoving = Mathf.Abs(currentSpeed) > 0f;
        if (isMoving && !stateInfo.IsName("Run"))
        {
            characterAnimator.Play("Run");
        }
        else if (!isMoving && !stateInfo.IsName("Idle"))
        {
            characterAnimator.Play("Idle");
        }
    }

    // --------------------------------------------------------
    // ATTACK ANIMATION LOGIC (No damage logic)
    // --------------------------------------------------------

    public void PerformHookPunch()
    {
      if (currentSelectedCharacter != null && hookPunchCooldownTimer <= 0f)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                // Restart the "HookPunch" animation from the beginning regardless of the current state
                characterAnimator.Play("HookPunch", 0, 0f);
                hookPunchCooldownTimer = hookPunchCooldownDuration;
            }
        }
    }

    public void PerformHeavyPunch()
    {
        if (currentSelectedCharacter != null && heavyPunchCooldownTimer <= 0f)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                // Restart the "HeavyPunch" animation from the beginning regardless of the current state
                characterAnimator.Play("HeavyPunch", 0, 0f);
                heavyPunchCooldownTimer = heavyPunchCooldownDuration;
            }
        }
    }

    public void PerformSpecialAttack()
    {
        if (currentSelectedCharacter != null && specialAttackCooldownTimer <= 0f)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                // Restart the "HeavyPunch" animation from the beginning regardless of the current state
                characterAnimator.Play("MagicAttack", 0, 0f);

                specialAttackCooldownTimer= specialAttackCooldownDuration;
            }
        }
    }
}
