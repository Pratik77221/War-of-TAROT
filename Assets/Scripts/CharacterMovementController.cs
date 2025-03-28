using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the selected character and handles movement and attacks.
/// </summary>
public class CharacterMovementController : MonoBehaviour
{
    public enum AttackType { None, HookPunch, HeavyPunch, MagicAttack }
    public AttackType currentAttack = AttackType.None;

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
    public Button magicAttackButton;

    // Cooldown timers 
    private float hookPunchCooldownTimer = 0f;
    private float heavyPunchCooldownTimer = 0f;
    private float magicAttackCooldownTimer = 0f;

    // Cooldown durations
    [Tooltip("Minimal cooldown for hook punch. Set to 0 for immediate re-trigger.")]
    public float hookPunchCooldownDuration = 0f;
    [Tooltip("Cooldown for heavy punch.")]
    public float heavyPunchCooldownDuration = 5f;
    [Tooltip("Cooldown for magic attack.")]
    public float magicAttackCooldownDuration = 3f;

    [Header("Magic Attack Projectile Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        characterAnimator = GetComponent<Animator>();


        hookPunchButton.onClick.AddListener(PerformHookPunch);
        heavyPunchButton.onClick.AddListener(PerformHeavyPunch);
        magicAttackButton.onClick.AddListener(PerformMagicAttack);
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
            magicAttackButton.interactable = true;
        }
        else
        {
            hookPunchButton.interactable = false;
            heavyPunchButton.interactable = false;
            magicAttackButton.interactable = false;
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
        if (magicAttackCooldownTimer > 0f)
            magicAttackCooldownTimer -= Time.deltaTime;
    }

    void HandleMovementInput()
    {

        currentMoveInput = moveAction.action.ReadValue<Vector2>().y;
    }

    void HandleRotationInput()
    {

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


        AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);


        if ((stateInfo.IsName("HookPunch") || stateInfo.IsName("HeavyPunch") || stateInfo.IsName("MagicAttack")) && stateInfo.normalizedTime < 1f)
            return;



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
    // ATTACK ANIMATION LOGIC 
    // --------------------------------------------------------

    public void PerformHookPunch()
    {
        if (currentSelectedCharacter != null && hookPunchCooldownTimer <= 0f)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                currentAttack = AttackType.HookPunch;
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
                currentAttack = AttackType.HeavyPunch;
                characterAnimator.Play("HeavyPunch", 0, 0f);
                heavyPunchCooldownTimer = heavyPunchCooldownDuration;
            }
        }
    }

    public void PerformMagicAttack()
    {
        if (currentSelectedCharacter != null && magicAttackCooldownTimer <= 0f)
        {
            PhotonView pv = currentSelectedCharacter.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                currentAttack = AttackType.MagicAttack;

                // Play the magic attack animation.
                characterAnimator.Play("MagicAttack", 0, 0f);

                magicAttackCooldownTimer = magicAttackCooldownDuration;

                // Always retrieve the spawn point from the current selected character.
                Transform spawnPoint = currentSelectedCharacter.transform.Find("FireballSpawnPoint");
                if (spawnPoint != null && fireballPrefab != null)
                {
                    // Spawn the fireball on the network.
                    PhotonNetwork.Instantiate(fireballPrefab.name, spawnPoint.position, spawnPoint.rotation);
                }
                else
                {
                    Debug.LogWarning("Fireball spawn point or fireball prefab not found on the current selected character.");
                }
            }
        }
    }

}
