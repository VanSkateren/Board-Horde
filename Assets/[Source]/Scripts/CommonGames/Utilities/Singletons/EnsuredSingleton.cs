namespace CommonGames.Utilities
{       
    //Why don't virtual statics exist?....
    
    using UnityEngine;

    #if ODIN_INSPECTOR
    using MonoBehaviour = Sirenix.OdinInspector.SerializedMonoBehaviour; 
    #endif

    /// <typeparam name="T">Type of the Singleton</typeparam>
    public abstract class EnsuredSingleton<T> : MonoBehaviour where T : EnsuredSingleton<T>
    {
        private static readonly object obj = new object();
        
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting) { return null; }

                lock (obj)
                {
                    if (_instance != null) { return _instance; }
                    
                    GameObject singletonGameObject = new GameObject {name = $"{typeof(T)} singleton"};
                    _instance = singletonGameObject.AddComponent<T>();
                    
                    return _instance;
                }
            }
                    
            protected set => _instance = value;
        }
        
        private static bool _applicationIsQuitting = false;

        protected virtual void OnEnable()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = (T)this;
            }
        }
        
        private void OnDestroy()
        {
            _applicationIsQuitting = true;
        }
    }
}