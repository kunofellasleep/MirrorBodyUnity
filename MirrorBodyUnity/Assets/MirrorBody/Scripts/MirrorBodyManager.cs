using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

namespace MirrorBody
{
    // ==================================== <summary> 
    // ミラーディスプレイにおける
    // 実像視点と虚像情報の関係にまつわるいろいろな値をかえす
    // </summary> ===================================

    public class MirrorBodyManager : SingletonMonoBehaviour<MirrorBodyManager>
    {
        [SerializeField]
        private Text pramText;


        //ボーンの描画するかどうか
        [SerializeField]
        private bool isDebugMode = false;

        //ボーン管理部分
        private BodyJointsManager bodyJointManager;

        //キネクトからの取得データ
        private KinectData kinectData;
        //環境設定
        private MirrorBodyPrefs prefs;

        //現在ターゲットいるかどうか
        protected bool isExistTarget = false;
        //最後のターゲット情報
        private Kinect.Body lastTargetBody;

        // Kinectの位置
        private Vector3 kinectPos;
        protected float kinectAngle;
        // ディスプレイサイズ
        private float mirrorWidth, mirrorHeight;
        // ディスプレイ位置(中心)
        private Vector3 mirrorPos;
        // 閾値
        private float nearThreshold = 1.75f;
        private float farThreshold = 3.25f;
        private float sideThreshold = 0.75f;

        #region PUBLIC_FUNCTIONS

        // ==================================== <summary> 
        // ターゲットがいるかどうかを返す
        // </summary> ===================================
        public bool GetIsTarget()
        {
            return isExistTarget;
        }

        // ==================================== <summary> 
        // 各部位の位置を返す
        // </summary> ===================================
        public Vector3 GetJointRawPos(Kinect.JointType jointType)
        {
            if (!isExistTarget) return new Vector3(0, 0, -10);
            //body.Joints[Kinect.JointType.Head].Position.Z
            Vector3 jointPos = new Vector3();
            jointPos.x = lastTargetBody.Joints[jointType].Position.X;
            jointPos.y = lastTargetBody.Joints[jointType].Position.Y;
            jointPos.z = lastTargetBody.Joints[jointType].Position.Z;
            return jointPos;
        }

        // ==================================== <summary> 
        // 各部位の位置を返す
        // </summary> ===================================
        public Vector3 GetJointPos(Kinect.JointType jointType)
        {
            if (!isExistTarget) return new Vector3(0,0,-10);
            Vector3 jointPos = bodyJointManager.GetJointPos(jointType);
            return jointPos;
        }

        // ==================================== <summary> 
        // カメラの位置（ターゲットの実像頭位置）を返す
        // </summary> ===================================
        public Vector3 GetCameraPos()
        {
            Vector3 jointHeadPos = bodyJointManager.GetJointPos(Kinect.JointType.Head);
            //虚像位置->実像位置なので符号反転(z軸)する
            Vector3 result = new Vector3(
                jointHeadPos.x,
                jointHeadPos.y,
                -(jointHeadPos.z)
            );
            return result;
        }

        // ==================================== <summary> 
        // カメラの見る方向を返す
        // </summary> ===================================
        public Vector3 GetCameraLookPos()
        {
            return mirrorPos;
        }

