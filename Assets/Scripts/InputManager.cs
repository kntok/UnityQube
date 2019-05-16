using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace PronamaDice
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;

        public static InputManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = (InputManager) FindObjectOfType(typeof(InputManager));

                    if (null == _instance)
                    {
                        Debug.Log("* InputManager Instance ERROR!");
                    }
                }

                return _instance;
            }
        }

        private PlayerController pc;

        private void Awake()
        {
            pc = GameObject.FindObjectOfType<PlayerController>();

            GameObject[] obj = GameObject.FindGameObjectsWithTag("InputManager");

            if (1 < obj.Length)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Update()
        {
            // 移動
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("Up" + " move.");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("Down" + " move.");
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Left" + " move.");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("Right" + " move.");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Jump.");
            }

            // ポーズメニュー
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Pause Menu.");
            }
        }
    }
}