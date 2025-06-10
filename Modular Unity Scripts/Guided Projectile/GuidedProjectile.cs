using UnityEngine;
using CustomAttributes;
using ModularUnityScripts;

/// <summary>
/// <para>
/// Controls the movement and targeting behavior of a guided projectile in a Unity scene.<br/>
/// Supports multiple guidance types including guided, controlled, unguided, and predictive,<br/>
/// and works in both 2D and 3D environments.
/// </para>
/// 
/// <para>
/// <b>Key Features:</b><br/>
/// <b>- GuidanceType:</b> Determines how the projectile moves toward or responds to a target.<br/>
/// <b>- GameDimension:</b> Enables behavior in either 2D (Rigidbody2D) or 3D (Rigidbody) space.<br/>
/// <b>- Targeting:</b> Supports using a Transform (object) or a Vector3 (position) as the target.<br/>
/// <b>- Movement:</b> Adjusts speed and turn rate using customizable parameters.<br/>
/// <b>- Input Handling:</b> Accepts directional input for controlled guidance mode.<br/>
/// <b>- Predictive Mode:</b> Calculates interception point based on target velocity and distance.
/// </para>
/// 
/// <para>
/// <b>Usage:</b><br/>
/// Inherit this class and attach the script to a GameObject with a Rigidbody (2D or 3D) and configure<br/>
/// the serialized fields in the Inspector or via code.
/// </para>
/// </summary>

[DisallowMultipleComponent]
public class GuidedProjectile : MonoBehaviour {
    #region Guided Projectile Variables
    [Header("Guided Projectile Properties")][Space(5)]

    [Tooltip("Type of guidance behavior used by the projectile (e.g., guided, controlled, unguided, predictive).")]
    [SerializeField] GuidanceType guidanceType;
    [Tooltip("Specifies whether the projectile operates in 2D or 3D space.")]
    [SerializeField] GameDimension gameDimension;

    [Space(5)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("Speed at which the projectile travels.")]
    [SerializeField][MinValue(0)] float projectileSpeed = 1;
    [Tooltip("Speed at which the projectile can turn toward its target direction.")]
    [SerializeField][MinValue(0)] float projectileTurnSpeed = 1;

    [Space(5)][LineDivider(1, LineColors.Gray)][Space(5)]

    [Tooltip("Direct projectile by using a position vector instead of an object's transform.")]
    [SerializeField] bool useTargetVector = false;

    [Space(15)][LineDivider(3, LineColors.Black)]

    [Header("Guided Projectile Debug Properties")][Space(5)]

    [Tooltip("Target object the projectile may track or predict based on guidance type.")]
    [SerializeField][ReadOnly] Transform targetObject;
    [Space(10)]
    [Tooltip("Input vector used for directional control in controlled guidance mode.")]
    [SerializeField][ReadOnly] Vector2 InputVector;
    [Space(10)]
    [Tooltip("Target vector the projectile may track or predict based on guidance type.")]
    [SerializeField][ReadOnly] Vector3 targetVector;

    private void OnValidate() {
        if (guidanceType == GuidanceType.Predictive) {
            useTargetVector = false;
        }
    }
    #endregion

    #region Getters & Setters
    public GuidanceType GuidanceType {
        get => guidanceType;
    }
    public GameDimension GameDimension {
        get => gameDimension;
    }
    public float ProjectileSpeed {
        get => projectileSpeed;
        set => projectileSpeed = Mathf.Max(0f, value); // Enforce non-negative values
    }
    public float ProjectileTurnSpeed {
        get => projectileTurnSpeed;
        set => projectileTurnSpeed = Mathf.Max(0f, value); // Enforce non-negative values
    }
    public bool UseTargetVector {
        get => useTargetVector;
        set => useTargetVector = value;
    }
    public Vector3 TargetVector {
        get => targetVector;
        set => targetVector = value;
    }
    public Transform TargetObject {
        get => targetObject;
        set => targetObject = value;
    }
    #endregion

    #region Script Functionality
    /// <summary>
    /// Sets the player input used for controlled guidance.
    /// </summary>
    /// <param name="PlayerInput">Directional input (e.g., from joystick or keyboard).</param>
    protected virtual void ProjectileInput(Vector2 PlayerInput) {
        InputVector = PlayerInput;
    }

