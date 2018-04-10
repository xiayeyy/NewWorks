using UnityEngine;
using System.Collections.Generic;
using IMIForUnity;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class ImiExplorer : MonoBehaviour
{
    private int errorCode = 0;

    public ImiResolution.Type colorResolution = ImiResolution.Type._640x480;
    public ImiResolution.Type depthResolution = ImiResolution.Type._320x240;

    public GameObject skeletons;

    public RawImage colorView;
    public RawImage depthView;
    public GameObject cube;
    public Text label;

    private ImiResolution colorRes;
    private ImiResolution depthRes;

    public UserControlMode playerControlMode = UserControlMode.NEAREST;

    private List<GameObject> skeletonList = new List<GameObject>();
    private int skeletonCount = (int)ImiSkeleton.Index.COUNT;

    private IManager iManager = IManagerFactory.GetInstance();

    private ImiTexture2D texture2d = null;
    private ImiTexture2D texture2dDepth = null;

    // 彩色视图上显示的图像类型
    public FrameType CVFrameType = FrameType.COLOR_TEXTURE;
    public UserExtractMode CVUserExtractMode = UserExtractMode.NO_EXTRACT;

    // 深度视图上显示的图像类型
    public FrameType DVFrameType = FrameType.DEPTH_TEXTURE;
    public UserExtractMode DVUserExtractMode = UserExtractMode.NO_EXTRACT;

    private IGesture imiGestureManager = IManagerFactory.GetInstance().IEGetImiGestureInstance();

    private int mMainPlayerId = -1;

    private bool mIsInit = false;

    public static void testMainPlayerChangeListener(int newid, int oldid)
    {
        Debug.Log("## testMainPlayerChangeListener= " + newid + " " + oldid);
        Debug.Log("## IEGetMainPlayerId = " + IManagerFactory.GetInstance().IEGetMainUserId());
    }

    public static void testUpgradeDownloaded()
    {
        Debug.Log("## testUpgradeDownloaded");
        Debug.Log("## IEGetMainPlayerId = " + IManagerFactory.GetInstance().IEGetMainUserId());
    }

    public static void testDeviceUpgrading(ImiEngineWrapper.IMI_UPGRADE_PROMPT promptId, float progress)
    {
        Debug.Log("## testDeviceUpgrading= " + promptId + " " + progress);
        Debug.Log("## IEGetMainPlayerId = " + IManagerFactory.GetInstance().IEGetMainUserId());
    }

    public static void testDeviceStateChanged(string deviceUri, ImiEngineWrapper.ImiDeviceState deviceState, System.IntPtr userData)
    {
        Debug.Log("## testDeviceStateChanged= " + deviceUri + " " + deviceState);
        Debug.Log("## IEGetMainPlayerId = " + IManagerFactory.GetInstance().IEGetMainUserId());
    }

    public static void testDeviceOpened()
    {
        Debug.Log("## testDeviceOpened ");
        Debug.Log("## IEGetMainPlayerId = " + IManagerFactory.GetInstance().IEGetMainUserId());
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("skeletonCount = " + skeletonCount);

        int skeletonsNum = skeletonCount;
        for (int i = 0; i < skeletonsNum; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = skeletons.transform;
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            skeletonList.Add(sphere);
        }

        for (int i = 0; i < skeletonsNum; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sphere.transform.parent = skeletons.transform;
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            skeletonList.Add(sphere);
        }

        Debug.Log("Create Manager");

        iManager.IEInit(this);

#if UNITY_ANDROID && !UNITY_EDITOR
        if (colorResolution == ImiResolution.Type._1280x720 || 
             colorResolution == ImiResolution.Type._960x720 ||
             colorResolution == ImiResolution.Type._1920x1080)
        {
            int ret = iManager.IEEnableHardDecoder();
            if(ret == -1)
            {
                //not support
                Debug.Log("enable decoder falied, because this platform not support");
                colorResolution = ImiResolution.Type._640x480;
            }
        }
#endif

        iManager.IEAddFrameSource(CVFrameType, CVUserExtractMode);
        iManager.IEAddFrameSource(DVFrameType, DVUserExtractMode);
        iManager.IEAddFrameSource(FrameType.SKELETON, UserExtractMode.NO_EXTRACT);
        //iManager.IEAddFrameSource(FrameType.DEPTH, UserExtractMode.NO_EXTRACT);

        SetResolution(FrameType.COLOR_TEXTURE, colorResolution);
        SetResolution(FrameType.DEPTH_TEXTURE, depthResolution);
		
#if UNITY_ANDROID && !UNITY_EDITOR
        iManager.IESetDeviceStateChangeMsgHandler(this, "onDeviceStateChange");//set device state change handler.
#endif

        //iManager.IESetDeviceOpenedListener(testDeviceOpened);
        errorCode = iManager.IEOpenDevice();
  
        texture2d = new ImiTexture2D(FrameType.COLOR_TEXTURE,  colorRes.Width, colorRes.Height);
        //texture2dDepth = new ImiTexture2D(FrameType.COLOR_TEXTURE, colorRes.Width, colorRes.Height);
        texture2dDepth = new ImiTexture2D(FrameType.DEPTH_TEXTURE,  depthRes.Width, depthRes.Height);

        colorView.texture = texture2d.Texture;
        depthView.texture = texture2dDepth.Texture;

        iManager.IESetUserControlMode(playerControlMode);

        //iManager.IESetMainPlayerChangeListener(testMainPlayerChangeListener);
        //iManager.IESetDeviceUpgradeCallback(testUpgradeDownloaded, testDeviceUpgrading);

#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log("### IESetDeviceStateCallback");
        //iManager.IESetDeviceStateCallback(testDeviceStateChanged, System.IntPtr.Zero);
#endif

        initGestures();


        //iManager.IESetUserProperty(0, ImiEngineWrapper.UserProperty.IE_PROPERTY_MARK_MAIN_USER_IN_DEPTH, true);
        //iManager.IESetMainUserColor(0,255,0,255);
    }
	
	 void onDeviceStateChange(string msg)
    {
        string[] deviceAndState = msg.Split('#');
        ImiDeviceType deviceType = (ImiDeviceType)int.Parse(deviceAndState[0]);
        ImiAndroidHelper.ImiDeviceState deviceState = (ImiAndroidHelper.ImiDeviceState)int.Parse(deviceAndState[1]);
        Debug.Log("onDeviceStateChange : device type is " + deviceType + ", device state is " + deviceState);
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
                case ImiResolution.Type._960x720:
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

    void initGestures()
    {
        if (imiGestureManager != null)
        {
            imiGestureManager.AddGesture(ImiGesture.GestureType.IMI_GESTURE_RIGHT_HAND_WAVE, (gesture, player, gevent) =>
            {
                if (gevent == ImiGesture.GestureEvent.Complete)
                {
                    Debug.Log("IMI_GESTURE_RIGHT_HAND_WAVE Detected!");
                }
            });
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
    //private void showSyncTexture()
    //{
    //    if (lastColor == IntPtr.Zero)
    //    {
    //        lastColor = ImiEngineWrapper.IEReadImiFrame(CVFrameType, CVUserExtractMode);
    //        if (lastColor != IntPtr.Zero)
    //        {
    //            ImiEngineWrapper.IEImageFrame stImageFrame1 = (ImiEngineWrapper.IEImageFrame)Marshal.PtrToStructure(lastColor, typeof(ImiEngineWrapper.IEImageFrame));
    //            colorTimeStamp = stImageFrame1.timeStamp/1000;
    //            Debug.Log("[Richard] Color timeStemp = " + colorTimeStamp);
    //        }
    //    }

    //    if (lastmainColor == IntPtr.Zero)
    //    {
    //        lastmainColor = ImiEngineWrapper.IEReadImiFrame(DVFrameType, DVUserExtractMode);
    //        if (lastmainColor != IntPtr.Zero)
    //        {
    //            ImiEngineWrapper.IEImageFrame stImageFrame2 = (ImiEngineWrapper.IEImageFrame)Marshal.PtrToStructure(lastmainColor, typeof(ImiEngineWrapper.IEImageFrame));
    //            mainColorTimeStamp = stImageFrame2.timeStamp/1000;
    //            Debug.Log("[Richard] MainUser timeStemp = " + mainColorTimeStamp);
    //        }
    //    }

    //    if (lastColor!=IntPtr.Zero && lastmainColor!=IntPtr.Zero)
    //    {
    //        long timeStemp = colorTimeStamp - mainColorTimeStamp;
    //        Debug.Log("[Richard] sub timeStemp = " + timeStemp);
    //        if (timeStemp > 50)
    //        {
    //            lastmainColor = IntPtr.Zero;
    //        }
    //        else if (timeStemp < -50)
    //        {
    //            lastColor = IntPtr.Zero;
    //        }
    //        else
    //        {
    //            lastmainColor = IntPtr.Zero;
    //            lastColor = IntPtr.Zero;
    //        }
    //    }
    //}

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

    void testGrasp()
    {
//#if UNITY_ANDROID && !UNITY_EDITOR
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
                iManager.IESetUserProperty(key, ImiEngineWrapper.UserProperty.IE_PROPERTY_BOTH_HAND_GRASP, true);
                ImiEngineWrapper.UserGraspState userGS = playerInfos[key].GetUserGraspState();
                ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons();
                for (int j = 0; j < skeletonCount; j++)
                {
                    //Debug.Log("player pos is " + skeletons[j].position.ToString());
                    skeletonList[j].transform.position = skeletons[j].position;
                }

                label.text = "lc=" + userGS.leftConfidence + ", lg=" + userGS.leftGesturestate + ", rc=" + userGS.rightConfidence + ", rg=" + userGS.rightGesturestate;
            }
        }
//#endif
    }

    private void updateControlPlayerIdByNear(Dictionary<int, IPlayerInfo> playerInfos)
    {
        if (playerInfos == null || playerInfos.Count == 0)
        {
            mMainPlayerId = -1;
            return;
        }

        mMainPlayerId = -1;
        foreach (int key in playerInfos.Keys)
        {
            if (playerInfos[key].GetPlayerTracked())
            {
                if (mMainPlayerId < 0
                    || playerInfos[key].GetPlayerPosition().z < playerInfos[mMainPlayerId].GetPlayerPosition().z)
                {
                    mMainPlayerId = key;
                    iManager.IESetMainUserId(mMainPlayerId);
                }
            }
        }
    }

    void testPlayerInfos()
    {
        Dictionary<int, IPlayerInfo> playerInfos = iManager.IEUpdatePlayerInfos();

        if (playerInfos == null || playerInfos.Count <= 0)
        {
            return;
        }


        //UserControlMode controlMode = UserControlMode.CUSTOM;
        //iManager.IEGetUserControlMode(ref controlMode);
        //if (controlMode == UserControlMode.CUSTOM)
        //{
        //    updateControlPlayerIdByNear(playerInfos);
        //}

        foreach (int key in playerInfos.Keys)
        {
            // Display main player skeleton
            if (key == iManager.IEGetMainUserId())
            {
                ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons();
                for (int j = 0; j < skeletonCount; j++)
                {
                    skeletonList[j].transform.position = skeletons[j].position;
                }
                if (skeletons[(int)ImiSkeleton.Index.HIP_CENTER].position.y - playerInfos[key].GetJointInfo().fixHipPosY > 0.1f)
                {
                    label.text = "Jump";
                }
                else if (skeletons[(int)ImiSkeleton.Index.HIP_CENTER].position.y - playerInfos[key].GetJointInfo().fixHipPosY < -0.2f)
                {
                    label.text = "Crouch";
                }
                else
                {
                    label.text = "Stand";
                }
            }

            // Display sub player skeleton
            //if (key == iManager.IEGetSubPlayerId())
            //{
            //    ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons();
            //    for (int j = 0; j < skeletonCount; j++)
            //    {
            //        //Debug.Log("player pos is " + skeletons[j].position.ToString());
            //        skeletonList[j + 20].transform.position = skeletons[j].position;
            //    }
            //}
        }

        //imiGestureManager.AddGestureUser(iManager.IEGetMainUserId());
        //imiGestureManager.UpdateGestures(playerInfos, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            //if (iManager.IEGetDeviceOpenState() != ImiEngineWrapper.DeviceOpenState.DEVICE_UNAUTHORIZED)
            //{
            //    iManager.IEKillOpenDeviceThreadIfAlive();
            //}

            Application.Quit();
        }

        cube.transform.Rotate(1, 1, 1);
        if (iManager.IEGetDeviceOpenState() != ImiEngineWrapper.DeviceOpenState.WORKING)
        {
            return;
        }

        //if (!mIsInit)
        //{
        //    //iManager.IESetDeviceProperty(ImiProperty.IMI_PROPERTY_GROUND_CLEANUP, 1);
        //    //iManager.IESetGroundJudge(true, true);
        //    mIsInit = true;
        //}

        showTexture();
        testPlayerInfos();
        //testGrasp();
    }

    void OnApplicationQuit()
    {
        Debug.Log("ImiUnityLog : Enter OnApplicationQuit");
        iManager.IECloseDevice();
    }
}