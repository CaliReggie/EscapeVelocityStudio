using UnityEngine;
using System.Collections.Generic;
public enum Equippables
{
    Grapple,
    Disk,
    TeleportDisk,
    StickyDisk,
    Melee,
    None,
}
public class PlayerStateManager : MonoBehaviour
{
    private static Equippables currentEquipable = Equippables.Grapple;
    private static Equippables attackingEquipable = Equippables.None;
    private static List<Equippables> equippables = new List<Equippables>();

    private void Awake()
    {
        equippables.Add(Equippables.Grapple);
        foreach (Weapon e in GetComponent<WeaponDefinitions>().weapons)
        {
            equippables.Add(e.equippableType);
        }
    }
    public static void GoToNextEquippable()
    {
        int currentIndex = equippables.IndexOf(currentEquipable);
        int nextIndex;
        if (currentIndex + 1 >= equippables.Count)
        {
            nextIndex = 0;
        }
        else
        {
            nextIndex = currentIndex + 1;
        }
        currentEquipable = equippables[nextIndex];
    }
    

    public static Equippables GetEquippable()
    {
        return currentEquipable;
    }

    public static void SetAttackingEquipable(Equippables equippable)
    {
        attackingEquipable = equippable;
        
        Debug.Log("Attacking Equipable: " + attackingEquipable);
    }

    public static Equippables GetAttackingEquipable()
    {
        return attackingEquipable;
    }
}
