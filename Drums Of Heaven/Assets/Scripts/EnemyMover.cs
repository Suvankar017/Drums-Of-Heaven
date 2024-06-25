using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public int id;
    public Vector3 startPos;
    public Vector3 endPos;
    public float totalTime;
    public float barrierEnterTime;
    public float barrierExitTime;
    public System.Action<GameObject> onDestinationReached;
    public System.Action<GameObject> onEnterPerfectRegion;
    public System.Action<GameObject> onExitPerfectRegion;

    private float currentTime;
    private bool hasEntered;
    private bool hasExited;

    private void Update()
    {
        currentTime += Time.deltaTime;
        transform.position = Vector3.Lerp(startPos, endPos, currentTime / totalTime);

        if (currentTime > barrierEnterTime && !hasEntered)
        {
            hasEntered = true;
            onEnterPerfectRegion?.Invoke(gameObject);
        }

        if (currentTime > barrierExitTime && !hasExited)
        {
            hasExited = true;
            onExitPerfectRegion?.Invoke(gameObject);
        }

        if (currentTime >= totalTime)
        {
            onDestinationReached?.Invoke(gameObject);
            Destroy(gameObject);
        }    
    }
}
