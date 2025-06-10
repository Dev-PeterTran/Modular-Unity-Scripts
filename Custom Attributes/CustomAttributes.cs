using System;
using UnityEngine;

namespace CustomAttributes {
    public interface CustomAttributes { }

    public class ValidatorAttribute : PropertyAttribute, CustomAttributes { }

    public class DrawerAttribute : PropertyAttribute, CustomAttributes { }
}