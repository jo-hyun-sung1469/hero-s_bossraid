using UnityEngine;

public struct Boss1
{
    public const float MAXHP = 10000f;
    public const float HEAVYATTACK = 15f;
    public const float AOEATTACK = 25f;
}

public class EnemyController : MonoBehaviour, IDamageable
{
    
    private float hp = Boss1.MAXHP;
    public float Hp
    {
        get => hp;
        set => hp = value;
    }
    
    public void TakeDamage(float damage)
    {
        Hp -= damage;
        Debug.Log(Hp);
    }

    public float HeavyAttack()
    {
        return Boss1.HEAVYATTACK;
    }

    public float AoEAttack()
    {
        return Boss1.AOEATTACK;
    }
}
