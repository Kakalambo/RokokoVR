using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawIndicator : MonoBehaviour
{

    [SerializeField] private float drawStepsBetweenPoints = 1000;
    [SerializeField]private ParticleSystem particle;
    private LineRenderer[] lineRenderers;
    // Start is called before the first frame update
    void Start()
    {
        StartIndicatingPath();
        DestroyIndicator(30);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIndicatingPath()
    {

        lineRenderers = GetComponentsInChildren<LineRenderer>();

        foreach (LineRenderer l in lineRenderers)
        {
            l.widthMultiplier = 0;
        }

        if (lineRenderers.Length > 0)
        {
            StartCoroutine(FollowLines());
        } 
    }

    private IEnumerator FollowLines()
    {
        int index = 0;
        foreach (LineRenderer lr in lineRenderers)
        {
            float t = 0;
            particle.transform.position = lineRenderers[0].GetPosition(0);
            particle.Play();

            for (int i2 = 0; i2 < lr.positionCount-1; i2++)
            {
                Vector3 pos1 = lineRenderers[index].GetPosition(i2);
                Vector3 pos2 = lineRenderers[index].GetPosition(i2 + 1);
                float distance = Vector3.Distance(pos1, pos2);
                int newDrawCount = Mathf.RoundToInt(drawStepsBetweenPoints * distance);

                for (int i = 0; i < newDrawCount; i ++)
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
        StartIndicatingPath();
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
