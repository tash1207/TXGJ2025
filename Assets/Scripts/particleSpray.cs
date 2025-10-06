using UnityEngine;

public class particleSpray : MonoBehaviour
{
    public ParticleSystem sprayAttack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sprayAttack.Play();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
