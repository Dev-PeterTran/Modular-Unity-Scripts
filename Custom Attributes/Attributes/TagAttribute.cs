using System;
using UnityEngine;

namespace CustomAttributes {

    /// <summary>
	/// Variable values are set to one of the possible 'tag' values.
	/// </sumary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TagAttribute : PropertyAttribute {
        public bool UseDefaultTagFieldDrawer = false;
    }
}