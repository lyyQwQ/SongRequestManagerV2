using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace SongRequestManagerV2.Utils
{

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 在场景中查找是否已经存在 CoroutineRunner
                    _instance = FindObjectOfType<CoroutineRunner>();
                    if (_instance == null)
                    {
                        // 如果不存在，则创建一个新的 GameObject 并附加 CoroutineRunner
                        GameObject obj = new GameObject("CoroutineRunner");
                        _instance = obj.AddComponent<CoroutineRunner>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }

        // 确保只有一个实例存在
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}