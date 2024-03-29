//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StoppableFloatFilter))]
    public class StoppableFloatFilterEditor : Editor
    {
        SerializedProperty _responseCurve;
        SerializedProperty _interpolator;
        SerializedProperty _amplitude;
        SerializedProperty _bias;
        SerializedProperty _startAtStart;
        SerializedProperty _outputEvent;

        void OnEnable()
        {
            _responseCurve = serializedObject.FindProperty("_responseCurve");
            _interpolator = serializedObject.FindProperty("_interpolator");
            _amplitude = serializedObject.FindProperty("_amplitude");
            _bias = serializedObject.FindProperty("_bias");
            _startAtStart = serializedObject.FindProperty("_startAtStart");
            _outputEvent = serializedObject.FindProperty("_outputEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_responseCurve);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_amplitude);
            EditorGUILayout.PropertyField(_bias);
            EditorGUILayout.PropertyField(_startAtStart);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Output = Response(Input) * Amplitude + Bias\n" +
                "Output value will be interpolated with the setting below.",
                MessageType.None, true
            );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_interpolator);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_outputEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
