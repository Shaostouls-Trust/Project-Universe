using UnityEngine;

public class HologramVolumeEffect : MonoBehaviour
{
    private Material material;
    private float glitchTimer;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        // Animated hologram effects
        glitchTimer += Time.deltaTime;

        if (glitchTimer > Random.Range(3f, 8f))
        {
            StartCoroutine(Glitch());
            glitchTimer = 0;
        }
    }

    System.Collections.IEnumerator Glitch()
    {
        //float originalIntensity = material.GetFloat("_GlitchIntensity");
        //material.SetFloat("_GlitchIntensity", Random.Range(0.1f, 0.3f));
        yield return null;//new WaitForSeconds(Random.Range(0.05f, 0.15f));
        //material.SetFloat("_GlitchIntensity", originalIntensity);
    }
}