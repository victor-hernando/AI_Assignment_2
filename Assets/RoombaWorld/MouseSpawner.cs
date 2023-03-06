using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSpawner : MonoBehaviour
{
    public Vector2 spawnRatio;
    float elapsedTime;
    GameObject mousePrefab;

    // Start is called before the first frame update
    void Start()
    {
        mousePrefab = Resources.Load<GameObject>("MOUSE");
        elapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime -= Time.deltaTime;
        if(elapsedTime <= 0)
        {
            spawnMice();
            elapsedTime = Random.Range(spawnRatio.x, spawnRatio.y);
        }
    }

    private void spawnMice()
    {
        GameObject mouse = GameObject.Instantiate(mousePrefab);
        mouse.transform.position = RandomLocationGenerator.RandomEntryLocation();
    }
}
