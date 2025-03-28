using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class CharacterHealth : MonoBehaviourPun
{
    public float currentHealth = 1f;
    public Slider healthBar;
    private Animator animator;
    public float delayTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    [PunRPC]
    public void RPC_ApplyDamage(float damagePercent)
    {
        currentHealth -= damagePercent;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0f && animator != null)
        {
            animator.Play("Die");
            StartCoroutine(DestroyAfterDelayCoroutine());
        }
    }

    private IEnumerator DestroyAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(gameObject);
    }

    public void ApplyDamage(float damagePercent)
    {
        photonView.RPC("RPC_ApplyDamage", RpcTarget.All, damagePercent);
    }
}
