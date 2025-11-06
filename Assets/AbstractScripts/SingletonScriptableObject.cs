using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject
    where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance from Resources folder
                _instance = Resources.Load<T>(typeof(T).Name);

                if (_instance == null)
                {
                    Debug.LogError(
                        $"SingletonScriptableObject: Could not find instance of {typeof(T).Name} in Resources."
                    );
                }
            }
            return _instance;
        }
    }

    protected virtual void OnEnable()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
#if UNITY_EDITOR
            // In editor, we can't destroy assets with DestroyImmediate.
            // Instead, delete the duplicate asset file to enforce singleton behavior.
            string duplicatePath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string instancePath = UnityEditor.AssetDatabase.GetAssetPath(_instance);

            Debug.LogError(
                $"SingletonScriptableObject: DUPLICATE DETECTED! Only one instance of {typeof(T).Name} is allowed. "
                    + $"Keeping: {instancePath}. "
                    + $"Deleting duplicate: {duplicatePath}",
                _instance
            );

            // Delete the duplicate asset file
            if (!string.IsNullOrEmpty(duplicatePath))
            {
                UnityEditor.AssetDatabase.DeleteAsset(duplicatePath);
                UnityEditor.AssetDatabase.Refresh();
            }
#else
            // At runtime, destroy the duplicate instance
            DestroyImmediate(this);
#endif
        }
    }

    protected virtual void OnDisable()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
