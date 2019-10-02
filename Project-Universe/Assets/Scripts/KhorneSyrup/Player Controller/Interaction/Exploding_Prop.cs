using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploding_Prop : MonoBehaviour, IInteractable
{
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float health = 0.0f;
    [SerializeField] private Rigidbody poop; 
    private int currentParticles = 0;
    private bool sendDamage = false;
    [SerializeField] private AnimationCurve explosionDamageCurve;
    [SerializeField] private float explosionDamageMultiplier = 1;
    [SerializeField] private float explosionDamage = 1000;
    [SerializeField] private AnimationCurve damageCurve;
    [SerializeField] private float fragility = 1.0f;
    [SerializeField] private Mesh mesh = null;
    [SerializeField] private GameObject effect = null;
    [SerializeField] private ParticleSystem explosion = null;
    [SerializeField] private List<GameObject> objectList;

    private int meshVertices, maxParticles, vertMod, currentVert;
    private Vector3[] meshVerts;
    private GameObject[] effectArray;


    private void Awake()
    {
        int meshVertices = (mesh.vertexCount);
        int maxParticles = Mathf.Clamp(mesh.vertexCount / 10, 0, 50);
        int vertMod = Random.Range(0, meshVertices);

        objectList = new List<GameObject>();
        health = maxHealth;

        //*********************Begin Effect spawning section*********************//
        meshVertices = (mesh.vertexCount);
        meshVerts = mesh.vertices;
        currentVert = 0;
        maxParticles = Mathf.Clamp(mesh.vertexCount / 10, 0, 50);
        effectArray = new GameObject[maxParticles];
        vertMod = Random.Range(0, meshVertices);

        int i = 0;
        effectArray = new GameObject[maxParticles];
        if (currentParticles < maxParticles)
        {
            for (i = 0; i < maxParticles; i++)
            {
                vertMod = Random.Range(0, meshVertices);
                currentVert = Mathf.Clamp(vertMod, 0, meshVertices);
                effectArray[i] = Instantiate(effect, transform.TransformPoint(meshVerts[currentVert]), transform.rotation);
                effectArray[i].transform.parent = transform;
                currentParticles++;
                effectArray[i].GetComponent<ParticleSystem>().Stop();
            }
        }
        //*********************End Effect spawning section*********************//
    }

    public void Interact()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.layer == 12 || other.gameObject.layer == 11) && other.isTrigger == false)
        {
            objectList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.layer == 12 || other.gameObject.layer == 11) && other.isTrigger == false)
        {
            objectList.Remove(other.gameObject);
        }
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
        //Debug.Log("myVelocity:" + myVelocity.magnitude +" " + "damageAmp:" + damageAmp + " " + "Impact:" + impactVelocity +"verts:" + mesh.vertexCount);
        RecieveDamage(damageAmp * CollisionMass * fragility);

    }
    public void RecieveDamage(float damage)
    {
       // Debug.Log("You got fucked up this many:" + damage);
        health -= damage;
        Debug.Log(damage);

        if (health <= maxHealth / 5)
        {
            //Make fire
            SpawnParticles();
        }
        if (health <= 0)
        {
            Debug.LogError("OH BISCUIT BITCH I AM ONE DEAD BARREL");
            Explode();
        }
    }

    private void SpawnParticles()
    {
        foreach (GameObject obj in effectArray)
        {
            obj.GetComponent<ParticleSystem>().Play();
        }
    }

    private void Explode()
    {
        ParticleSystem explosionObj = new ParticleSystem();
        float expDur = explosionObj.main.duration + explosion.main.startLifetimeMultiplier;
        sendDamage = true;
        Debug.LogError("KABOOOOOOOOOOOOOOOOOM");
        explosionObj = Instantiate(explosion, transform.position, transform.rotation);
        explosionObj.transform.parent = transform;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        Debug.Log(objectList.Count);
        foreach (GameObject obj in objectList)
        {
            IInteractable targetScr = obj.GetComponent<IInteractable>();
            Debug.Log(targetScr);
            Vector3 dir = (obj.transform.position - transform.position).normalized;
            float explosionDamageMult = 0;
            float explosionDamageValue = explosionDamage;
            float targetDist = Vector3.Distance(obj.transform.position, gameObject.transform.position) * explosionDamageMultiplier;

            explosionDamageMult = Mathf.Clamp(explosionDamageCurve.Evaluate(explosionDamage / targetDist), 0, 10000);

            explosionDamageValue = explosionDamageCurve.Evaluate(explosionDamageMult * explosionDamageMultiplier);

            targetScr.RecieveDamage(explosionDamageValue);
            obj.GetComponent<Rigidbody>().AddForce(dir * 100.0f);
        }
        Destroy(gameObject, expDur);
        //go boom
    }

}

