using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptableActionData {
    public GameObject target;
    public Collision collision;

    public IEnumerable<Vector3> collisionPoints => collision
        .contacts
        .Select(contact => contact.point);
    public Vector3 collisionPointAverage => collisionPoints
        .Aggregate(Vector3.zero, (average, point) => average += point) / collision.contactCount;
}
