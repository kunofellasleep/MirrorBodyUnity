using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace MirrorBody
{
    // ==================================== <summary> 
    // KinectSDKからの取得データ受け渡しクラス
    // </summary> ===================================
    public class KinectData : MonoBehaviour
    {

        //kinect
        private KinectSensor _Sensor;
        //ボーンデータ取得クラス
        private BodyFrameReader _BodyReader;
        //データ取得クラス
        private DepthFrameReader _DepthReader;
        private ushort[] _DepthDataRaw;
        private CameraSpacePoint[] _DepthData;

        //各種データ
        private Body[] _BodyData = null;

        private CoordinateMapper _Mapper;

        #region PUBLIC_FUNCTIONS

        // ==================================== <summary> 
        // 床データ
        // </summary> ===================================
        public Windows.Kinect.Vector4 FloorClipPlane {
            get;
            private set;
        }

        // ==================================== <summary> 
        // 深度データ配列を返す
        // </summary> ===================================
        public CameraSpacePoint[] GetDepthData()
        {
            return _DepthData;
        }
        // ==================================== <summary> 
        // 深度情報を返す
        // </summary> ===================================
        public FrameDescription GetDepthFrameDesc()
        {
            return _Sensor.DepthFrameSource.FrameDescription;
        }

        // ==================================== <summary> 
        // すべてのボーンデータを返す
        // </summary> ===================================
        public Body[] GetAllBody()
        {
            return _BodyData;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        // ==================================== <summary> 
        // 初回処理
        // </summary> ===================================
        void Start()
        {
            // Kinect セットアップ
            _Sensor = KinectSensor.GetDefault();
            if (_Sensor != null)
            {
                //ボーン
                _BodyReader = _Sensor.BodyFrameSource.OpenReader();
                ////Depth
                _DepthReader = _Sensor.DepthFrameSource.OpenReader();

                var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
                _DepthDataRaw = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
                _DepthData = new CameraSpacePoint[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

                _Mapper = _Sensor.CoordinateMapper;

                if (!_Sensor.IsOpen)
                {
                    _Sensor.Open();
                }
            }
        }

        // ==================================== <summary> 
        // アップデート処理
        // </summary> ===================================
        void Update()
        {
            // ------------------
            // ボーンデータ更新
            if (_BodyReader != null)
            {
                var frame = _BodyReader.AcquireLatestFrame();
                if (frame != null)
                {
                    if (_BodyData == null)
                    {
                        _BodyData = new Body[_Sensor.BodyFrameSource.BodyCount];
                    }

                    frame.GetAndRefreshBodyData(_BodyData);

                    // FloorClipPlaneを取得する
                    FloorClipPlane = frame.FloorClipPlane;

                    frame.Dispose();
                    frame = null;
                }
            }

            // ------------------
            // Depth
            if (_DepthReader != null)
            {
                var frame = _DepthReader.AcquireLatestFrame();
                if (frame != null)
                {
                    frame.CopyFrameDataToArray(_DepthDataRaw);

                    _Mapper.MapDepthFrameToCameraSpace(_DepthDataRaw, _DepthData);

                    frame.Dispose();
                    frame = null;
                }
            }

        }

        // ==================================== <summary> 
        // アプリ終了時
        // </summary> ===================================
        void OnApplicationQuit()
        {
            if (_BodyReader != null)
            {
                _BodyReader.Dispose();
                _BodyReader = null;
            }

            if (_DepthReader != null)
            {
                _DepthReader.Dispose();
                _DepthReader = null;
            }

            if (_Sensor != null)
            {
                if (_Sensor.IsOpen)
                {
                    _Sensor.Close();
                }
                _Sensor = null;
            }
        }

        #endregion

    }
}