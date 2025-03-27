using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test script for attack and colission test 
/// </summary>

public class CharacterAttack : MonoBehaviour
{
    private Animator animator;       
    private bool isAttacking = false;

    private void Start()
    {
        // Get the Animator component of Character 2
        animator = GetComponent<Animator>();
    }

    // Function to play the HookPunch animation when the button is pressed
    public void PlayHookPunch()
    {
        if (animator != null)
        {
            // Play the HookPunch animation directly by name
            animator.Play("HookPunch"); // Replace "HookPunch" with the name of your animation clip
            isAttacking = true;
        }
    }

    // OnTriggerEnter will be called when the collider of Character 2 collides with any trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the animation is playing and collided with another object
        if (isAttacking)
        {
            // Log a message every time the collision happens during the attack
            Debug.Log("Character 2 collided with another collider while performing HookPunch animation.");
        }
    }

    // Optional: You can reset isAttacking when the animation finishes (if necessary)
    private void OnAnimationFinish()
    {
        isAttacking = false;
    }
}
