using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Pour �viter de spammer pendant l�animation
    private bool isAttacking = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PlayRandomAttack();
        }
    }

    void PlayRandomAttack()
    {
        isAttacking = true;

        int randomAttack = Random.Range(1, 4); // 1, 2 ou 3

        string triggerName = "Attack" + randomAttack;
        animator.SetTrigger(triggerName);
    }

    // Appelle cette m�thode avec un Event � la fin de chaque animation d�attaque dans l'Animator
    public void EndAttack()
    {
        isAttacking = false;
    }
}
