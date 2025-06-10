using UnityEngine;

namespace ModularUnityScripts {
    /// <summary>
    /// Represents a single axis in 3D or 2D space.<br/>
    /// Used to define orientation, rotation, or directional constraints.
    /// </summary>
    public enum Axis { X_Axis, Y_Axis, Z_Axis }

    /// <summary>
    /// Specifies the type of gravity applied to objects.<br/>
    /// <b>- Spherical:</b> Gravity pulls objects towards a central point (e.g., a planet).<br/>
    /// <b>- Directional:</b> Gravity applies in a fixed direction, such as downwards.
    /// </summary>
    public enum GravityType { Spherical, Directional }

    /// <summary>
    /// Defines the dimensional space of the game world.
    /// </summary>
    public enum GameDimension { TwoDimensional, ThreeDimensional }

    /// <summary>
    /// Specifies the type of guidance applied to moving objects or projectiles.<br/>
    /// <b>- Predictive:</b> Path is predicted based on initial parameters; no further control.<br/>
    /// <b>- Controlled:</b> Fully controlled by user input.<br/>
    /// <b>- Unguided:</b> No guidance or external control; object follows initial trajectory.<br/>
    /// <b>- Guided:</b> Follows a target or objective using automated guidance logic.
    /// </summary>
    public enum GuidanceType { Predictive, Controlled, Unguided, Guided }
}
