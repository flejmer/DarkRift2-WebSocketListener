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
        string address;
        SerializedProperty port;
        SerializedProperty ipVersion;
        SerializedProperty isUsingSecureConnection;
        SerializedProperty autoConnect;
        SerializedProperty sniffData;

        SerializedProperty objectCacheSettings;

        void OnEnable()
        {
            client = ((WebSocketUnityClient)serializedObject.targetObject);

            address                 = client.Address.ToString();
            port                    = serializedObject.FindProperty("port");
            ipVersion               = serializedObject.FindProperty("ipVersion");
            isUsingSecureConnection = serializedObject.FindProperty("isUsingSecureConnection");
            autoConnect             = serializedObject.FindProperty("autoConnect");
            sniffData               = serializedObject.FindProperty("sniffData");

            objectCacheSettings     = serializedObject.FindProperty("objectCacheSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            address = EditorGUILayout.TextField(
                new GUIContent("Address", "The address the client will connect to."),
                address
            );
            
            try
            {
                client.Address = IPAddress.Parse(address);
                EditorUtility.SetDirty(client);
            }
            catch (FormatException)
            {
                EditorGUILayout.HelpBox("Invalid IP address.", MessageType.Error);
            }

            EditorGUILayout.PropertyField(port);

            ipVersion.enumValueIndex = EditorGUILayout.Popup(
                new GUIContent("IP Version", "The IP protocol version to connect using."),
                ipVersion.enumValueIndex,
                Array.ConvertAll(ipVersion.enumNames, i => new GUIContent(i))
            );
            
            EditorGUILayout.PropertyField(isUsingSecureConnection);
            
            EditorGUILayout.PropertyField(autoConnect);

            EditorGUILayout.PropertyField(sniffData);
            
            EditorGUILayout.PropertyField(objectCacheSettings, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
