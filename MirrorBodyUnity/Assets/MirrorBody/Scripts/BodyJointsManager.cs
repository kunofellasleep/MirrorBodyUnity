using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;
using JointType = Windows.Kinect.JointType;

namespace MirrorBody
{
    // ==================================== <summary> 
    // ボーンデータ管理クラス
    // </summary> ===================================

    public class BodyJointsManager : MonoBehaviour
    {

        bool isExistTarget = false;
        bool isDisplayBodyJoints = false;

        //現在のボディデータ格納用
        public Kinect.Body currentBody;

        private Vector3 kinectPos;
        private float kinectAngle;

        Dictionary<Kinect.JointType, GameObject> jointObjects = new Dictionary<Kinect.JointType, GameObject>();

        #region PUBLIC_FUNCTIONS

        // ==================================== <summary> 
        // タ－ゲットの有無
        // </summary> ===================================
        public void SetIsExistTarget(bool _isExistTarget)
        {
            isExistTarget = _isExistTarget;
        }

        // ==================================== <summary> 
        // タ－ゲットの有無
        // </summary> ===================================
        public void SetIsDisplayBodyJoints(bool _isDisplayBodyJoints)
        {
            isDisplayBodyJoints = _isDisplayBodyJoints;
        }

        // ==================================== <summary> 
        // タ－ゲットのセット
        // </summary> ===================================
        public void SetTargetBody(Kinect.Body _currentBody)
        {
            currentBody = _currentBody;
        }

        // ==================================== <summary> 
        // kinect位置セット
        // </summary> ===================================
        public void SetKinectPos(Vector3 _kinectPos)
        {
            kinectPos = _kinectPos;
        }

        // ==================================== <summary> 
        // kinect位置セット
        // </summary> ===================================
        public void SetKinectAngle(float _kinectAngle)
        {
            kinectAngle = _kinectAngle;
        }

        // ==================================== <summary> 
        // 各部位の位置を返す
        // </summary> ===================================
        public Vector3 GetJointPos(Kinect.JointType _jointType)
        {
            if (currentBody == null)
                return Vector3.zero;
            //ターゲットがいる場合は指定部位の位置を取得して返す
            Kinect.Joint joint = currentBody.Joints[_jointType];
            Vector3 result = new Vector3(
                joint.Position.X + kinectPos.x,
                joint.Position.Y + kinectPos.y,
                joint.Position.Z + kinectPos.z
            );
            result = RotateAroundPivot(result, Vector3.zero, new Vector3(kinectAngle, 0, 0));
            return result;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        // ==================================== <summary> 
        // 初回処理
        // </summary> ==================================
        void Start()
        {

            for (int i = 0; i < Enum.GetNames(typeof(JointType)).Length; i++) {
                var jointType = (Windows.Kinect.JointType)Enum.ToObject(typeof(Windows.Kinect.JointType), i);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "_" + jointType.ToString().ToLower();
                go.transform.localScale = Vector3.one * 0.02f;
                go.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
                go.GetComponent<Renderer>().material.color = Color.white;
                go.transform.parent = gameObject.transform;
                jointObjects.Add(jointType, go);
            }
        }

        // ==================================== <summary> 
        // アップデート処理
        // </summary> ===================================
        void Update()
        {
            // ターゲットがいたらデータを更新してボーン描画
            if (isExistTarget)
            {
                //関節更新
                UpdateJointsPos();
            }
        }

        // ==================================== <summary> 
        // 関節点更新
        // </summary> ===================================
        private void UpdateJointsPos()
        {
            int jointSize = Enum.GetNames(typeof(JointType)).Length;
            Dictionary<Kinect.JointType, Vector3> calibrateJoints = new Dictionary<Kinect.JointType, Vector3>();
            for (int i = 0; i < jointSize; i++) {
                var jointType = (Windows.Kinect.JointType)Enum.ToObject(typeof(Windows.Kinect.JointType), i);
                calibrateJoints.Add(jointType, GetJointPos(jointType));
                jointObjects[jointType].transform.position = GetJointPos(jointType);
                jointObjects[jointType].SetActive(isDisplayBodyJoints);
            }
        }

        // ==================================== <summary> 
        //　kinect座標型をvector3に変換
        // </summary> ===================================
        private Vector3 kinectPointToVector3(Kinect.CameraSpacePoint _pos, Vector3? _offset = null)
        {
            Vector3 offset = _offset ?? Vector3.zero;
            Vector3 pos = new Vector3(
              _pos.X + offset.x,
              _pos.Y + offset.y,
              _pos.Z + offset.z
           );
            return pos;
        }

        // ==================================== <summary> 
        //　座標回転
        // </summary> ===================================
        private Vector3 RotateAroundPivot(Vector3 Point, Vector3 Pivot, Quaternion Angle)
        {
            return Angle * (Point - Pivot) + Pivot;
        }
        private Vector3 RotateAroundPivot(Vector3 Point, Vector3 Pivot, Vector3 Euler)
        {
            return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
        }
        private Vector3 CorrectOffset(Vector3 Point)
        {
            return RotateAroundPivot(Point, Vector3.zero, Quaternion.Euler(-kinectAngle,0,0));
        }

        #endregion

    }

}