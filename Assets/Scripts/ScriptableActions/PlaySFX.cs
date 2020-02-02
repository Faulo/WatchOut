using Slothsoft.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlaySFX : ScriptableAction {
    [SerializeField, Expandable]
    AudioClip[] clips = default;
    public override void Act(ScriptableActionData data) {
        var audioSource = data.target.GetComponent<AudioSource>();
        if (!audioSource) {
            audioSource = data.target.AddComponent<AudioSource>();
        }
        audioSource.PlayOneShot(clips.RandomElement());
    }
}
