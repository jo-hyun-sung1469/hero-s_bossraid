using UnityEngine;

using System.Collections.Generic;

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

    //무기 종류
    private readonly Dictionary<Weapon, bool> weaponsKind = new Dictionary<Weapon, bool>
    {
        { Weapon.None, true },
        { Weapon.IronSword, false },
        { Weapon.GoldSword, false },
        { Weapon.RubySword, false },
        { Weapon.DiamondSword, false }
    };
    
    private bool weaponsLoaded;
    
    //사용하고 있는 무기
    public Weapon UsingWeapon { get => usingWeapon; internal set => usingWeapon = value; }
    public SwordSwingAnimation SwordSwing => swordSwing;

    // 활성화시 무기 로딩 실행
    private void OnEnable()
    {
        LoadOwnedWeapons();
    }

    //무기 로딩 함수
    private void LoadOwnedWeapons()
    {
        // 무기를 소유하고 있는지 foreach로 확인후 저장
        foreach (Weapon weapon in System.Enum.GetValues(typeof(Weapon)))
            weaponsKind[weapon] = weapon == Weapon.None || PlayerPrefs.GetInt($"HeroBossRaid.Player.Weapon.{weapon}", 0) == 1;
        //로딩 완료 처리
        weaponsLoaded = true;
    }

    public bool HasWeapon(Weapon weapon)
    {
        //로딩이 되어 있지 않다면 실행
        if (!weaponsLoaded)
            LoadOwnedWeapons();
        return weaponsKind.TryGetValue(weapon, out bool owned) && owned;
    }

    public bool HasWeapon(string weaponName)
    {
        return TryGetWeapon(weaponName, out Weapon weapon) && HasWeapon(weapon);
    }

    public void AddWeapon(Weapon weapon)
    {
        if (!weaponsLoaded)
            LoadOwnedWeapons();
        //무기 종류에 포함되지 않는다면 
        if (!weaponsKind.ContainsKey(weapon))
            return;
        if (weapon == Weapon.None)
            return;
        weaponsKind[weapon] = true;
        PlayerPrefs.SetInt($"HeroBossRaid.Player.Weapon.{weapon}", 1);
        PlayerPrefs.Save();
    }

    //무기가 존재하는지 확인하는 함수
    public static bool TryGetWeapon(string weaponName, out Weapon weapon)
    {
        weapon = Weapon.None;
        switch (weaponName)
        {
            case "None" : return true;
            case "Iron":
                weapon = Weapon.IronSword; return true;
            case "Gold":
                weapon = Weapon.GoldSword; return true;
            case "Ruby":
                weapon = Weapon.RubySword; return true;
            case "Diamond":
                weapon = Weapon.DiamondSword; return true;
            default:
                return false;
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
