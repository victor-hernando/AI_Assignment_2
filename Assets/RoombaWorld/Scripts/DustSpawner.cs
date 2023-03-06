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

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator TimeSpawner(float time)
    {
        Debug.Log("INSIDE");
        yield return new WaitForSeconds(time);
        Instantiate(dustPrefab,RandomLocationGenerator.RandomWalkableLocation(), Quaternion.identity, dustCollector.transform);
        StartCoroutine(TimeSpawner(this.time));
    }
}
