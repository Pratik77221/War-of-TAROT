/*using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AttackController : MonoBehaviourPun
{
    [Header("UI References")]
    public Button hookPunchButton;
    public Button heavyPunchButton;

    [Header("Attack Settings")]
    public float hookDamage = 15f;
    public float heavyDamage = 25f;
    public float attackCooldown = 0.8f;

    [Header("Collider Setup")]
    public string leftHandName = "LeftHand";
    public string rightHandName = "RightHand";

    private Animator animator;
    private List<Collider> attackColliders = new List<Collider>();
    private bool isAttacking;
    private HashSet<int> damagedTargets = new HashSet<int>();
    private bool isSelected;

    
    private float currentAttackDamage;
    private string currentAttackType;

    void Start()
    {
        animator = GetComponent<Animator>();
        InitializeColliders();
        SetSelectionState(false); // Initialize as deselected
    }

    public void SetSelectionState(bool state)
    {
        isSelected = state;
        UpdateButtonStates();
    }

    void InitializeColliders()
    {
        Transform leftHand = FindChildRecursive(transform, leftHandName);
        Transform rightHand = FindChildRecursive(transform, rightHandName);

        if (leftHand != null && leftHand.TryGetComponent<Collider>(out var leftCol))
            attackColliders.Add(leftCol);

        if (rightHand != null && rightHand.TryGetComponent<Collider>(out var rightCol))
            attackColliders.Add(rightCol);

        SetColliders(false);
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name)) return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    void UpdateButtonStates()
    {
        if (photonView.IsMine)
        {
            hookPunchButton.interactable = isSelected;
            heavyPunchButton.interactable = isSelected;
        }
    }

    public void PerformHookPunch()
    {
        if (isSelected && !isAttacking)
            StartCoroutine(PerformAttack(hookDamage, "HookPunch"));
    }

    public void PerformHeavyPunch()
    {
        if (isSelected && !isAttacking)
            StartCoroutine(PerformAttack(heavyDamage, "HeavyPunch"));
    }

    IEnumerator PerformAttack(float damage, string animationName)
    {
        currentAttackDamage = damage;  // Store damage for this attack
        isAttacking = true;
        damagedTargets.Clear();

        // CRUCIAL: This animation call MUST stay in the code
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, animationName);
        SetColliders(true);

        yield return new WaitForSeconds(0.3f);
        SetColliders(false);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void SetColliders(bool state)
    {
        foreach (Collider col in attackColliders)
        {
            if (col != null) col.enabled = state;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || !isAttacking) return;

        PhotonView targetView = other.GetComponent<PhotonView>();
        if (targetView != null && targetView != photonView)
        {
            int targetID = targetView.ViewID;
            if (!damagedTargets.Contains(targetID))
            {
                damagedTargets.Add(targetID);
                targetView.RPC("TakeDamage", RpcTarget.All, currentAttackDamage);
            }
        }
    }

    [PunRPC]
    void RPC_PlayAnimation(string animationName)
    {
        animator.Play(animationName, 0, 0f);
    }
}*/