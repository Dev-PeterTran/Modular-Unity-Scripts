using UnityEngine;
using CustomAttributes;
using ModularUnityScripts;

/// <summary>
/// <para>
/// Simulates customizable gravity behavior for game objects in both 2D and 3D Unity environments.<br/>
/// Supports both directional gravity (e.g., downward like Earth's gravity) and spherical gravity (e.g., radial pull toward a planet-like center).<br/>
/// Designed for modular interaction with gravity-aware components (e.g., GravityBody).
/// </para>
/// 
/// <para>
/// <b>Key Features:</b><br/>
/// <b>- GravityType:</b> Allows switching between directional and spherical gravity.<br/>
/// <b>- GameDimension:</b> Compatible with both 2D and 3D physics systems using Rigidbody2D or Rigidbody.<br/>
/// <b>- Gravity Fall-Off:</b> Optionally weakens gravitational force based on distance using inverse-square law.<br/>
/// <b>- Bidirectional Mode:</b> Directional gravity can act in both positive and negative axis directions.<br/>
/// <b>- Range Control:</b> Supports unlimited or localized gravity influence via colliders.<br/>
/// <b>- Interval Checking:</b> Optimizes performance by controlling how often objects are scanned and affected by gravity.<br/>
/// <b>- Automatic Collider Assignment:</b> Automatically adds and configures required trigger colliders based on gravity settings.
/// </para>
/// 
/// <para>
/// <b>Usage:</b><br/>
/// Attach this script to any GameObject that should act as a gravity source (e.g., a planet or gravity field generator).<br/>
/// Configure gravity behavior in the Inspector and optionally link it to objects containing a GravityBody component.<br/>
/// Automatically manages object registration through trigger detection or periodic scanning.
/// </para>
/// </summary>

[DisallowMultipleComponent]
public class GravityManipulator : MonoBehaviour {
    #region Gravity Manipulator Variables
    [Header("Gravity Manipulator Properties")][Space(5)]

    [Tooltip("The type of gravity to apply.\n\n• Directional: Gravity pulls objects in a specific direction (like Earth's gravity).\n• Circular: Gravity pulls objects toward a point (like a planet).")]
    [SerializeField] GravityType gravityType;
    [Tooltip("The dimension type of the game world.\n\n• TwoD: Uses Rigidbody2D and 2D physics.\n• ThreeD: Uses Rigidbody and 3D physics.")]
    [SerializeField] GameDimension gameDimension;

    [Space(10)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("The axis along which directional gravity will be applied (e.g., X, Y, or Z).")]
    [SerializeField][ConditionalEnumHide("gravityType", (int)GravityType.Directional)] Axis axis;
    [Tooltip("If enabled, gravity will be applied in both positive and negative directions along the selected axis.")]
    [SerializeField][ConditionalEnumHide("gravityType", (int)GravityType.Directional)] bool bidirectionalGravity = false;

    [Space(10)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("Enables gravitational fall-off based on distance.\n\nWhen enabled, objects farther from the gravity source experience weaker gravitational pull, using an inverse-square distance formula.")]
    [SerializeField] bool enableGravityFallOff = false;
    [Tooltip("The strength of the gravitational pull.\n\nUse negative values to attract objects (default is -9.81).")]
    [SerializeField][NegativeValue][MaxValue(0)] float gravitationalForce = 9.81f;
    [Tooltip("How quickly the object rotates to align with the gravity direction.\n\nHigher values result in faster alignment.")]
    [SerializeField][AbsoluteValue][MinValue(0)] float rotationalCorrectionSpeed = 50;

    [Space(10)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("If enabled, gravity affects all objects regardless of distance.\n\nIf disabled, gravity only applies within a defined area or radius.")]
    [SerializeField] bool unlimitedRange = true;
    [Tooltip("Time interval (in seconds) between gravity checks when Unlimited Range is enabled.\n\nUseful for performance optimization in systems where constant updates aren't required.")]
    [SerializeField][ShowIf("unlimitedRange", true)][MinValue(0)] float gravityCheckInterval = 0;

    [Space(10)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("Defines the maximum radius around the gravity source where gravity is applied (used for Circular gravity).\n\nOnly visible if gravity type is Circular and unlimited range is disabled.")]
    [SerializeField][ConditionalEnumHide("gravityType", (int)GravityType.Spherical)][HideIf("unlimitedRange", true)][MinValue(0)] float gravitationalRadius = 1;
    [Tooltip("Defines the area size in which directional gravity is applied.\n\nOnly visible when gravity type is Directional and Unlimited Range is disabled.")]
    [SerializeField][ConditionalEnumHide("gravityType", (int)GravityType.Directional)][HideIf("unlimitedRange", true)] Vector3 gravitationalArea = Vector3.one;

    [Space(15)][LineDivider(3, LineColors.Black)]

    [Header("Gravity Manipulator Debug Properties")][Space(5)]

