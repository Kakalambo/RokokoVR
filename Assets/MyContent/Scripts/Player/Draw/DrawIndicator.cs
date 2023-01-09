using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawIndicator : MonoBehaviour
{
    
    [SerializeField] private float drawStepsBetweenPoints = 1000;
    [SerializeField] private float drawSpeed = 2;
    [SerializeField]private ParticleSystem particle;
    private LineRenderer[] lineRenderers;
    public int maxLoops = 3;
    public Vector3 offsetVector;
    private int counter;
    private Vector3 scale;
    // Start is called before the first frame update
    void Start()
    {
        StartIndicatingPath();
        DestroyIndicator(120);
        scale = transform.lossyScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIndicatingPath()
    {

        lineRenderers = GetComponentsInChildren<LineRenderer>();
        //drawSpeed = 10;
        //maxLoops = 100;
        foreach (LineRenderer l in lineRenderers)
        {
            l.widthMultiplier = 0;
        }

        if (lineRenderers.Length > 0)
        {
            StartCoroutine(FollowLines());
        } 
    }

    private Vector3 ScalePosition(Vector3 pos)
    {
        Vector3 newPos = new Vector3(pos.x * scale.x, pos.y * scale.y, pos.z * scale.z);
        newPos += offsetVector;
        return newPos;
    }

    private IEnumerator FollowLines()
    {
        int index = 0;
        foreach (LineRenderer lr in lineRenderers)
        {
            float t = 0;
            particle.transform.position = ScalePosition(lineRenderers[0].GetPosition(0));
            particle.Play();

            for (int i2 = 0; i2 < lr.positionCount-1; i2++)
            {
                Vector3 pos1 = ScalePosition(lineRenderers[index].GetPosition(i2));
                Vector3 pos2 = ScalePosition(lineRenderers[index].GetPosition(i2 + 1));
                float distance = Vector3.Distance(pos1, pos2);
                int newDrawCount = Mathf.RoundToInt(drawStepsBetweenPoints * distance);

                for (int i = 0; i < newDrawCount; i += Mathf.RoundToInt(drawSpeed))
                {

                    t = (float)i / newDrawCount;
                    particle.transform.localPosition = Vector3.Lerp(pos1, pos2, t);
                    yield return new WaitForFixedUpdate();
                }

                particle.transform.localPosition = pos2;
            }

            index++;
            particle.Stop();
            yield return new WaitForSeconds(.5f);
        }

        counter++;
        if (counter >= maxLoops)
        {
            DestroyIndicator(0);
        }
        else
        {
            StartIndicatingPath();
        }
        
    }

    public void DestroyIndicator(float timeTillDestruction)
    {
        StartCoroutine(DestroyIndicatorCoroutine(timeTillDestruction));
    }

    private IEnumerator DestroyIndicatorCoroutine(float timeTillDestruction)
    {
        yield return new WaitForSeconds(timeTillDestruction);
        particle.Stop();
        StopCoroutine(FollowLines());
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }
}
