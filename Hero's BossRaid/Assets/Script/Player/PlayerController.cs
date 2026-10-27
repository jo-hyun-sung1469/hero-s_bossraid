using UnityEngine;

public struct PlayerStat
{
    public const float MaxHealth = 100f;
    public const float Damage = 20f;
}

public enum Weapon
{
    None = 0,
    IronSword = 50,
    GoldSword = 100,
    RubySword = 150,
    DiamondSword = 200,
}

//플레이어의 행동를 관리하는 컨트롤러
public class PlayerController : MonoBehaviour, IAttackable, IDamageable
{
    [SerializeField] private Weapon usingWeapon = Weapon.None;
    [SerializeField] private SwordSwingAnimation swordSwing;
    private float hp = PlayerStat.MaxHealth;
    public float Hp { get; set; }
    
    public void ChangeWeapon(string weaponName)//연결 필요
    {
        switch (weaponName)
        {
            case "None":
            case "NormalSword":
                usingWeapon = Weapon.None; break;
            case "IronSword":
                usingWeapon = Weapon.IronSword; break;
            case "GoldSword":
                usingWeapon = Weapon.GoldSword; break;
            case "RubySword":
                usingWeapon = Weapon.RubySword; break;
            case "DiamondSword":
                usingWeapon = Weapon.DiamondSword; break;
        }
    }
    
    public void TakeDamage(float damage)
    {
        Hp -= damage;
    }

    public float Attack()
    {
        return Attack(Vector2.zero);
    }

    public float Attack(Vector2 direction)
    {
        if (swordSwing != null)
            swordSwing.Play(usingWeapon, direction);

        // 일반 검의 추가 피해량은 0이므로 기본 피해량만 사용
        return PlayerStat.Damage + (float)usingWeapon;
    }

    public void GameEnd()
    {
        Hp = PlayerStat.MaxHealth;
    }
}
