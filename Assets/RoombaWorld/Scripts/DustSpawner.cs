using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    private GameObject dustPrefab;
    public Transform dustCollector;
    public float time;

    void Start()
    {
        dustPrefab = Resources.Load<GameObject>("DUST");
        StartCoroutine(TimeSpawner(time));
    }
    IEnumerator TimeSpawner(float time)
    {
        yield return new WaitForSeconds(time);
        GameObject lastDust = Instantiate(dustPrefab,RandomLocationGenerator.RandomWalkableLocation(), Quaternion.identity, dustCollector.transform);
        lastDust.GetComponent<SpriteRenderer>().color = Random.ColorHSV();
        StartCoroutine(TimeSpawner(this.time));
    }
}