    [Tooltip("The actual gravitational force applied to objects.\n\nThis value is internally calculated and may vary if gravity fall-off is enabled.")]
    [SerializeField][ReadOnly] float GravityForce = 0;
    [Tooltip("Internal timer used to control how often gravity checks are performed.\n\nThis value decreases over time and resets based on the Gravity Check Interval.")]
    [SerializeField][ReadOnly] float CheckInterval = 0;
    #endregion

    #region Getters & Setters
    public GravityType GravityType {
        get => gravityType;
    }
    public GameDimension GameDimension {
        get => gameDimension;
    }
    public Axis Axis {
        get => axis;
        set => axis = value;
    }
    public bool BidirectionalGravity {
        get => bidirectionalGravity;
        set => bidirectionalGravity = value;
    }
    public bool EnableGravityFallOff {
        get => enableGravityFallOff;
        set => enableGravityFallOff = value;
    }
    public float GravitationalForce {
        get => gravitationalForce;
        set => gravitationalForce = value;
    }
    public float RotationalCorrectionSpeed {
        get => rotationalCorrectionSpeed;
        set => rotationalCorrectionSpeed = value;
    }
    public bool UnlimitedRange {
        get => unlimitedRange;
    }
    public float GravityCheckInterval {
        get => gravityCheckInterval;
        set => gravityCheckInterval = value;
    }
    public float GravitationalRadius {
        get => gravitationalRadius;
        set {
            gravitationalRadius = value;
            switch (gameDimension) {
                case GameDimension.TwoDimensional:
                    gameObject.GetComponent<CircleCollider2D>().radius = gravitationalRadius;
                    break;
                case GameDimension.ThreeDimensional:
                    gameObject.GetComponent<SphereCollider>().radius = gravitationalRadius;
                    break;
            }
        }
    }
    public Vector3 GravitationalArea {
        get => gravitationalArea;
        set {
            gravitationalArea = value;
            switch (gameDimension) {
                case GameDimension.TwoDimensional:
                    gameObject.GetComponent<BoxCollider2D>().size = gravitationalArea;
                    break;
                case GameDimension.ThreeDimensional:
                    gameObject.GetComponent<BoxCollider>().size = gravitationalArea;
                    break;
            }
        }
    }
    #endregion

    #region Script Functionality
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        GravityForce = gravitationalForce;
        CheckInterval = gravityCheckInterval;

        switch (gameDimension) {
            case GameDimension.TwoDimensional:
                switch (gravityType) {
                    case GravityType.Spherical:
                        if (gameObject.GetComponent<Collider2D>() == null) {
                            gameObject.AddComponent<CircleCollider2D>();
                        }
                        gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
                        gameObject.GetComponent<CircleCollider2D>().radius = gravitationalRadius;
                        break;
                    case GravityType.Directional:
                        if (gameObject.GetComponent<Collider2D>() == null) {
                            gameObject.AddComponent<BoxCollider2D>();
                        }
                        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
                        gameObject.GetComponent<BoxCollider2D>().size = gravitationalArea;
                        break;
                }
                break;
            case GameDimension.ThreeDimensional:
                switch (gravityType) {
                    case GravityType.Spherical:
                        if (gameObject.GetComponent<Collider>() == null) {
                            gameObject.AddComponent<SphereCollider>();
                        }
                        gameObject.GetComponent<SphereCollider>().isTrigger = true;
                        gameObject.GetComponent<SphereCollider>().radius = gravitationalRadius;
                        break;
                    case GravityType.Directional:
                        if (gameObject.GetComponent<Collider>() == null) {
                            gameObject.AddComponent<BoxCollider>();
                        }
                        gameObject.GetComponent<BoxCollider>().isTrigger = true;
                        gameObject.GetComponent<BoxCollider>().size = gravitationalArea;
                        break;
                }
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        if (unlimitedRange && gravityCheckInterval > 0) {
            CheckInterval -= Time.deltaTime;
            if (CheckInterval <= 0) {
                IntervalCheckForGravityBodies();
                CheckInterval = gravityCheckInterval;
            }
        }
    }

    /// <summary>
    /// Applies gravity to a given object based on the configured gravity type (spherical or directional)
    /// </summary>
    /// <param name="Obj"></param>
    public virtual void ApplyGravity(Transform Obj) {
        if (enableGravityFallOff) {
            float Distance = Vector3.Distance(transform.position, Obj.position);

            float Attenuation = 1f / Mathf.Clamp((Distance * Distance), 1f, Mathf.Infinity);
            GravityForce *= Attenuation;
        }

        switch (gravityType) {
            case GravityType.Spherical:
                SphericalGravity(Obj);
                break;
            case GravityType.Directional:
                DirectionalGravity(Obj);
                break;
        }
    }

