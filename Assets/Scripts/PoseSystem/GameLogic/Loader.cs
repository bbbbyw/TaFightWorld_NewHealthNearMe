#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public class Loader : MonoBehaviour
{
    void Start()
    {
        try
        {
            using (var javaLangSystem = new AndroidJavaClass("java.lang.System"))
            {
                javaLangSystem.CallStatic("loadLibrary", "mediapipe_jni");
                Debug.Log("✅ Loaded mediapipe_jni via System.loadLibrary");
            }
        }
        catch (AndroidJavaException ex)
        {
            Debug.LogError("❌ Failed to load mediapipe_jni manually: " + ex.Message);
        }
    }
}
#endif
