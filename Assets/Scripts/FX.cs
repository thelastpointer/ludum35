using UnityEngine;
using System.Collections;

public class FX : MonoBehaviour
{
    public GameObject MissEffect;
    public GameObject DeflectEffect;
    public GameObject HitEffect;
    public GameObject HealEffect;
    public GameObject DelayedEffect;

    public float RandomOffset = 0;

    static FX instance;

    void Awake()
    {
        instance = this;
    }

    public static void DoMissEffect(Vector3 pos)
    {
        GameObject go = Instantiate(instance.MissEffect);
        go.transform.position = pos + (Vector3)(Random.insideUnitCircle * instance.RandomOffset);
    }
    public static void DoHitEffect(Vector3 pos)
    {
        GameObject go = Instantiate(instance.HitEffect);
        go.transform.position = pos + (Vector3)(Random.insideUnitCircle * instance.RandomOffset);
    }
    public static void DoDeflectEffect(Vector3 pos)
    {
        GameObject go = Instantiate(instance.DeflectEffect);
        go.transform.position = pos + (Vector3)(Random.insideUnitCircle * instance.RandomOffset);
    }
    public static void DoHealEffect(Vector3 pos)
    {
        GameObject go = Instantiate(instance.HealEffect);
        go.transform.position = pos + (Vector3)(Random.insideUnitCircle);
    }
    public static void DoDelayedEffect(Vector3 pos)
    {
        GameObject go = Instantiate(instance.DelayedEffect);
        go.transform.position = pos + (Vector3)(Random.insideUnitCircle);
    }
}
