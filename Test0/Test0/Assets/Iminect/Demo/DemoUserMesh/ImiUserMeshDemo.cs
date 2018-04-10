using UnityEngine;
using System.Collections.Generic;
using IMIForUnity;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class ImiUserMeshDemo : MonoBehaviour
{
    private int errorCode = 0;

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool IsHardDecoderWorking = false;
#endif
    private ImiResolution.Type colorResolution = ImiResolution.Type._640x480;
    private ImiResolution.Type depthResolution = ImiResolution.Type._320x240;

    private GameObject skeletons;

    public RawImage colorView;
    public RawImage depthView;
    private GameObject cube;
    private Text label;

    private ImiResolution colorRes;
    private ImiResolution depthRes;

    public UserControlMode playerControlMode = UserControlMode.NEAREST;

    private List<GameObject> skeletonList = new List<GameObject>();
    private int skeletonCount = (int)ImiSkeleton.Index.COUNT;

    private IManager iManager = IManagerFactory.GetInstance();

    private ImiTexture2D texture2d = null;
    private ImiTexture2D texture2dDepth = null;

    // 彩色视图上显示的图像类型
    private FrameType CVFrameType = FrameType.COLOR_TEXTURE;
    private UserExtractMode CVUserExtractMode = UserExtractMode.MAIN_USER_IN_CENTER;

    // 深度视图上显示的图像类型
    private FrameType DVFrameType = FrameType.DEPTH_TEXTURE;
    private UserExtractMode DVUserExtractMode = UserExtractMode.NO_EXTRACT;

    private IGesture imiGestureManager = IManagerFactory.GetInstance().IEGetImiGestureInstance();

    private int mMainPlayerId = -1;

    private bool mIsInit = false;

    GameObject humanMeshObj;

    bool isAddCollider = false;

    // Use this for initialization
    void Start()
    {
        iManager.IEInit(this);


#if UNITY_ANDROID && !UNITY_EDITOR
        if (colorResolution == ImiResolution.Type._1280x720 || colorResolution == ImiResolution.Type._1920x1080)
        {
            //enable performance for color 720p/1080p
            int ret = iManager.IEEnableHardDecoder();
            if(ret == -1)
            {
                //not support
                Debug.Log("enable decoder falied, because this platform not support");
                IsHardDecoderWorking = false;
                colorResolution = ImiResolution.Type._640x480;
            }
            else
            {
                //enable success
                Debug.Log("enable decoder success");
                CVUserExtractMode = UserExtractMode.NO_EXTRACT;
                IsHardDecoderWorking = true;
            }
        }
#endif

        iManager.IEAddFrameSource(CVFrameType, CVUserExtractMode);
        iManager.IEAddFrameSource(DVFrameType, DVUserExtractMode);
        iManager.IEAddFrameSource(FrameType.SKELETON, UserExtractMode.NO_EXTRACT);

        iManager.IESetMeshEnable(true);
        SetResolution(FrameType.COLOR_TEXTURE, colorResolution);
        SetResolution(FrameType.DEPTH_TEXTURE, depthResolution);
		
#if UNITY_ANDROID && !UNITY_EDITOR
        iManager.IESetDeviceStateChangeMsgHandler(this, "onDeviceStateChange");//set device state change handler.
#endif
        errorCode = iManager.IEOpenDevice();
  
        texture2d = new ImiTexture2D(FrameType.COLOR_TEXTURE,  colorRes.Width, colorRes.Height);
        texture2dDepth = new ImiTexture2D(FrameType.DEPTH_TEXTURE,  depthRes.Width, depthRes.Height);

        colorView.texture = texture2d.Texture;
        depthView.texture = texture2dDepth.Texture;

        iManager.IESetUserControlMode(playerControlMode);

        iManager.IESetDeviceProperty(ImiProperty.IMI_PROPERTY_GROUND_CLEANUP,1);
        humanMeshObj = iManager.IECreateMeshGameObject("Human mesh",ImiUserMeshID.IMI_UPPER_BODY_MESH,8); 
        humanMeshObj.GetComponent<Renderer>().material = Resources.Load<Material>("DepthMask");

    }


    void SetResolution(FrameType frameType, ImiResolution.Type resType)
    {
        if(frameType == FrameType.COLOR_TEXTURE)
        {
            switch (resType)
            {
                case ImiResolution.Type._320x240:
                    colorRes.type = colorResolution;
                    break;
                case ImiResolution.Type._640x480:
                    colorRes.type = colorResolution;
                    break;
                case ImiResolution.Type._1280x720:
                    colorRes.type = colorResolution;
                    break;
                case ImiResolution.Type._1920x1080:
                    colorRes.type = colorResolution;
                    break;
                default:
                    colorRes.type = ImiResolution.Type._640x480;
                    break;
            }

            Debug.Log("############ Color Resolution w=" + colorRes.Width + " h=" + colorRes.Height);
            iManager.IESetResolution(FrameType.COLOR_TEXTURE, colorRes.Width, colorRes.Height);
        }

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

            Debug.Log("############ Depth Resolution w=" + depthRes.Width + " h=" + depthRes.Height);
            iManager.IESetResolution(FrameType.DEPTH_TEXTURE, depthRes.Width, depthRes.Height);
        }

    }


    private void showTexture()
    {
        iManager.IEUpdateTextureFrame(CVFrameType, CVUserExtractMode, ref texture2d);
        iManager.IEUpdateTextureFrame(DVFrameType, DVUserExtractMode, ref texture2dDepth);
    }

    private IntPtr lastColor = IntPtr.Zero;
    private long colorTimeStamp = 0;
    private IntPtr lastmainColor = IntPtr.Zero;
    private long mainColorTimeStamp = 0;
   
    private void showSyncTexture()
    {
        if (lastmainColor == IntPtr.Zero)
        {
            lastmainColor = ImiEngineWrapper.IEReadImiFrame(DVFrameType, DVUserExtractMode);
            if (lastmainColor != IntPtr.Zero)
            {
                ImiEngineWrapper.IEImageFrame stImageFrame2 = (ImiEngineWrapper.IEImageFrame)Marshal.PtrToStructure(lastmainColor, typeof(ImiEngineWrapper.IEImageFrame));
                mainColorTimeStamp = stImageFrame2.timeStamp / 1000;
                //Debug.Log("[Richard] MainUser timeStemp = " + mainColorTimeStamp);
            }
        }

        if (lastColor == IntPtr.Zero)
        {
            lastColor = ImiEngineWrapper.IEReadImiFrame(CVFrameType, CVUserExtractMode);
            if (lastColor != IntPtr.Zero)
            {
                ImiEngineWrapper.IEImageFrame stImageFrame1 = (ImiEngineWrapper.IEImageFrame)Marshal.PtrToStructure(lastColor, typeof(ImiEngineWrapper.IEImageFrame));
                colorTimeStamp = stImageFrame1.timeStamp / 1000;
                //Debug.Log("[Richard] Color timeStemp = " + colorTimeStamp);
            }
        }

        if (lastColor != IntPtr.Zero && lastmainColor != IntPtr.Zero)
        {
            long timeStemp = colorTimeStamp - mainColorTimeStamp;
            Debug.Log("[Richard] sub timeStemp = " + timeStemp);
            lastmainColor = IntPtr.Zero;
            lastColor = IntPtr.Zero;
        }
    }

   
    void testPlayerInfos()
    {
        Dictionary<int, IPlayerInfo> playerInfos = iManager.IEUpdatePlayerInfos();

        if (playerInfos == null || playerInfos.Count <= 0)
        {
            return;
        }

        foreach (int key in playerInfos.Keys)
        {
            // Display main player skeleton
            if (key == iManager.IEGetMainUserId())
            {
                ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons();
                iManager.IEGenerateUserMesh(key);
            }
        }
    }
    static int i = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            iManager.IEKillOpenDeviceThreadIfAlive();
            Application.Quit();
        }

        //cube.transform.Rotate(1, 1, 1);
        if (iManager.IEGetDeviceOpenState() != ImiEngineWrapper.DeviceOpenState.WORKING)
        {
            return;
        }

        showTexture();
        testPlayerInfos();

    }

    void OnApplicationQuit()
    {
        Debug.Log("ImiUnityLog : Enter OnApplicationQuit");
        iManager.IECloseDevice();
    }
}