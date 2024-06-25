using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioPeer : MonoBehaviour
{
    [System.Serializable]
    public struct RandomBeat : System.IComparable<RandomBeat>
    {
        public int id;
        public float time;

        public RandomBeat(int id, float time)
        {
            this.id = id;
            this.time = time;
        }

        public readonly int CompareTo(RandomBeat other) => time.CompareTo(other.time);
    }

    [System.Serializable]
    public struct Lane
    {
        public Transform start;
        public Transform end;

        public Lane(Transform start, Transform end)
        {
            this.start = start;
            this.end = end;
        }
    }

    public Color[] colors;

    public Transform drum;
    public Lane leftLane;
    public Lane midLane;
    public Lane rightLane;
    public GameObject enemyPrefab;
    public float totalMoveTime = 1.0f;
    public float perfectBarrierDistance = 1.0f;
    public float perfectBarrierRange = 1.0f;

    public AudioSource audioSource;
    public Transform barParent;
    public float[] samples = new float[512];
    public float[] frequencyBands = new float[8];
    public float[] bandBuffer = new float[8];
    public float[] bandHighest = new float[8];
    public float[] normalizedValues = new float[8];

    private float[] m_BandDecrese = new float[8];

    public float musicTotalTime;
    public int numberOfBeatsToGenerate;
    public List<RandomBeat> randomBeatsTime;
    public int currentIndexForPreBeat;
    public int currentIndexForBeat;

    public float currentTimeForPreBeat;
    public float currentTimeForBeat;

    public List<EnemyMover> enemies;
    public List<EnemyMover> perfectEnemies;

    [ContextMenu("Generate Random Beats")]
    public void GenerateRandomBeats()
    {
        randomBeatsTime ??= new List<RandomBeat>();
        randomBeatsTime.Clear();

        for (int i = 0; i < numberOfBeatsToGenerate; i++)
        {
            int randomID = Random.Range(0, 6);
            float randomTime = Random.Range(0f, musicTotalTime);
            randomBeatsTime.Add(new(randomID, randomTime));
        }

        randomBeatsTime.Sort();
    }

    private void Start()
    {
        currentTimeForBeat = -totalMoveTime;
        enemies = new List<EnemyMover>();
        perfectEnemies = new List<EnemyMover>();
        drum.GetComponent<Drum>().onDrumPadTapped = OnDrumPadTapped;
    }

    private void OnDrawGizmos()
    {
        const float startAndEndSphereRadius = 0.1f;
        const float perfectBarrierCenterSphereRadius = 0.075f;
        const float perfectBarrierEndSphereRadius = 0.05f;

        Gizmos.color = Color.cyan * 0.8f;
        Gizmos.DrawSphere(leftLane.start.position, startAndEndSphereRadius);
        Gizmos.DrawSphere(midLane.start.position, startAndEndSphereRadius);
        Gizmos.DrawSphere(rightLane.start.position, startAndEndSphereRadius);

        Gizmos.color = Color.magenta * 0.8f;
        Gizmos.DrawSphere(leftLane.end.position, startAndEndSphereRadius);
        Gizmos.DrawSphere(midLane.end.position, startAndEndSphereRadius);
        Gizmos.DrawSphere(rightLane.end.position, startAndEndSphereRadius);

        Gizmos.color = Color.white * 0.5f;
        Gizmos.DrawLine(leftLane.start.position, leftLane.end.position);
        Gizmos.DrawLine(midLane.start.position, midLane.end.position);
        Gizmos.DrawLine(rightLane.start.position, rightLane.end.position);

        Vector3 dirA = (leftLane.start.position - leftLane.end.position).normalized;
        Vector3 barrierPosA = leftLane.end.position + dirA * perfectBarrierDistance;
        Gizmos.color = Color.white * 0.75f;
        Gizmos.DrawSphere(barrierPosA, perfectBarrierCenterSphereRadius);
        Gizmos.color = Color.black * 0.5f;
        Gizmos.DrawSphere(barrierPosA + 0.5f * perfectBarrierRange * dirA, perfectBarrierEndSphereRadius);
        Gizmos.DrawSphere(barrierPosA - 0.5f * perfectBarrierRange * dirA, perfectBarrierEndSphereRadius);

        Vector3 dirB = (midLane.start.position - midLane.end.position).normalized;
        Vector3 barrierPosB = midLane.end.position + dirB * perfectBarrierDistance;
        Gizmos.color = Color.white * 0.75f;
        Gizmos.DrawSphere(barrierPosB, perfectBarrierCenterSphereRadius);
        Gizmos.color = Color.black * 0.5f;
        Gizmos.DrawSphere(barrierPosB + 0.5f * perfectBarrierRange * dirB, perfectBarrierEndSphereRadius);
        Gizmos.DrawSphere(barrierPosB - 0.5f * perfectBarrierRange * dirB, perfectBarrierEndSphereRadius);

        Vector3 dirC = (rightLane.start.position - rightLane.end.position).normalized;
        Vector3 barrierPosC = rightLane.end.position + dirC * perfectBarrierDistance;
        Gizmos.color = Color.white * 0.75f;
        Gizmos.DrawSphere(barrierPosC, perfectBarrierCenterSphereRadius);
        Gizmos.color = Color.black * 0.5f;
        Gizmos.DrawSphere(barrierPosC + 0.5f * perfectBarrierRange * dirC, perfectBarrierEndSphereRadius);
        Gizmos.DrawSphere(barrierPosC - 0.5f * perfectBarrierRange * dirC, perfectBarrierEndSphereRadius);
    }

    private void Update()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);

        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float avg = 0.0f;
            int sampleCount = (int)Mathf.Pow(2, i + 1);

            if (i == 7)
                sampleCount += 2;

            for (int j = 0; j < sampleCount; j++)
            {
                avg += samples[count] * (count + 1);
                count++;
            }

            avg /= count;
            frequencyBands[i] = avg * 10.0f;
        }

        for (int i = 0; i < 8; i++)
        {
            if (frequencyBands[i] > bandBuffer[i])
            {
                bandBuffer[i] = frequencyBands[i];
                m_BandDecrese[i] = 0.005f;
            }

            if (frequencyBands[i] < bandBuffer[i])
            {
                bandBuffer[i] -= m_BandDecrese[i];
                m_BandDecrese[i] *= 1.2f;
            }
        }

        for (int i = 0; i < 8; i++)
        {
            if (bandBuffer[i] > bandHighest[i])
                bandHighest[i] = bandBuffer[i];
        }

        for (int i = 0; i < 8; i++)
        {
            normalizedValues[i] = bandBuffer[i] / bandHighest[i];
        }

        for (int i = 0; i < 8; i++)
        {
            Transform bar = barParent.GetChild(i);
            Vector3 scale = bar.localScale;
            scale.y = 1.0f + Mathf.Lerp(0.0f, 4.0f, normalizedValues[i]);
            if (float.IsNaN(scale.y))
                continue;
            bar.localScale = scale;
        }

        currentTimeForPreBeat += Time.deltaTime;

        if (currentIndexForPreBeat < randomBeatsTime.Count && currentTimeForPreBeat > randomBeatsTime[currentIndexForPreBeat].time)
        {
            OnPreBeat(randomBeatsTime[currentIndexForPreBeat]);
            currentIndexForPreBeat++;
        }

        currentTimeForBeat += Time.deltaTime;

        if (currentIndexForBeat < randomBeatsTime.Count && currentTimeForBeat > randomBeatsTime[currentIndexForBeat].time)
        {
            OnBeat(randomBeatsTime[currentIndexForBeat]);
            currentIndexForBeat++;
        }

        Time.timeScale = (perfectEnemies.Count > 0) ? 0.8f : 1.0f;
    }

    private void OnBeat(RandomBeat beat)
    {
        GameObject drumPad = drum.GetChild(beat.id).gameObject;
        if (drumPad.TryGetComponent(out MeshRenderer meshRenderer))
        {
            //meshRenderer.material.color = Random.ColorHSV();
        }
    }

    private void OnPreBeat(RandomBeat beat)
    {
        if (beat.id == 0 || beat.id == 3)
        {
            SpawnEnemy(beat, leftLane);
        }

        if (beat.id == 1 || beat.id == 4)
        {
            SpawnEnemy(beat, midLane);
        }

        if (beat.id == 2 || beat.id == 5)
        {
            SpawnEnemy(beat, rightLane);
        }
    }

    private void SpawnEnemy(RandomBeat beat, Lane lane)
    {
        Transform start = lane.start;
        Transform end = lane.end;

        GameObject enemyGO = Instantiate(enemyPrefab, start.position, Quaternion.identity);
        if (enemyGO == null)
            return;

        enemyGO.name = $"Enemy {beat.id}";
        
        if (enemyGO.TryGetComponent(out EnemyMover mover))
        {
            enemies.Add(mover);

            mover.id = beat.id;
            mover.startPos = start.position;
            mover.endPos = end.position;
            mover.onDestinationReached = OnDestinationReachedByEnemyButNotFreedFromQueue;
            mover.onEnterPerfectRegion = OnEnemyEnterPerfectRegion;
            mover.onExitPerfectRegion = OnEnemyExitPerfectRegion;

            Vector3 endToStartDir = (start.position - end.position).normalized;
            Vector3 barrierCenterPos = end.position + endToStartDir * perfectBarrierDistance;
            Vector3 barrierStartPos = barrierCenterPos + 0.5f * perfectBarrierRange * endToStartDir;
            Vector3 barrierExitPos = barrierCenterPos - 0.5f * perfectBarrierRange * endToStartDir;

            float distanceTillBarrier = Vector3.Distance(start.position, barrierCenterPos);
            float speed = distanceTillBarrier / totalMoveTime;
            float fullDistance = Vector3.Distance(start.position, end.position);
            float distanceTillBarrierStart = Vector3.Distance(start.position, barrierStartPos);
            float distanceTillBarrierExit = Vector3.Distance(start.position, barrierExitPos);

            mover.totalTime = fullDistance / speed;
            mover.barrierEnterTime = distanceTillBarrierStart / speed;
            mover.barrierExitTime = distanceTillBarrierExit / speed;
        }

        if (enemyGO.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.material.color = colors[beat.id];
        }    
    }

    private void OnDestinationReachedByEnemyButNotFreedFromQueue(GameObject enemyGO)
    {
        EnemyMover mover = enemyGO.GetComponent<EnemyMover>();
        enemies.Remove(mover);
        if (perfectEnemies.Contains(mover))
            perfectEnemies.Remove(mover);
    }

    private void OnEnemyEnterPerfectRegion(GameObject enemyGO)
    {
        perfectEnemies.Add(enemyGO.GetComponent<EnemyMover>());
    }

    private void OnEnemyExitPerfectRegion(GameObject enemyGO)
    {
        //perfectEnemies.Remove(enemyGO.GetComponent<EnemyMover>());
    }

    private void OnDrumPadTapped(int drumPadID)
    {
        if (perfectEnemies.Count == 0)
            return;

        EnemyMover mover = perfectEnemies[0];
        if (mover.id == drumPadID)
        {
            perfectEnemies.RemoveAt(0);
            if (enemies.Contains(mover))
                enemies.Remove(mover);
            Destroy(mover.gameObject);
        }    
    }    
}
