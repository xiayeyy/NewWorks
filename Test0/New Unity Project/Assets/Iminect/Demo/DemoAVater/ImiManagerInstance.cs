using UnityEngine;
using System;
using System.Collections.Generic;
using IMIForUnity;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

public class ImiManagerInstance : MonoBehaviour
{
    public ImiResolution.Type depthResolution = ImiResolution.Type._640x480;
    public RawImage depthView;
    private ImiResolution depthRes;
    private IManager iManager = IManagerFactory.GetInstance();
    private ImiTexture2D texture2dDepth = null;
    
    // 深度视图上显示的图像类型
    private FrameType DVFrameType = FrameType.DEPTH_TEXTURE;
    private UserExtractMode DVUserExtractMode = UserExtractMode.NO_EXTRACT;
    
    void Start()
    {
        iManager.IEAddFrameSource(DVFrameType, DVUserExtractMode);
        iManager.IEAddFrameSource(FrameType.SKELETON, UserExtractMode.NO_EXTRACT);
        SetResolution(FrameType.DEPTH_TEXTURE, depthResolution);

        iManager.IEInit(this);
#if UNITY_ANDROID && !UNITY_EDITOR
        iManager.IESetDeviceStateChangeMsgHandler(this, "onDeviceStateChange");//set device state change handler.
#endif
        iManager.IEOpenDevice();
        texture2dDepth = new ImiTexture2D(FrameType.DEPTH_TEXTURE,  depthRes.Width, depthRes.Height);
    }
    
    void onDeviceStateChange(string msg)
    {
        string[] deviceAndState = msg.Split('#');
        ImiDeviceType sensorType = (ImiDeviceType)int.Parse(deviceAndState[0]);
        ImiAndroidHelper.ImiDeviceState deviceState = (ImiAndroidHelper.ImiDeviceState)int.Parse(deviceAndState[1]);

        Debug.Log("onDeviceStateChange : device type is " + sensorType + ", device state is " + deviceState);
    }

    void SetResolution(FrameType frameType, ImiResolution.Type resType)
    {   
        if (frameType == FrameType.DEPTH_TEXTURE)
        {
            switch (resType)
            {
                case ImiResolution.Type._320x240:
                    depthRes.type = depthResolution;
                    break;
                case ImiResolution.Type._640x480:
                    depthRes.type = depthResolution;
                    break;
                case ImiResolution.Type._1280x720:
                    depthRes.type = depthResolution;
                    break;
                case ImiResolution.Type._1920x1080:
                    depthRes.type = depthResolution;
                    break;
                default:
                    depthRes.type = ImiResolution.Type._640x480;
                    break;
            }

            iManager.IESetResolution(FrameType.DEPTH_TEXTURE, depthRes.Width, depthRes.Height);
        }

    }


    private void showTexture()
    {
        iManager.IEUpdateTextureFrame(DVFrameType, DVUserExtractMode, ref texture2dDepth);
        texture2dDepth.UpdateTexture();
        
        depthView.texture = texture2dDepth.Texture;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            iManager.IEKillOpenDeviceThreadIfAlive();
            Application.Quit();
        }
        
        showTexture();
        
        Dictionary<int, IPlayerInfo> playerInfos = iManager.IEUpdatePlayerInfos();

        if (playerInfos == null || playerInfos.Count <= 0)
        {
            return;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("ImiUnityLog : Enter OnApplicationQuit");

        iManager.IECloseDevice();
    }
}