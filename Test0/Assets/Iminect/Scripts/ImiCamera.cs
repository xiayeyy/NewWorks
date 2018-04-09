using UnityEngine;
using System.Collections.Generic;
using IMIForUnity;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class ImiCamera : MonoBehaviour {

    private int errorCode = 0;

    public UserControlMode playerControlMode = UserControlMode.NEAREST;

    public bool bOutPutLog = false;

    public bool bEnableOpenGLRender = true;

    public bool bMarkMainUserInDepthImage = false;

    private IManager iManager = IManagerFactory.GetInstance();

 
    // Use this for initialization
    void Start()
    {
        Debug.Log("Create Manager");

        iManager.IEInit(this);

        errorCode = iManager.IEOpenDevice();


        iManager.IESetUserControlMode(playerControlMode);


        

        iManager.IESetLogFileOutput(bOutPutLog);

        iManager.IEEnableNativeOpenGLRenderer(bEnableOpenGLRender);
        
       
        if(bMarkMainUserInDepthImage)
        {
            iManager.IESetUserProperty(0, ImiEngineWrapper.UserProperty.IE_PROPERTY_MARK_MAIN_USER_IN_DEPTH, true);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("ImiUnityLog : Enter OnApplicationQuit");
        iManager.IECloseDevice();
    }

    private static ImiCamera ins;
    public static ImiCamera Ins
    {
        get
        {
            if (ins == null)
            {
                ins = GameObject.FindObjectOfType<ImiCamera>();
            }
            return ins;
        }
    }
}