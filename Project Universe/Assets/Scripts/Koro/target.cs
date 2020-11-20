
using UnityEngine;
using UnityEngine.AI;

public class target : MonoBehaviour
{
    public float health = 50f;
    public float lookRadius = 10f;
    public Transform player;
    Rigidbody rig;
    public float speed;
    public bool isInRange = false;
    public AudioSource takedmgSFX;

   

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
    
    }
    public void TakeDamage(float amount   )
    {
        health -= amount;
        takedmgSFX.Play();
        if (health <= 0f)
        {
           
            GameObject.Find("GameManager").SendMessage("onemorevirusisdead");
            //GameObject.Find("GameManager").SendMessage("Spawn");
            Die();
        }
    }
    void Die()
    {
        Destroy(gameObject);
    }
    private void FixedUpdate()
    {
       
        Vector3 pos = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        rig.MovePosition(pos);
        transform.LookAt(player);

    }   
}
