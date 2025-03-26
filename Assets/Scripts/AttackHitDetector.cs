using UnityEngine;
using Photon.Pun;

public class AttackHitDetector : MonoBehaviourPun
{
    private float currentDamage;
    private bool detectionActive;

    public void SetCurrentDamage(float damage)
    {
        currentDamage = damage;
    }

    // Call this from animation events
    public void ActivateHitDetection()
    {
        if (!photonView.IsMine) return;
        detectionActive = true;
        Invoke(nameof(DeactivateHitDetection), 0.5f); // Adjust based on attack animation length
    }

    void DeactivateHitDetection()
    {
        detectionActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!detectionActive || !photonView.IsMine) return;

        CharacterHealth targetHealth = other.GetComponent<CharacterHealth>();
        if (targetHealth != null && targetHealth.photonView != photonView)
        {
            targetHealth.photonView.RPC("TakeDamage", RpcTarget.All, currentDamage);
        }
    }
}