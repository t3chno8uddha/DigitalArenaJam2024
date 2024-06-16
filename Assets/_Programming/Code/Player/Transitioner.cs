using Unity.Mathematics;
using UnityEngine;

public enum PosterizeState { idle, fadeIn, fadeOut }

public class Transitioner : MonoBehaviour
{
    public Material pMat;
    public PosterizeState pState = PosterizeState.fadeIn;

    public int defaultSteps = 64;
    public float sceneFadeSmoothness = 5f;

    // Singleton instance
    private static Transitioner instance;

    void Awake()
    {
        // Check if there is already an instance of Transitioner
        if (instance == null)
        {
            // If not, set this instance as the singleton instance
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            // If an instance already exists and it's not this one, destroy this object to enforce the singleton pattern
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        pMat.SetFloat("_Steps", 0);
    }

    void Update()
    {
        PosterizeHandling();
    }    

    void PosterizeHandling()
    {
        float fadeTarget = 0;

        switch (pState)
        {
            case PosterizeState.idle:
                break;

            case PosterizeState.fadeIn:
                fadeTarget = defaultSteps;
                break;

            case PosterizeState.fadeOut:
                fadeTarget = 0;
                break;
        }
        
        pMat.SetFloat("_Steps", Mathf.Lerp(pMat.GetFloat("_Steps"), fadeTarget, sceneFadeSmoothness * Time.deltaTime)) ;
        
        if (Mathf.Abs(pMat.GetFloat("_Steps") - fadeTarget) < 5)
        {
            pMat.SetFloat("_Steps", fadeTarget);
            pState = PosterizeState.fadeIn;
        }
    }
}
