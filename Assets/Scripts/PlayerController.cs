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
    public float parryCost;
    public float manaRegen;
    public float parryWindow;
    private float parryTimer;
    public float blockCooldown;
    private float blockCooldownTimer;

    [Header("Projectiles")]
    public Rigidbody playerProjectile;

    [Header("Rules")]
    bool CAN_BLOCK;
    bool NO_MANA;
    bool PLAYER_DEAD;
    bool CASTING;
    bool PRIMARY_FIRE;
    bool SECONDARY_FIRE_PRESSED;
    bool SECONDARY_FIRE_HELD;
    bool TOGGLE_CAST_MODE;

    private enum playerState
    {
        Base,
        Blocking,
        Parry,
        Casting,
        Cast_Mode
    }

    private playerState currentState;
    private playerState lastState;

    private bool changedState = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedSlot = 1;
        PLAYER_DEAD = false;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        currentHealth = maxHealth;

        manaBar.maxValue = maxMana;
        manaBar.value = maxMana;
        currentMana = maxMana;

        currentState = playerState.Base;
        lastState = playerState.Base;

        parryTimer = 0;
        blockCooldownTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        StateHandler();
        UpdateRules();
        UpdateUI();
    }

    private void GetInputs()
    {
        if (Input.GetKeyDown(primaryFire))
        {
            PRIMARY_FIRE = true;
        }
        else
        {
            PRIMARY_FIRE = false;
        }

        if (Input.GetKeyDown(secondaryFire)) { SECONDARY_FIRE_PRESSED = true; }
        else{ SECONDARY_FIRE_PRESSED = false; }
        if (Input.GetKey(secondaryFire) && !SECONDARY_FIRE_PRESSED) { SECONDARY_FIRE_HELD = true; }
        else { SECONDARY_FIRE_HELD = false; }

        if (Input.GetKeyDown(slot1)) { selectedSlot = 1; }
        else if (Input.GetKeyDown(slot2)) { selectedSlot = 2; }
        else if (Input.GetKeyDown(slot3)) { selectedSlot = 3; }
        else if (Input.GetKeyDown(slot4)) { selectedSlot = 4; }
        else if (Input.GetKeyDown(slot5)) { selectedSlot = 5; }
        else if (Input.GetKeyDown(slot6)) { selectedSlot = 6; }

        if (Input.GetKeyDown(toggleCastMode))
        {
            TOGGLE_CAST_MODE = true;
        }
        else { TOGGLE_CAST_MODE = false; }
    }

    private void StateHandler() 
    {
        if(currentState != lastState) {lastState = currentState; changedState = true; }

        switch (currentState) 
        {
            case playerState.Base:
                if (PRIMARY_FIRE) 
                {
                    var projectile = Instantiate(playerProjectile, orientaion.position, orientaion.rotation);
                    projectile.linearVelocity = orientaion.transform.TransformDirection(new Vector3(0, 0, 10));
                    Physics.IgnoreCollision(projectile.GetComponent<Collider>(), GetComponent<Collider>());
                    break;
                }
                else if (SECONDARY_FIRE_PRESSED) 
                {
                    if (NO_MANA || !CAN_BLOCK) { break; }
                    currentState = playerState.Parry;
                    parryTimer = parryWindow;
                    break;
                }
                else if (TOGGLE_CAST_MODE)
                {
                    currentState = playerState.Cast_Mode;
                    TOGGLE_CAST_MODE = false;
                    break;
                }
                else 
                {
                    if(currentMana <= maxMana) { currentMana += manaRegen; }
                    else { currentMana = maxMana; }
                    break;
                } 

            case playerState.Parry:
                if (changedState) { currentMana -= parryCost; }
                if (parryTimer <= 0)
                {
                    if (SECONDARY_FIRE_HELD) 
                    { 
                        currentState = playerState.Blocking; 
                        break; 
                    }
                    currentState = playerState.Base;
                    blockCooldownTimer = blockCooldown;
                    break;
                }
                parryTimer -= Time.deltaTime;
                break;

            case playerState.Blocking:
                if (NO_MANA || !SECONDARY_FIRE_HELD) 
                { 
                    currentState = playerState.Base;
                    blockCooldownTimer = blockCooldown;
                    break; 
                }
                currentMana -= shieldCost;
                break;

            case playerState.Cast_Mode:
                if (TOGGLE_CAST_MODE) { currentState = playerState.Base; }
                break;
        }
        if (changedState) { changedState = false; }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "damagingProjectile" && currentState != playerState.Parry)
        {
            if (currentState != playerState.Blocking)
            {
                currentHealth -= collision.gameObject.GetComponent<ProjectileScript>().damage;
                //Debug.Log("Hit!");
            }
            else
            {
                currentMana -= collision.gameObject.GetComponent<ProjectileScript>().damage;
                //Debug.Log("Hit Shield");
            }
        }
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

        if(blockCooldownTimer > 0) 
        {
            blockCooldownTimer -= Time.deltaTime;
            CAN_BLOCK = false;
        }
        else { CAN_BLOCK = true; }
    }

    private void UpdateUI()
    {
        healthBar.value = currentHealth;
        manaBar.value = currentMana;
    }
}

