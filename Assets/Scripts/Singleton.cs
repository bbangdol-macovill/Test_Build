using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T singleton = null;
    public static T Instance
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType(typeof(T)) as T;
            }

            if (singleton == null)
            {
                GameObject obj = new GameObject(typeof(T).ToString());
                singleton = obj.AddComponent(typeof(T)) as T;
            }

            DontDestroyOnLoad(singleton);

            return singleton;
        }
    }

    public static bool IsInstanceExists
    {
        get
        {
            return (singleton != null);
        }
    }

    public virtual void OnDestroy()
    {
        singleton = null;
    }

    protected virtual void Awake()
    {
        singleton = Instance;
    }
}

public class SingletonDontDestroyed<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T singleton = null;
    public static T Instance
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType(typeof(T)) as T;
            }

            if (singleton == null)
            {
                GameObject obj = new GameObject(typeof(T).ToString());
                singleton = obj.AddComponent(typeof(T)) as T;
            }

            DontDestroyOnLoad(singleton);

            return singleton;
        }
    }

    public static bool IsInstanceExists
    {
        get
        {
            return (singleton != null);
        }
    }

    public virtual void OnDestroy()
    {
        singleton = null;
    }

    protected virtual void Awake()
    {
        singleton = Instance;
    }
}

public class StaticUI<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T singleton = null;
    public static T Instance
    {
        get
        {
            return singleton;
        }
    }
    protected virtual void Awake()
    {
        if (null != singleton)
        {
            GameObject.Destroy(singleton.gameObject);
            singleton = null;
        }

        singleton = gameObject.GetComponent<T>();
        DontDestroyOnLoad(this);
    }

    protected virtual void OnDestroy()
    {
        singleton = null;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

public class SingletonGroup
{
    static private GameObject m_group = null;
    static private string m_name { get { return "SingletonGroup"; } }

    static public GameObject gameObject
    {
        get
        {
            if (null == m_group)
            {
                m_group = new GameObject(m_name);
                m_group.isStatic = true;
                GameObject.DontDestroyOnLoad(m_group);
            }

            return m_group;
        }
    }

    static public Transform transform
    {
        get
        {
            return gameObject.transform;
        }
    }

    static public void DestroyImmediate()
    {
        GameObject.DestroyImmediate(m_group);
        m_group = null;
    }
}