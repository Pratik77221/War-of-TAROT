using UnityEngine;
using Photon.Pun;

public class AnimationSyncProxy : MonoBehaviourPun
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [PunRPC]
    public void SyncMovementAnimation(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }
}