using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 5;

    public GameObject progenitor;

    public Transform rTransform;

    bool marked;
    public float countdown = 3;
    float lifetime = 10f;

    //public Transform lookatTarget;

    void Start()
    {
        StartCoroutine(Timer());
    }

    void Update()
    {
        transform.position = transform.position + transform.forward * projectileSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject != progenitor)
        {
            IDamageable hit = col.gameObject.GetComponent<IDamageable>();
            if (hit != null)
            {
                hit.Damage(false);
            }

            StartCoroutine(Proceed());
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds (lifetime);

        if (!marked)
        {
            Proceed();
        }
    }

    IEnumerator Proceed()
    {
        marked = true;
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
        Destroy(rTransform.gameObject);

        yield return new WaitForSeconds(countdown);

        Destroy(gameObject);
    }
}