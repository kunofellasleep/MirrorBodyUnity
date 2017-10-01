using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MirrorBody
{
    // ==================================== <summary> 
    // 環境設定系
    // </summary> ===================================

    public class MirrorBodyPrefs : MonoBehaviour
    {
        static string settingFileName = "MirrorbodySetting.xml";

        //-------------------
        //PrayerPrefsで使用するキー
        static string key_kinectPosX = "MB_KINECT_POS_X";
        static string key_kinectPosZ = "MB_KINECT_POS_Z";
        static string key_mirrorPosY = "MB_MIRROR_POS_Y";
        static string key_mirrorWidth = "MB_MIRROR_WIDTH";
        static string key_mirrorHeight = "MB_MIRROR_HEIGHT";
        static string key_offsetX = "MB_OFFSET_X";
        static string key_offsetY = "MB_OFFSET_Y";

        static private Dictionary<string, float> prams = new Dictionary<string, float>() {
            {key_kinectPosX, 0},
            {key_kinectPosZ, 0},
            {key_mirrorPosY, 0},
            {key_mirrorWidth, 0},
            {key_mirrorHeight, 0},
            {key_offsetX, 0},
            {key_offsetY, 0}
        };

        #region PUBLIC_FUNCTIONS

        // ==================================== <summary> 
        // kinect位置設定
        // </summary> ===================================
        public Vector3 GetKinectPos() 
        { 
            Vector3 kinectPos = new Vector3(prams[key_kinectPosX], 0f, prams[key_kinectPosZ]);
            return kinectPos;
        }

        // ==================================== <summary> 
        // 鏡位置設定
        // </summary> ===================================
        public Vector3 GetMirrorPos()
        {
            Vector3 mirrorPos = new Vector3(0f, prams[key_mirrorPosY], 0f);
            return mirrorPos;
        }

        // ==================================== <summary> 
        // 鏡サイズ設定
        // </summary> ===================================
        public float GetMirrorWidth()
        {
            float mirrorWidth = prams[key_mirrorWidth];
            return mirrorWidth;
        }
        public float GetMirrorHeight()
        {
            float mirrorHeight = prams[key_mirrorHeight];
            return mirrorHeight;
        }

        // ==================================== <summary> 
        // オフセット設定（微妙なズレ補正）
        // </summary> ===================================
        public float GetOffsetX() {
            return prams[key_offsetX];
        }
        public float GetOffsetY() {
            return prams[key_offsetY];
        }
        //これだけアプリ側からも編集できる
        public void SetOffsetX(float value) {
            prams[key_offsetX] = value;
            saveXmlSetting();
        }
        public void SetOffsetY(float value) {
            prams[key_offsetY] = value;
            saveXmlSetting();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        void Start() {
            loadXmlSetting();
        }

        void loadXmlSetting() {
            var filePath = Path.Combine(Application.streamingAssetsPath, settingFileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(filePath));
            XmlNode root = xmlDoc.FirstChild;
            List<string> keyList = new List<string>(prams.Keys);
            foreach (string key in keyList) {
                prams[key] = float.Parse(xmlDoc.GetElementsByTagName(key)[0].InnerText);
            }
        }

        void saveXmlSetting() {
            var filePath = Path.Combine(Application.streamingAssetsPath, settingFileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(filePath));
            xmlDoc.GetElementsByTagName(key_offsetX)[0].InnerText = GetOffsetX().ToString("#,0.###");
            xmlDoc.GetElementsByTagName(key_offsetY)[0].InnerText = GetOffsetY().ToString("#,0.###");
            xmlDoc.Save(filePath);
        }

        #endregion

    }
}

