using UnityEngine;
using Photon.Pun;

public class AttackHitDetector : MonoBehaviour
{
    [Header("Damage Settings")]
    public float hookPunchDamage = 0.1f;  
    public float heavyPunchDamage = 0.3f; 
    public float magicAttackDamage = 0.5f;


    /*
    void OnTriggerEnter(Collider other)
    {
       
        CharacterHealth healthManager = other.GetComponent<CharacterHealth>();
        if (healthManager != null)
        {
           
            healthManager.ApplyDamage(hookPunchDamage);
        }
    }
    */

    void OnTriggerEnter(Collider other)
    {
        CharacterHealth healthManager = other.GetComponent<CharacterHealth>();
        if (healthManager != null)
        {
            PhotonView targetPhotonView = other.GetComponent<PhotonView>();
            PhotonView attackerPhotonView = GetComponentInParent<PhotonView>();

            if (targetPhotonView != null && attackerPhotonView != null)
            {
               
                if (targetPhotonView.Owner == attackerPhotonView.Owner)
                {
                    return;
                }
            }

            
            CharacterMovementController movementController = GetComponentInParent<CharacterMovementController>();
            float appliedDamage = hookPunchDamage; 

            
               
                //appliedDamage = hookPunchDamage;
               

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
               
                movementController.currentAttack = CharacterMovementController.AttackType.None;
            }

            healthManager.ApplyDamage(appliedDamage);
        }
    }
}
