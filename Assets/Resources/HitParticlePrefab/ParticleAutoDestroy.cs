using UnityEngine;

public class ParticleAutoDestroy : MonoBehaviour
{
    public float lifetime = -1f;
    private ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        float t = lifetime > 0 ? lifetime : ps.main.duration + 0.1f;
        Destroy(gameObject, t);
    }
}
