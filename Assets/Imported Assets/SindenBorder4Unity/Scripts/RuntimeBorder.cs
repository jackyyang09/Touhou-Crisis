using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SindenUnity
{
    /// <summary>
    /// Ensure that there is only ever a single instance of this object at any given time
    /// </summary>
    public class RuntimeBorder : MonoBehaviour
    {
        public BorderProperties properties;

        /// <summary>
        /// True if the current border's width and height add up to 0, or if the color's alpha is 0
        /// Value is set by SindenBorder
        /// </summary>
        [SerializeField] public bool isEmpty = true;

        static RuntimeBorder instance;
        public static RuntimeBorder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RuntimeBorder>();
                    if (instance == null)
                    {
                        GameObject g = new GameObject("RuntimeBorder");
                        instance = g.AddComponent<RuntimeBorder>();
                    }
                }
                return instance;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}