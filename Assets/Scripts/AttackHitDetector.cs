using UnityEngine;
using Photon.Pun;

public class AttackHitDetector : MonoBehaviour
{
    [Header("Damage Settings")]
   
    public float damageAmount = 0.1f;

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

            healthManager.ApplyDamage(damageAmount);
        }
    }
}
