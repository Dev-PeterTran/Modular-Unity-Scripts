using UnityEngine;
using CustomAttributes;
using System.Collections.Generic;

/// <summary>
/// <para>
/// Represents an object that can be influenced by one or more custom gravity sources within a Unity scene.<br/>
/// Designed to work in conjunction with <see cref="GravityManipulator"/> components that simulate directional or spherical gravity fields.
/// </para>
///
/// <para>
/// <b>Usage:</b><br/>
/// Attach this script to any GameObject you want to be affected by custom gravity sources.<br/>
/// Ensure the object has either a <see cref="Rigidbody"/> (for 3D) or <see cref="Rigidbody2D"/> (for 2D).<br/>
/// This component will automatically disable Unity’s default gravity and respond to any <see cref="GravityManipulator"/> in range.
/// </para>
/// </summary>

[DisallowMultipleComponent]
public class GravityBody : MonoBehaviour {
    #region Gravity Body Variables
    [Header("Gravity Body Properties")]
    [Tooltip("Enables or disables gravity for this object.\n\nWhen disabled, the object will not be affected by any gravity manipulators.")]
    [SerializeField] bool enableGravity = true;
    [Tooltip("A read-only list of gravity manipulators currently affecting this object.\n\nThis is automatically managed at runtime and should not be modified manually.")]
    [SerializeField][ReadOnly] List<GravityManipulator> gravityManipulator = new List<GravityManipulator>();
    #endregion

    #region Script Functions
    /// <summary>
	/// Initializes the gravity-related settings for this object by disabling Unity's built-in gravity.
	/// 
	/// This method checks whether the object has a Rigidbody or Rigidbody2D component and disables the default gravity accordingly.
	/// This ensures that only custom gravity, applied through gravity manipulators, will affect the object.
	/// </summary>
	protected virtual void InitializeGravityBody() {
        if (GetComponent<Rigidbody>() != null) {
            GetComponent<Rigidbody>().useGravity = false;
        }
        else if (GetComponent<Rigidbody2D>() != null) {
            GetComponent<Rigidbody2D>().gravityScale = 0;
        }
    }
    /// <summary>
    /// Updates the gravitational influence on this object by applying forces from all active gravity manipulators.
	/// 
	/// This function is called to simulate custom gravity behavior. It iterates through the list of 
	/// gravity manipulators currently affecting this object and calls their ApplyGravity method, 
	/// passing in this object's transform.
	/// 
	/// Only works if gravity is currently enabled for this object.
    /// </summary>
	protected virtual void UpdateGravity() {
        if (enableGravity) {
            foreach (GravityManipulator Grav in gravityManipulator) {
                Grav.ApplyGravity(transform);
            }
        }
    }
    /// <summary>
    /// Adds a gravity manipulator to the list if it's not already present. This allows the object to be influenced by that gravity source.
    /// </summary>
    /// <param name="Manipulator"></param>
    public virtual void AddGravityManipulator(GravityManipulator Manipulator) {
        if (!gravityManipulator.Contains(Manipulator)) {
            gravityManipulator.Add(Manipulator);
        }
    }
    /// <summary>
    /// Removes a gravity manipulator from the list if it exists. This stops the object from being influenced by that gravity source.
    /// </summary>
    /// <param name="Manipulator"></param>
    public virtual void RemoveGravityManipulator(GravityManipulator Manipulator) {
        if (gravityManipulator.Contains(Manipulator)) {
            gravityManipulator.Remove(Manipulator);
        }
    }

    // Whether gravity is currently enabled for this object
    public bool EnableGravity {
        get => enableGravity;
        set => enableGravity = value;
    }
    #endregion
}
