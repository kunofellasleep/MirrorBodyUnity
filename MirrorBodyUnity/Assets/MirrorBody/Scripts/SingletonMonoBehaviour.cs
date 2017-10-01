using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorBody {

    /// <summary>
    /// シングルトン生成用のクラス
    /// </summary>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {

        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {

                    instance = (T)FindObjectOfType(typeof(T));
                    if (instance == null)
                    {
                        Debug.LogWarning("SingletonMonoBehaviour:: Error : " + typeof(T) + " is nothing");
                    }
                }
                return instance;
            }
        }

        protected void Awaike()
        {
            CheckInstance();
        }

        protected bool CheckInstance()
        {
            if (instance == null)
            {
                instance = (T)this;
                return true;
            }
            else if (Instance == this)
            {
                return true;
            }

            Destroy(this);
            return false;
        }

    }
}