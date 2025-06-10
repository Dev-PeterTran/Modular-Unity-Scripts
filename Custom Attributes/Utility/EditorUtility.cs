using UnityEngine;

namespace CustomAttributes {
    /// <summary>
    /// Defines comparison logic for conditional attributes.
    /// </summary>
    public enum ComparisonType {
        Equals,
        NotEqual,
        GreaterThan,
        SmallerThan,
        SmallerOrEqual,
        GreaterOrEqual
    }

    /// <summary>
    /// Controls how a property should be visually disabled in the Inspector.
    /// </summary>
    public enum DisablingType {
        ReadOnly,
        DontDraw
    }

    /// <summary>
    /// Represents supported line colors for custom Inspector UI.
    /// </summary>
    public enum LineColors {
        Orange,
        Yellow,
        Indigo,
        Violet,
        Clear,
        White,
        Black,
        Green,
        Blue,
        Gray,
        Pink,
        Red
    }

    // Color properties
    public static class LineColorExtensions {
        public static Color GetColor(this LineColors color) {
            switch (color) {
                case LineColors.Orange: return new Color32(255, 128, 0, 255);
                case LineColors.Yellow: return new Color32(255, 211, 0, 255);
                case LineColors.Indigo: return new Color32(75, 0, 130, 255);
                case LineColors.Violet: return new Color32(128, 0, 255, 255);
                case LineColors.Clear: return new Color32(0, 0, 0, 0);
                case LineColors.White: return new Color32(255, 255, 255, 255);
                case LineColors.Black: return new Color32(0, 0, 0, 255);
                case LineColors.Green: return new Color32(0, 255, 0, 255);
                case LineColors.Blue: return new Color32(0, 0, 255, 255);
                case LineColors.Gray: return new Color32(128, 128, 128, 255);
                case LineColors.Pink: return new Color32(255, 152, 203, 255);
                case LineColors.Red: return new Color32(255, 0, 0, 255);
                default: return new Color32(0, 0, 0, 255);
            }
        }
    }
}