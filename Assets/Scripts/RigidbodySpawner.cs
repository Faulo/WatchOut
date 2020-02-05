using Slothsoft.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodySpawner : MonoBehaviour {
    [SerializeField]
    Rigidbody[] prefabs = null;

    [SerializeField, Range(0, 1000)]
    int initialSpawnCount = 0;

    [SerializeField, Range(0, 10)]
    float interval = 1;

    [SerializeField, Range(0, 100)]
    float force = 0;

    IEnumerator Start() {
        for (int i = 0; i < initialSpawnCount; i++) {
            Spawn(Mathf.Sqrt(i));
            yield return null;
        }
        while (true) {
            Spawn();
            yield return new WaitForSeconds(interval);
        }
    }

    private void Spawn(float offset = 0) {
        var prefab = prefabs.RandomElement();
        var direction = Random.insideUnitCircle;
        var body = Instantiate(prefab, transform.position + offset * Random.insideUnitSphere, prefab.rotation);
        body.AddForce(force * new Vector3(direction.x, 1, direction.y), ForceMode.Acceleration);
        body.GetComponent<Part>()?.SetToPhysical();
    }
}