    /// <summary>
    /// Handles spherical gravity, where gravity pulls objects toward the gravity source's position
    /// </summary>
    /// <param name="Obj"></param>
    private void SphericalGravity(Transform Obj) {
        // Current 'up' direction of the object
        Vector3 ObjectUpDirection = Obj.up;
        // Direction pointing from gravity source to object
        Vector3 GravityUpDirection = (Obj.position - transform.position).normalized;
        // Rotation to align object 'up' with gravity direction
        Quaternion TargetRotation = Quaternion.FromToRotation(ObjectUpDirection, GravityUpDirection) * Obj.rotation;

        switch (gameDimension) {
            case GameDimension.TwoDimensional:
                // Adjust rotation only on the Z-axis for 2D
                TargetRotation = Quaternion.Euler(0, 0, TargetRotation.z);
                // Smoothly interpolate object's rotation to align with gravity direction
                Obj.rotation = Quaternion.Lerp(Obj.rotation, TargetRotation, rotationalCorrectionSpeed * Time.deltaTime);

                // Apply force toward gravity source
                Obj.GetComponent<Rigidbody2D>().AddForce(GravityUpDirection * GravityForce);
                break;
            case GameDimension.ThreeDimensional:
                // Smoothly interpolate object's rotation to align with gravity direction
                Obj.rotation = Quaternion.Slerp(Obj.rotation, TargetRotation, rotationalCorrectionSpeed * Time.deltaTime);

                // Apply force toward gravity source
                Obj.GetComponent<Rigidbody>().AddForce(GravityUpDirection * GravityForce);
                break;
        }
    }
    /// <summary>
    /// Applies directional gravity to the specified object, aligning its up direction with the gravity vector determined by the selected axis and configuration.
    /// </summary>
    /// <param name="Obj"></param>
    private void DirectionalGravity(Transform Obj) {
        // Current 'up' direction of the object
        Vector3 ObjectUpDirection = Obj.up;
        // Initialize the gravity direction vector
        Vector3 GravityUpDirection = Vector3.zero;

        // Determine the direction of gravity based on the selected axis
        if (bidirectionalGravity) {
            var direction = Obj.position - transform.position;
            switch (axis) {
                case Axis.X_Axis: GravityUpDirection = new Vector3(direction.x, 0, 0).normalized; break;
                case Axis.Y_Axis: GravityUpDirection = new Vector3(0, direction.y, 0).normalized; break;
                case Axis.Z_Axis: GravityUpDirection = new Vector3(0, 0, direction.z).normalized; break;
            }
        }
        else {
            switch (axis) {
                case Axis.X_Axis: GravityUpDirection = Vector3.right; break;
                case Axis.Y_Axis: GravityUpDirection = Vector3.up; break;
                case Axis.Z_Axis: GravityUpDirection = Vector3.forward; break;
            }
        }

        // Rotation to align object 'up' with gravity direction
        Quaternion TargetRotation = Quaternion.FromToRotation(ObjectUpDirection, GravityUpDirection) * Obj.rotation;
        // Smoothly interpolate object's rotation to align with gravity direction
        Obj.rotation = Quaternion.Slerp(Obj.rotation, TargetRotation, rotationalCorrectionSpeed * Time.deltaTime);

        // Apply force toward gravity source
        switch (gameDimension) {
            case GameDimension.TwoDimensional:
                Obj.GetComponent<Rigidbody2D>().AddForce(GravityUpDirection * GravityForce);
                break;
            case GameDimension.ThreeDimensional:
                Obj.GetComponent<Rigidbody>().AddForce(GravityUpDirection * GravityForce);
                break;
        }
    }

    /// <summary>
    /// Periodically checks for gravity-affected bodies and adds this manipulator to them
    /// </summary>
    protected virtual void IntervalCheckForGravityBodies() {
        // Exit early if range is limited or the check interval is active
        if (!unlimitedRange || CheckInterval > 0) {
            return;
        }

        // Find a gravity body in the scene
        var ObjBody = FindAnyObjectByType<GravityBody>();
        // Register this manipulator with the gravity body
        if (ObjBody != null) {
            ObjBody.AddGravityManipulator(this);
        }

        // Reset the interval timer
        CheckInterval = gravityCheckInterval;
    }

    private void OnDestroy() {
        // Find a gravity body in the scene
        var ObjBody = FindAnyObjectByType<GravityBody>();
        // Remove this manipulator with the gravity body
        if (ObjBody != null) {
            ObjBody.RemoveGravityManipulator(this);
        }
    }
    #endregion

    #region Trigger Collision Detection
    private void OnTriggerEnter(Collider Obj) {
        if (Obj.TryGetComponent(out GravityBody body) && !unlimitedRange) {
            body.AddGravityManipulator(this);
        }
    }
    private void OnTriggerExit(Collider Obj) {
        if (!unlimitedRange) {
            Obj.GetComponent<GravityBody>().RemoveGravityManipulator(this);
        }
    }
    private void OnTriggerEnter2D(Collider2D Obj) {
        if (Obj.TryGetComponent(out GravityBody body) && !unlimitedRange) {
            body.AddGravityManipulator(this);
        }
    }
    private void OnTriggerExit2D(Collider2D Obj) {
        if (!unlimitedRange) {
            Obj.GetComponent<GravityBody>().RemoveGravityManipulator(this);
        }
    }
    #endregion
}