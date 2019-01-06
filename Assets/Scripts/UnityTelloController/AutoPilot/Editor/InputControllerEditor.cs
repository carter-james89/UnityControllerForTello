using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityControllerForTello
{
    [CustomEditor(typeof(InputController))]
    public class InputControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InputController inputController = (InputController)target;

            base.DrawDefaultInspector();

            //if(inputController.inputType == InputController.InputType.Keyboard)
            //{

            //}
        }
    } 
}