        // ==================================== <summary> 
        // カメラのFOV値(鏡に移る視界の広さ)を返す
        // </summary> ===================================
        public float GetCameraFOV()
        {
            // 鏡の中の世界は近づくと見える空間が広くなります
            // unityカメラでは鏡の中の世界だけを表現するのでFOV値に変動が起きます
            Vector3 camPos = GetCameraPos();
            Vector3 mirrorTopPos = new Vector3(mirrorPos.x, mirrorPos.y + mirrorHeight / 2, mirrorPos.z);
            Vector3 mirrorBottomPos = new Vector3(mirrorPos.x, mirrorPos.y - mirrorHeight / 2, mirrorPos.z);

            float[] ba = new float[2];
            ba[0] = mirrorTopPos.z - camPos.z;
            ba[1] = mirrorTopPos.y - camPos.y;
            float[] bc = new float[2];
            bc[0] = mirrorBottomPos.z - camPos.z;
            bc[1] = mirrorBottomPos.y - camPos.y;

            float babc = ba[0] * bc[0] + ba[1] * bc[1];
            float ban = (ba[0] * ba[0]) + (ba[1] * ba[1]);
            float bcn = (bc[0] * bc[0]) + (bc[1] * bc[1]);
            float radian = Mathf.Acos(babc / (Mathf.Sqrt(ban * bcn)));
            float angle = radian * 180 / Mathf.PI;

            //エラー回避
            if (angle <= 0) angle = 10;

            return angle;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        // ==================================== <summary> 
        // 初回処理
        // </summary> ===================================
        void Awake()
        {
            //動的追加スクリプト
            kinectData = gameObject.AddComponent<KinectData>();
            prefs = gameObject.AddComponent<MirrorBodyPrefs>();
            bodyJointManager = gameObject.AddComponent<BodyJointsManager>();

            //インスペクタから設定必要なスクリプトがなかったらログ
            if (bodyJointManager == null)
                Debug.LogError("<color=red>NEED TO SET \"BodyJointManager\" FROM INSPECTOR WINDOW</color>");
        }

        // ==================================== <summary> 
        // アップデート処理
        // </summary> ===================================
        void Update()
        {
            //キー押下でボーン情報の描画の有無を変更
            if (Input.GetKeyUp(KeyCode.B))
            {
                isDebugMode = !isDebugMode;
            }


            //各パラメータに更新をかける
            updateParamaters();

            // 最も距離が近いやつを現在のターゲットにする処理
            isExistTarget = false;
            float nearestZ = 9999;

            //生のボーンデータすべて取得してくる
            Kinect.Body[] data = kinectData.GetAllBody();

            

            // 1体もいなかったらretrun
            if (data == null)
            {
                bodyJointManager.SetIsExistTarget(false);
                return;
            }
            // 各ボーンの位置確認していく
            foreach (var body in data)
            {
                if (body == null) continue;
                if (body.IsTracked)
                {
                    //しきいの外の人は無視
                    if (body.Joints[Kinect.JointType.Head].Position.Z > farThreshold ||
                        body.Joints[Kinect.JointType.Head].Position.Z < nearThreshold ||
                        body.Joints[Kinect.JointType.Head].Position.X > sideThreshold ||
                        body.Joints[Kinect.JointType.Head].Position.X < -(sideThreshold)
                        ) continue;
                    //有効なボーンが１つでもいたらターゲット存在フラグたてる
                    isExistTarget = true;
                    //現在の候補よりさらに近いやつがいたらそれを保持
                    if (body.Joints[Kinect.JointType.Head].Position.Z < nearestZ)
                    {
                        nearestZ = body.Joints[Kinect.JointType.Head].Position.Z;
                        lastTargetBody = body;

                    }
                }
            }

            //現在のターゲット情報をボーン管理にわたす
            bodyJointManager.SetTargetBody(lastTargetBody);
            bodyJointManager.SetIsExistTarget(isExistTarget);
            bodyJointManager.SetIsDisplayBodyJoints(isDebugMode);
        }

        // ==================================== <summary> 
        // パラメータの変更を反映
        // </summary> ===================================
        void updateParamaters() {

            if (isDebugMode) {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) prefs.SetOffsetX(prefs.GetOffsetX() - 0.005f);
                if (Input.GetKeyDown(KeyCode.RightArrow)) prefs.SetOffsetX(prefs.GetOffsetX() + 0.005f);
                if (Input.GetKeyDown(KeyCode.UpArrow)) prefs.SetOffsetY(prefs.GetOffsetY() + 0.005f);
                if (Input.GetKeyDown(KeyCode.DownArrow)) prefs.SetOffsetY(prefs.GetOffsetY() - 0.005f);
            }

            //kinect角度取得
            var floorPlane = kinectData.FloorClipPlane;
            var comp = Quaternion.FromToRotation(new Vector3(floorPlane.X, floorPlane.Y, floorPlane.Z), Vector3.up);
            kinectAngle = comp.eulerAngles.x;
            //kinect位置
            kinectPos = new Vector3(prefs.GetKinectPos().x + prefs.GetOffsetX(), floorPlane.W + prefs.GetOffsetY(), prefs.GetKinectPos().z);
            //mirror位置
            mirrorPos = prefs.GetMirrorPos() + new Vector3(0, prefs.GetMirrorHeight()/2, 0);
            
            //mirrorサイズ
            mirrorWidth = prefs.GetMirrorWidth();
            mirrorHeight = prefs.GetMirrorHeight();
            //Kinect位置角度わたす
            bodyJointManager.SetKinectPos(kinectPos);
            bodyJointManager.SetKinectAngle(kinectAngle);

            //GUIテキスト更新
            pramText.gameObject.SetActive(isDebugMode);
            pramText.text =
                 "ANGLE : AUTO (" + kinectAngle + ")\n" + 
                 "KINECT_POS_X : " + prefs.GetKinectPos().x + "\n" +
                 "KINECT_POS_Y : AUTO (" + floorPlane.W + ")\n" +
                 "KINECT_POS_Z : " + prefs.GetKinectPos().z + "\n" +
                 "MIRROR_POS_Y : " + prefs.GetMirrorPos().y + "\n" +
                 "MIRROR_WIDTH : " + mirrorWidth + "\n" +
                 "MIRROR_HEIGHT : " + mirrorHeight + "\n\n" +
                 "OFFSET_X : " + prefs.GetOffsetX().ToString("#,0.###") + "\n" +
                 "OFFSET_Y : " + prefs.GetOffsetY().ToString("#,0.###") + "\n\n" +
                 "IS_FLOOR : " + (floorPlane.W != 0.0  ? "TRUE" : "FALSE") + "\n" ;
        }

