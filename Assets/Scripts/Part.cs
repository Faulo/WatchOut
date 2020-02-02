using OculusSampleFramework;
using Slothsoft.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Part : MonoBehaviour {
    [Header("Level design")]
    [SerializeField, Expandable]
    Part[] preParts;
    [SerializeField, Expandable]
    Part[] blockingParts;

    [Header("Part configuration")]
    [SerializeField]
    Dimension dimension = Dimension.BLUEPRINT;
    Dimension oldDimension;
    [SerializeField]
    State state = State.Off;
    [SerializeField]
    PartType type = default;

    [SerializeField, Range(0, 10)]
    float maximumSnapDistance = 1.0f;

    [Header("Related Components")]
    [SerializeField]
    Renderer meshRenderer = default;
    [SerializeField]
    DistanceGrabbable grabbable = default;
    [SerializeField]
    new Rigidbody rigidbody = default;
    bool previousIsKinematic;
    [SerializeField]
    Collider physicalCollider = default;
    [SerializeField]
    Collider blueprintCollider = default;
    [SerializeField]
    SimpleGear gearRotation = default;
    [SerializeField]
    Material physicalMaterial = default;
    [SerializeField]
    Material placableHologramMaterial = default;
    [SerializeField]
    Material blockedHologramMaterial = default;

    [Header("Debug info")]
    public bool isAttached = false;
    public bool isAttachable = false;

    // Instance Information
    private float boundingZ;
    private float precision;
    private Part attachedPart = default;
    private Part attachedBlueprint = default;

    #region Mouse stuff
    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown() {
        if (dimension == Dimension.PHYSICAL) {
            screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        } else {
            if (attachedPart != null) {
                screenPoint = Camera.main.WorldToScreenPoint(attachedPart.transform.position);
                offset = attachedPart.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
        }
    }

    void OnMouseDrag() {
        if (dimension == Dimension.PHYSICAL) {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        } else {
            if (attachedPart != null) {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                attachedPart.transform.position = curPosition;
            }
        }
    }
    #endregion

    private enum Dimension {
        BLUEPRINT,
        PHYSICAL
    };

    private enum State {
        Off,
        PlacableHologram,
        BlockedHologram,
        Loose,
        Placed,
        Grabbed
    };

    private enum PartType {
        GEAR01,
        GEAR02,
        GEAR03,
        GEAR04,
        GEAR05,
        GEAR06,
        GEAR07,
        GEAR08,
        GEAR09,
        GEAR10,
        GEAR11,
        GEAR12,
        GEAR13,
        GEAR14,
        GEAR15,
        BALANCE_WHEEL,
        COIL,
        JEWEL,
        PALETTE,
        OTHER
    };

    // Start is called before the first frame update
    void Start() {
        previousIsKinematic = rigidbody.isKinematic;
        oldDimension = dimension;
        ApplyDimension();
        boundingZ = physicalCollider.bounds.size.z;
    }

    // Update is called once per frame
    void Update() {
        // Check current state of object
        CheckDimension();
        isAttachable = CheckForAttachable();

        if (dimension == Dimension.BLUEPRINT) {
            Debug.Log(nearbyParts.Count);
        }
        nearbyParts.ForAll(ProcessPart);
    }

    void FixedUpdate() {
        switch (dimension) {
            case Dimension.BLUEPRINT:
                if (isAttachable && isAttached) {
                    throw new Exception("part cannot be attachable and attached");
                }
                if (isAttachable && !isAttached) {
                    SetState(State.PlacableHologram);
                }
                if (!isAttachable && isAttached) {
                    SetState(State.Off);
                }
                if (!isAttachable && !isAttached) {
                    SetState(State.BlockedHologram);
                }
                break;
            case Dimension.PHYSICAL:
                if (previousIsKinematic != rigidbody.isKinematic) {
                    previousIsKinematic = rigidbody.isKinematic;
                    SetState(rigidbody.isKinematic ? State.Grabbed : State.Loose);
                }
                break;
        }
    }

    // Checks if dimension has changed and if so adjusts the settings
    private void CheckDimension() {
        if (!dimension.Equals(oldDimension)) {
            oldDimension = dimension;
            ApplyDimension();
        }
    }

    private void ApplyDimension() {
        switch (dimension) {
            case Dimension.BLUEPRINT:
                physicalCollider.enabled = false;
                blueprintCollider.enabled = true;
                grabbable.Grabbable = false;
                SetState(State.PlacableHologram);
                break;
            case Dimension.PHYSICAL:
                physicalCollider.enabled = true;
                blueprintCollider.enabled = false;
                grabbable.Grabbable = true;
                SetState(State.Loose);
                break;
        }
    }

    void SetState(State state) {
        if (this.state != state) {
            ExitState();
            this.state = state;
            EnterState();
            previousIsKinematic = rigidbody.isKinematic;
        }
    }

    void EnterState() {
        switch (state) {
            // Blueprint states
            case State.Off:
                meshRenderer.enabled = false;
                rigidbody.isKinematic = true;
                break;
            case State.PlacableHologram:
                meshRenderer.material = placableHologramMaterial;
                rigidbody.isKinematic = true;
                break;
            case State.BlockedHologram:
                meshRenderer.material = blockedHologramMaterial;
                rigidbody.isKinematic = true;
                break;

            // Physical states
            case State.Loose:
                meshRenderer.material = physicalMaterial;
                rigidbody.isKinematic = false;
                break;
            case State.Grabbed:
                meshRenderer.material = physicalMaterial;
                rigidbody.isKinematic = true;
                break;
            case State.Placed:
                meshRenderer.material = physicalMaterial;
                rigidbody.isKinematic = true;
                gearRotation.ActivateGear();
                break;
        }
    }

    void ExitState() {
        switch (state) {
            // Blueprint states
            case State.Off:
                meshRenderer.enabled = true;
                break;
            case State.PlacableHologram:
            case State.BlockedHologram:
                break;

            // Physical states
            case State.Loose:
                break;
            case State.Grabbed:
                break;
            case State.Placed:
                gearRotation.DeactivateGear();
                break;
        }
    }

    ISet<Part> nearbyParts = new HashSet<Part>();
    void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Part otherPart)) {
            nearbyParts.Add(otherPart);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out Part otherPart)) {
            //nearbyParts.Remove(otherPart);
        }
    }
    // IF BLUEPRINT:

    // Detect if this blueprint is active and the collided object is correct (has same part type as blueprint).
    // If so and threshold is met, set collided object to blueprint position.
    private void OnTriggerStay(Collider other) {
        if (other.TryGetComponent(out Part otherPart)) {
            //ProcessPart(otherPart);
        }
    }

    void ProcessPart(Part otherPart) {
        Debug.Log(otherPart.state);
        switch (dimension) {
            case Dimension.PHYSICAL:
                attachedBlueprint = otherPart;
                break;
            case Dimension.BLUEPRINT:
                if (otherPart.dimension == Dimension.PHYSICAL) {
                    if (isAttachable) {
                        if (otherPart.type == type)
                            Debug.Log(otherPart.state);
                        if (otherPart.type == type && otherPart.state == State.Loose) {
                            // Test if collided objects meets threshold
                            float distance = Vector3.Distance(otherPart.transform.position, transform.position);
                            if (distance <= maximumSnapDistance) { // * boundingZ
                                                                   // Set collided object to blueprint position, save reference and disable rigidbody
                                otherPart.transform.SetPositionAndRotation(transform.position, transform.rotation);
                                otherPart.SetState(State.Placed);
                                attachedPart = otherPart;

                                // Set necessary variables
                                isAttached = true;
                                isAttachable = false;
                                Debug.Log("Attached part with " + distance + " units precision!");
                            }
                        }
                    }
                }
                break;
        }
    }

    bool IsGrabbing() {
        return Input.GetMouseButton(0) || grabbable.isGrabbed;
    }

