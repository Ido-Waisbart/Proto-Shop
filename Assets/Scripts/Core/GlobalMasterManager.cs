using UnityEngine;

public class GlobalMasterManager : MonoBehaviour
{
    public static GlobalMasterManager Main { get; private set; }

    void Start()
    {
        if (Main == null)
        {
            Main = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);  // The current game object is unneeded. It's a second, redundant global master manager.
    }
}