        // ==================================== <summary>
        //　座標回転
        // </summary> ===================================
        protected Vector3 RotateAroundPivot(Vector3 Point, Vector3 Pivot, Quaternion Angle)
        {
            return Angle * (Point - Pivot) + Pivot;
        }
        protected Vector3 RotateAroundPivot(Vector3 Point, Vector3 Pivot, Vector3 Euler)
        {
            return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
        }

        #endregion

#if UNITY_EDITOR
        // ==================================== <summary>
        //　Gizumo描画
        // </summary> ===================================

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.yellow;
            //鏡gizmo
            Gizmos.DrawWireCube(mirrorPos, new Vector3(mirrorWidth, mirrorHeight, 0.001f));

            //unityカメラ画角gizmo
            var cameraPos = GetCameraPos();
            Gizmos.DrawLine(cameraPos, new Vector3(mirrorPos.x, mirrorPos.y + mirrorHeight / 2, mirrorPos.z));
            Gizmos.DrawLine(cameraPos, new Vector3(mirrorPos.x, mirrorPos.y - mirrorHeight / 2, mirrorPos.z));
            
            Gizmos.color = Color.blue;

            //kinect gizmo
            Vector3 kinectPlateSize = new Vector3(0.245f, 0.01f, 0.06f);
            Vector3 kinectSize = new Vector3(0.245f, 0.065f, 0.06f);
            Gizmos.DrawWireCube(kinectPos, kinectPlateSize);

            Matrix4x4 cubeTransform = Matrix4x4.TRS(kinectPos, Quaternion.Euler(-kinectAngle, 0, 0), Vector3.one);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;

            Gizmos.DrawWireCube(new Vector3(0, kinectSize.y / 2 + kinectPlateSize.y, 0), kinectSize);
            Gizmos.DrawLine(new Vector3(0, kinectSize.y / 2 + kinectPlateSize.y, 0), new Vector3(0, kinectSize.y / 2 + kinectPlateSize.y, -4.5f));

            Gizmos.matrix = oldGizmosMatrix;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(GetCameraPos(), GetJointPos(Kinect.JointType.Head));

            UnityEditor.Handles.Label(mirrorPos, "MIRROR");
            UnityEditor.Handles.Label(mirrorPos + new Vector3(0, -0.1f, 0), mirrorWidth.ToString() + " x " + mirrorHeight.ToString());
            UnityEditor.Handles.Label(kinectPos, "KINECT");
            UnityEditor.Handles.Label(kinectPos + new Vector3(0, -0.1f, 0), kinectAngle.ToString() + "°");

        }
#endif

    }
}