/*
private void OnTriggerExit(Collider other) {
    if (dimension.Equals(Dimension.BLUEPRINT) && other.attachedRigidbody) {
        if (other.TryGetComponent(out Part part) && part.IsGrabbing()) {
            if (part.attachedBlueprint == this) {

                if (!CheckForBlocked()) {
                    // Enable physical object rigidbody and delete part reference
                    other.GetComponent<Rigidbody>().isKinematic = false;
                    part.attachedBlueprint = null;
                    attachedPart = null;

                    // Enable blueprint
                    blueprintRenderer.enabled = true;

                    // Set necessary variables
                    IsAttached = false;
                } else {
                    other.GetComponent<Rigidbody>().isKinematic = false;
                    part.attachedBlueprint = null;
                    attachedPart = null;
                    // Enable blueprint
                    blueprintRenderer.enabled = true;

                    // Set necessary variables
                    IsAttached = false;

                    part.WreakHavoc();
                }
            }
        }
    }
}
//*/


    private bool CheckForAttachable() {
        if (dimension.Equals(Dimension.BLUEPRINT)) {
            // Check if underlying parts are attached. If there are no underlying parts, part can always be attached. 

            // If there is already an attached part, no part can be attached anymore.
            if (isAttached) {
                return false;
            }

            foreach (Part each in preParts) {
                if (!each.isAttached) {
                    return false;
                }
            }

            // Check if blocking parts are attached, if underlying parts are already attached. 
            if (CheckForBlocked()) {
                return false;
            }
        }
        return true;
    }

    private bool CheckForBlocked() {
        // Check if blocking parts are attached, if underlying parts are already attached. 
        foreach (Part each in blockingParts) {
            if (each.isAttached) {
                return true;
            }
        }
        return false;
    }

    private void WreakHavoc() {
        // Debug.Log("Something bad is about to happen...");
        if (dimension == Dimension.PHYSICAL) {
            Debug.Log("Snap and Die");
            //rigidbody.AddForce(new Vector3(0f, 1000f, 0f));
            // TODO: Wait for few seconds, then dissolve...
        }
    }
}
