
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ==================================== <summary> 
// カメラコントローラー
// </summary> ===================================   
namespace MirrorBody
{
	public class MirrorBodyCamera : MonoBehaviour
    {

        [SerializeField]
        private MirrorBodyManager mirrorBodyManager;

        private Camera cam;

        // ==================================== <summary> 
        // 初回設定
        // </summary> ===================================
        protected void Start()
        {
            cam = gameObject.GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }
            //インスペクタから設定必要なスクリプトがなかったらログ
            if (mirrorBodyManager == null)
                Debug.LogError("<color=red>NEED TO SET \"MirrorBodyManager\" FROM INSPECTOR WINDOW</color>");
        }

        protected void Update()
        {
            bool isTarget = mirrorBodyManager.GetIsTarget();
            
            //ターゲットが存在している
            if (isTarget)
            {
                //カメラ位置更新
                cam.transform.position = mirrorBodyManager.GetCameraPos();
                //カメラの方向更新
                cam.transform.LookAt(mirrorBodyManager.GetCameraLookPos());
                //カメラの視野更新
                cam.fieldOfView = mirrorBodyManager.GetCameraFOV();
            }
        }
    }
}