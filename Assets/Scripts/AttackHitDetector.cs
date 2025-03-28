using UnityEngine;
using Photon.Pun;

public class AttackHitDetector : MonoBehaviour
{
    [Header("Damage Settings")]
   
    public float hookdamageAmount;
    public float heavydamageAmount;

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

            if(hookdamageAmount! == 0.0f)
            {
                healthManager.ApplyDamage(hookdamageAmount);
            }

            else if (heavydamageAmount! == 0.0f)
            {
                healthManager.ApplyDamage(heavydamageAmount);
            }

            
        }
    }
}
