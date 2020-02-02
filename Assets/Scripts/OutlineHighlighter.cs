using OculusSampleFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineHighlighter : MonoBehaviour {
    [SerializeField, Range(0, 1)]
    float width = 0.1f;
    [SerializeField]
    DistanceGrabber target = default;
    [SerializeField]
    MeshFilter outlinePrefab = default;

    IDictionary<DistanceGrabbable, MeshFilter> outlines = new Dictionary<DistanceGrabbable, MeshFilter>();

    void OnEnable() {
        target.onFocus += Hightlight;
        target.onBlur += Unhighlight;
    }
    void OnDisable() {
        target.onFocus -= Hightlight;
        target.onBlur -= Unhighlight;
    }

    void Hightlight(DistanceGrabbable grabbable) {
        if (outlines.ContainsKey(grabbable)) {
            throw new ArgumentOutOfRangeException(string.Format("Grabbable {0} is already highlighted!", grabbable));
        }
        var outlineInstance = Instantiate(outlinePrefab, grabbable.meshFilter.transform);
        outlineInstance.transform.localPosition = Vector3.zero;
        outlineInstance.transform.localRotation = Quaternion.identity;
        outlineInstance.transform.localScale = Vector3.one * (1 + width);
        outlineInstance.mesh = grabbable.meshFilter.mesh;

        outlines[grabbable] = outlineInstance;
    }

    void Unhighlight(DistanceGrabbable grabbable) {
        if (!outlines.ContainsKey(grabbable)) {
            throw new ArgumentOutOfRangeException(string.Format("Grabbable {0} hasn't been highlighted!", grabbable));
        }
        Destroy(outlines[grabbable].gameObject);
        outlines.Remove(grabbable);
    }
}
