using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploding_Prop : MonoBehaviour, IInteractable
{
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float health = 0.0f;
    [SerializeField] private Rigidbody poop;
    private int currentParticles = 0;
    public int test = 1;
    [SerializeField] private AnimationCurve damageCurve;
    [SerializeField] private float fragility = 0.0f;
    [SerializeField] private Mesh mesh = null;
    [SerializeField] private GameObject effect = null;


    private void Awake()
    {
        health = maxHealth;
    }

    public void Interact()
    {
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 myVelocity = collision.relativeVelocity;
        float CollisionMass = 0;
        if (collision.rigidbody != null)
        {
            CollisionMass = collision.rigidbody.mass;
        }
        else
        {
            CollisionMass = poop.mass;
        }
        float impactVelocity = Mathf.Clamp(myVelocity.magnitude, 0.0f, 11.0f);
        float damageAmp = damageCurve.Evaluate(impactVelocity);
        //Debugging
        Debug.Log("myVelocity:" + myVelocity.magnitude +" " + "damageAmp:" + damageAmp + " " + "Impact:" + impactVelocity +"verts:" + mesh.vertexCount);
        RecieveDamage(damageAmp * (CollisionMass));

    }
    public void RecieveDamage(float damage)
    {
        Debug.Log("You got fucked up this many:" + damage);
        health -= damage;

        if (health <= maxHealth / 5)
        {
            //Make fire
            SpawnParticles();
        }
        if (health <= 0)
        {
            Debug.LogError("OH FUCK I AM ONE DEAD BARREL");
            health = 100;
            Explode();
        }
    }

    private void SpawnParticles()
    {
        int meshVertices = (mesh.vertexCount);
        int maxParticles = Mathf.Clamp(mesh.vertexCount / 10, 0, 50);
        int i = 0;
        int vertMod = Random.Range(0, meshVertices);
        int currentVert = 0;

        Vector3[] meshVerts = mesh.vertices;
        GameObject[] effectArray;

        effectArray = new GameObject[maxParticles];
        if (currentParticles < maxParticles)
        {
            for (i = 0; i < maxParticles; i++)
            {
                vertMod = Random.Range(0, meshVertices);
                currentVert = Mathf.Clamp(vertMod, 0, meshVertices);
                //effectArray[i] = Instantiate(effect, transform.TransformPoint(meshVerts[currentVert]), transform.rotation);
                effectArray[i] = Instantiate(effect, Vector3.zero, transform.rotation);
                effectArray[i].transform.parent = transform;
                currentParticles++;
            }
        }
        else
        {
            return;
        }
    }
    void Explode()
    {
        //go boom
    }

}