    /// <summary>
    /// Calculates and applies projectile rotation and movement 
    /// based on the current guidance type and game dimension.
    /// </summary>
    protected virtual void ProjectileGuidanceDirection() {
        Vector3 ProjectileDirection = Vector3.zero;
        Vector3 TargetPosition = useTargetVector ? targetVector : targetObject.position;
        switch (guidanceType) {
            case GuidanceType.Predictive:
                if (targetObject != null) {
                    // Estimate future position of the target using velocity and time to intercept
                    float TimeToReach = (transform.position - TargetPosition).magnitude / projectileSpeed;
                    Vector3 TargetObjectVelocity = (gameDimension == GameDimension.TwoDimensional) ? targetObject.GetComponent<Rigidbody2D>().linearVelocity : targetObject.GetComponent<Rigidbody>().linearVelocity;
                    Vector3 PredictedPosition = (Vector3)TargetPosition + TargetObjectVelocity * TimeToReach;

                    ProjectileDirection = (gameDimension == GameDimension.TwoDimensional) ? (transform.position - PredictedPosition) : (PredictedPosition - transform.position);
                }
                break;
            case GuidanceType.Controlled:
                // Use external input to determine direction in 2D or 3D
                if (gameDimension == GameDimension.TwoDimensional) {
                    if (InputVector.sqrMagnitude > 0.001f) {
                        ProjectileDirection = new Vector3(InputVector.x, InputVector.y, 0f);
                    }
                    else {
                        return;
                    }
                }
                else {
                    // Convert input vector to a rotation and apply it to the forward vector
                    Quaternion InputRotation = Quaternion.Euler(InputVector.y, InputVector.x, 0f);
                    ProjectileDirection = InputRotation * transform.forward;
                }
                break;
            case GuidanceType.Unguided:
                // Maintain current direction without altering rotation
                if (gameDimension == GameDimension.TwoDimensional) {
                    ProjectileMovement2D(transform.eulerAngles.z);
                }
                else {
                    ProjectileMovement3D(transform.forward);
                }
                return;
            case GuidanceType.Guided:
                // Directly steer toward the target's current position
                ProjectileDirection = (gameDimension == GameDimension.TwoDimensional) ? (transform.position - TargetPosition) : (TargetPosition - transform.position);
                break;
        }

        // Apply rotation and movement based on the game dimension
        switch (gameDimension) {
            case GameDimension.TwoDimensional:
                // Calculate angle to face the direction vector
                float TargetAngle = Mathf.Atan2(ProjectileDirection.normalized.y, ProjectileDirection.normalized.x) * Mathf.Rad2Deg + 90f;
                float ProjectileRotationAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, TargetAngle, projectileTurnSpeed * Time.deltaTime);

                ProjectileMovement2D(ProjectileRotationAngle);
                break;
            case GameDimension.ThreeDimensional:
                // Gradually rotate toward the target direction
                Vector3 ProjectileRotation = Vector3.RotateTowards(transform.forward, ProjectileDirection.normalized, projectileTurnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);

                ProjectileMovement3D(ProjectileRotation);
                break;
        }
    }

    /// <summary>
    /// Applies rotation and forward movement to a 2D projectile.
    /// </summary>
    /// <param name="ProjectileRotation">Z-axis rotation angle in degrees.</param>
    private void ProjectileMovement2D(float ProjectileRotation) {
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D ProjectileBody)) {
            // Apply the new rotation
            transform.rotation = Quaternion.Euler(0f, 0f, ProjectileRotation);

            ProjectileBody.linearVelocity = transform.up * projectileSpeed;
        }
    }
    /// <summary>
    /// Applies rotation and forward movement to a 3D projectile.
    /// </summary>
    /// <param name="ProjectileRotation">Direction vector to rotate toward.</param>
    private void ProjectileMovement3D(Vector3 ProjectileRotation) {
        if (TryGetComponent<Rigidbody>(out Rigidbody ProjectileBody)) {
            // Apply the rotation
            transform.rotation = Quaternion.LookRotation(ProjectileRotation.normalized);

            ProjectileBody.linearVelocity = transform.forward * projectileSpeed;
        }
    }
    #endregion
}