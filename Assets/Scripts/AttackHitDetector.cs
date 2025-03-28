using UnityEngine;
using Photon.Pun;

public class AttackHitDetector : MonoBehaviour
{
    [Header("Damage Settings")]
    public float hookPunchDamage = 0.1f;  // Hook punch damage
    public float heavyPunchDamage = 0.3f; // Heavy punch damage
    public float magicAttackDamage = 0.5f; // Magic attack damage

    void OnTriggerEnter(Collider other)
    {
        CharacterHealth healthManager = other.GetComponent<CharacterHealth>();
        if (healthManager != null)
        {
            PhotonView targetPhotonView = other.GetComponent<PhotonView>();
            PhotonView attackerPhotonView = GetComponentInParent<PhotonView>();

            if (targetPhotonView != null && attackerPhotonView != null)
            {
                // Prevent self damage
                if (targetPhotonView.Owner == attackerPhotonView.Owner)
                {
                    return;
                }
            }

            // Instead of checking the animator, get the current attack type from CharacterMovementController
            CharacterMovementController movementController = GetComponentInParent<CharacterMovementController>();
            float appliedDamage = hookPunchDamage; // default damage

            if (movementController != null)
            {
                switch (movementController.currentAttack)
                {
                    case CharacterMovementController.AttackType.HookPunch:
                        appliedDamage = hookPunchDamage;
                        break;
                    case CharacterMovementController.AttackType.HeavyPunch:
                        appliedDamage = heavyPunchDamage;
                        break;
                    case CharacterMovementController.AttackType.MagicAttack:
                        appliedDamage = magicAttackDamage;
                        break;
                    default:
                        appliedDamage = hookPunchDamage;
                        break;
                }
                // Reset attack state after applying damage so damage is only applied once per attack
                movementController.currentAttack = CharacterMovementController.AttackType.None;
            }

            healthManager.ApplyDamage(appliedDamage);
        }
    }
}
