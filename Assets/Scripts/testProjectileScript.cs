using UnityEngine;

public class testProjectileScript : MonoBehaviour
{

    public Rigidbody projectileRB;

    private float spawnTimer;
    public float spawnTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnTimer = spawnTime;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer < 0)
        {
            var projectile = Instantiate(projectileRB, transform.position, transform.rotation);
            projectile.linearVelocity = transform.TransformDirection(new Vector3(0, 0, 10));
            spawnTimer = spawnTime;
        }
    }
}
