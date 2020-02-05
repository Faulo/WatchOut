using Slothsoft.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SpawnParticles : ScriptableAction {
    [SerializeField, Expandable]
    ParticleSystem particleSystemPrefab = default;
    public override void Act(ScriptableActionData data) {
        if (data.collision != null) {
            var rotation = Quaternion.LookRotation(data.collision.impulse);
            InstantiateParticles(data.collisionPointAverage, rotation);
        }
    }
    void InstantiateParticles(Vector3 position, Quaternion rotation) {
        var particleSystem = Instantiate(particleSystemPrefab, position, rotation);
        Destroy(particleSystem.gameObject, particleSystem.main.duration);
    }
}
