using Slothsoft.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ImpactListener : MonoBehaviour {
    [SerializeField, Range(0, 10)]
    float impactThreshold = 0;

    [SerializeField]
    ScriptableAction[] impactActions;

    void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude > impactThreshold) {
            var data = new ScriptableActionData() {
                target = gameObject,
                collision = collision
            };
            impactActions.ForAll(action => action.Act(data));
        }
    }
}
