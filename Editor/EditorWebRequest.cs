using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Originer.Editor
{
    public class EditorWebRequest
    {
        private UnityWebRequest www;
        private Action<UnityWebRequest> onComplete;

        //POST
        public EditorWebRequest(string url, string postData, Action<UnityWebRequest> onComplete, bool isJson = true)
        {
            this.onComplete = onComplete;
            www = UnityWebRequest.Post(url, postData);
            var uH = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
            www.uploadHandler= uH;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SendWebRequest();
            EditorApplication.update += EditorUpdate;

            Debug.Log($"Request POST start\nurl: {www.url}\ndata: {postData}");
        }

        //GET
        public EditorWebRequest(string url, Action<UnityWebRequest> onComplete)
        {
            this.onComplete = onComplete;
            www = UnityWebRequest.Get(url);
            www.SendWebRequest();
            EditorApplication.update += EditorUpdate;
        }
    
        void EditorUpdate()
        {
            if (!www.isDone)
            {
                return;
            }

            if(www.isNetworkError | www.isHttpError)
            {
                Debug.LogError($"Request error\nurl: {www.url}\nerror: {www.error}");
            }

            Debug.Log($"Request completed\nurl: {www.url}\nresponse: {www.downloadHandler.text}");
            
            onComplete?.Invoke(www);
            
            EditorApplication.update -= EditorUpdate;
        }
    }
}