using System;
using System.Data;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public Transform orientaion;

    [Header("PlayerValues")]
    public float maxHealth;
    private float currentHealth;
    public float maxMana;
    private float currentMana;
    public Slider healthBar;
    public Slider manaBar;

    [Header("Keybinds")]
    public KeyCode primaryFire = KeyCode.Mouse0;
    public KeyCode secondaryFire = KeyCode.Mouse1;
    public KeyCode slot1 = KeyCode.Alpha1;
    public KeyCode slot2 = KeyCode.Alpha2;
    public KeyCode slot3 = KeyCode.Alpha3;
    public KeyCode slot4 = KeyCode.Alpha4;
    public KeyCode slot5 = KeyCode.Alpha5;
    public KeyCode slot6 = KeyCode.Alpha6;
    public KeyCode toggleCastMode = KeyCode.Q;

    [Header("Spellcasting")]
    private Array equippedComponents;
    private int selectedSlot;
    public float shieldCost;
    public float manaRegen;

    [Header("Projectiles")]
    public Rigidbody playerProjectile;

    [Header("Rules")]
    bool BLOCKING;
    bool NO_MANA;
    bool CAN_REGEN_MANA;
    bool PLAYER_DEAD;
    bool CASTING;
    bool CAST_MODE;
    bool PRIMARY_FIRE;
    bool SECONDARY_FIRE;
    bool TOGGLE_CAST_MODE;

    private enum playerState
    {
        Base,
        Blocking,
        Casting
    }

    private playerState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedSlot = 1;
        CAST_MODE = false;
        PLAYER_DEAD = false;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        currentHealth = maxHealth;

        manaBar.maxValue = maxMana;
        manaBar.value = maxMana;
        currentMana = maxMana;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        StateHandler()
        UpdateRules();
        UpdateUI();
    }

    private void GetInputs()
    {
        if (Input.GetKeyDown(primaryFire))
        {
            if (CAST_MODE) { Debug.Log("Cast mode lmb"); }
            else
            {
                var projectile = Instantiate(playerProjectile, orientaion.position, orientaion.rotation);
                projectile.linearVelocity = orientaion.transform.TransformDirection(new Vector3(0, 0, 10));
                Physics.IgnoreCollision(projectile.GetComponent<Collider>(), GetComponent<Collider>());
            }
        }

        if (Input.GetKeyDown(secondaryFire))
        {
            if (CAST_MODE) { Debug.Log("Cast mode rmb"); }
            else if((currentMana > 0)) { BLOCKING = true; }
        }
        else if(Input.GetKeyUp(secondaryFire))
        {
            if (CAST_MODE) { Debug.Log("Cast mode release rmb"); }
            else 
            { 
                BLOCKING = false;
            }
        }

        if (BLOCKING) 
        { 
            if(currentMana > 0) 
            {
                currentMana -= shieldCost;
            }
            else 
            {
                BLOCKING = false;
            }
        }
        else 
        {
            if (currentMana <= maxMana) { currentMana += manaRegen; }
        }

        if (BLOCKING) { Debug.Log("BLOCKING"); }
        if (PLAYER_DEAD) { Debug.Log("PLAYERDEAD"); }

        if (Input.GetKeyDown(slot1)) { selectedSlot = 1; }
        else if (Input.GetKeyDown(slot2)) { selectedSlot = 2; }
        else if (Input.GetKeyDown(slot3)) { selectedSlot = 3; }
        else if (Input.GetKeyDown(slot4)) { selectedSlot = 4; }
        else if (Input.GetKeyDown(slot5)) { selectedSlot = 5; }
        else if (Input.GetKeyDown(slot6)) { selectedSlot = 6; }

        if (Input.GetKeyDown(toggleCastMode))
        {
            CAST_MODE = !CAST_MODE;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "damagingProjectile")
        {
            if (!BLOCKING) 
            { 
                currentHealth -= collision.gameObject.GetComponent<ProjectileScript>().damage;
                Debug.Log("Hit!");
            }
            else 
            { 
                currentMana -= collision.gameObject.GetComponent<ProjectileScript>().damage;
                Debug.Log("Hit Shield");
            }
        }
    }

    private void StateHandler() 
    {

    }

    private void UpdateUI() 
    {
        healthBar.value = currentHealth;
        manaBar.value = currentMana;
    }

    private void UpdateRules() 
    {
        if (currentHealth <= 0) { PLAYER_DEAD = true; }

        if(currentMana <= 0) 
        { 
            NO_MANA = true; 
            currentMana = 0;
        }
        else { NO_MANA = false; }

        CAN_REGEN_MANA = !BLOCKING && (currentMana < maxMana);
    }
}

