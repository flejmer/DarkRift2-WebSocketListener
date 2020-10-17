using UnityEngine;
using UnityEditor;
using System;
using System.Net;

namespace DarkRift.Client.Unity
{
    [CustomEditor(typeof(WebSocketUnityClient))]
    [CanEditMultipleObjects]
    public class WebSocketUnityClientEditor : Editor
    {
        WebSocketUnityClient client;
        SerializedProperty address;
        SerializedProperty port;
        SerializedProperty isUsingSecureConnection;
        SerializedProperty autoConnect;
        SerializedProperty sniffData;

        SerializedProperty objectCacheSettings;

        void OnEnable()
        {
            client = ((WebSocketUnityClient)serializedObject.targetObject);

            address                 = serializedObject.FindProperty("address");
            port                    = serializedObject.FindProperty("port");
            isUsingSecureConnection = serializedObject.FindProperty("isUsingSecureConnection");
            autoConnect             = serializedObject.FindProperty("autoConnect");
            sniffData               = serializedObject.FindProperty("sniffData");

            objectCacheSettings     = serializedObject.FindProperty("objectCacheSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(address);

            EditorGUILayout.PropertyField(port);

            EditorGUILayout.PropertyField(isUsingSecureConnection);
            
            EditorGUILayout.PropertyField(autoConnect);

            EditorGUILayout.PropertyField(sniffData);
            
            EditorGUILayout.PropertyField(objectCacheSettings, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
