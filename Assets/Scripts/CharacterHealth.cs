using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CharacterHealth : MonoBehaviourPun
{
    [Header("Health Settings")]
    [Range(0f, 1f)]
    public float currentHealth = 1f;

    public Slider healthBar;

    void Start()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

   
    [PunRPC]
    public void RPC_ApplyDamage(float damagePercent)
    {
        currentHealth -= damagePercent;
        currentHealth = Mathf.Clamp(currentHealth, 0f, 1f);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

   
    public void ApplyDamage(float damagePercent)
    {
        photonView.RPC("RPC_ApplyDamage", RpcTarget.All, damagePercent);
    }
}
