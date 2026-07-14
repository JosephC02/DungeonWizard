using System;
using UnityEngine;

public class Spell 
{
    public damageType DamageType;
    public spellType PrimaryType;
    public spellType SecondaryType;
    public float Damage;
    public float ManaCost;

    public Spell(spellType primaryType, spellType secondaryType, damageType damageType, float damage, float manaCost)
    {
        DamageType = damageType;
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
        Damage = damage;
        ManaCost = manaCost;

        var SpellObject = new GameObject();
        SpellObject.AddComponent<ParticleSystem>();

        switch (PrimaryType)
        {
            case spellType.Ball:
                SpellObject.AddComponent<SphereCollider>();
                SpellObject.GetComponent<MeshFilter>().mesh = new Mesh();
                break;
            case spellType.Vortex:
                SpellObject.AddComponent<CapsuleCollider>();
                break;
            case spellType.Cone:
                Debug.Log("Error, spell has no mesh");
                break;
            case spellType.None:
                Debug.Log("Error, spell has no mesh");
                break;
        }
    }

    public void CastSpell() 
    {
        Debug.Log(PrimaryType.ToString());
        Debug.Log(SecondaryType.ToString());
        Debug.Log(DamageType.ToString());
    }
}
