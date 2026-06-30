using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float flightTime;

    private float flightTimer;

    public float damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flightTimer = flightTime;
    }

    // Update is called once per frame
    void Update()
    {
        flightTimer -= Time.deltaTime;

        if ( flightTimer < 0) 
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}