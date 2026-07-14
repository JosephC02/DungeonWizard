using UnityEngine;
using UnityEngine.UI;


public enum damageType 
{ 
    None,
    Fire,
    Ice,
    Lightning
}

public enum spellType 
{
    None,
    Ball,
    Cone,
    Vortex
}

struct SpellComponent
{
    public damageType damageType;
    public spellType spellType;
    public float damage;
    public float manaCost;
}