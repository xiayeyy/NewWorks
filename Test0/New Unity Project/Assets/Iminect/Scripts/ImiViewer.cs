using UnityEngine;
using System.Collections;
using IMIForUnity;
using UnityEngine.UI;

public class ImiViewer : MonoBehaviour {


    public ImiResolution.Type Resolution = ImiResolution.Type._640x480;
    public FrameType FrameType = FrameType.COLOR_TEXTURE;
    public UserExtractMode UserExtractMode = UserExtractMode.NO_EXTRACT;

    private RawImage image;
    private IManager iManager = IManagerFactory.GetInstance();

    private ImiTexture2D texture2d = null;
    private ImiResolution colorRes;
    private ImiResolution depthRes;
    private void Start()
    {
        iManager.IEInit(this);

#if UNITY_ANDROID && !UNITY_EDITOR
        if ((FrameType == FrameType.COLOR_TEXTURE) && (Resolution == ImiResolution.Type._1280x720 || 
             Resolution == ImiResolution.Type._960x720 ||
             Resolution == ImiResolution.Type._1920x1080))
        {
            int ret = iManager.IEEnableHardDecoder();
            if(ret == -1)
            {
                //not support
                Debug.Log("enable decoder falied, because this platform not support");
                Resolution = ImiResolution.Type._640x480;
            }
        }
#endif
        if (image == null)
        {
            image = GetComponent<RawImage>();
        }
        iManager.IEAddFrameSource(FrameType, UserExtractMode);

        SetResolution(FrameType, Resolution);

    }
        void SetResolution(FrameType frameType, ImiResolution.Type resType)
        {
            if (frameType == FrameType.COLOR_TEXTURE)
            {
                switch (resType)
                {
                    case ImiResolution.Type._320x240:
                        colorRes.type = Resolution;
                        break;
                    case ImiResolution.Type._640x480:
                        colorRes.type = Resolution;
                        break;
                    case ImiResolution.Type._960x720:
                        colorRes.type = Resolution;
                        break;
                    case ImiResolution.Type._1280x720:
                        colorRes.type = Resolution;
                        break;
                    case ImiResolution.Type._1920x1080:
                        colorRes.type = Resolution;
                        break;
                    default:
                        colorRes.type = ImiResolution.Type._640x480;
                        break;
                }

                Debug.Log("############ Color Resolution w=" + colorRes.Width + " h=" + colorRes.Height);
                iManager.IESetResolution(FrameType.COLOR_TEXTURE, colorRes.Width, colorRes.Height);
            texture2d = new ImiTexture2D(FrameType, colorRes.Width, colorRes.Height);
            image.texture = texture2d.Texture;
        }

            if (frameType == FrameType.DEPTH_TEXTURE)
            {
                switch (resType)
                {
                    case ImiResolution.Type._320x240:
                        depthRes.type = Resolution;
                        break;
                    case ImiResolution.Type._640x480:
                        depthRes.type = Resolution;
                        break;
                    case ImiResolution.Type._1280x720:
                        depthRes.type = Resolution;
                        break;
                    case ImiResolution.Type._1920x1080:
                        depthRes.type = Resolution;
                        break;
                    default:
                        depthRes.type = ImiResolution.Type._640x480;
                        break;
                }

                Debug.Log("############ Depth Resolution w=" + depthRes.Width + " h=" + depthRes.Height);
                iManager.IESetResolution(FrameType.DEPTH_TEXTURE, depthRes.Width, depthRes.Height);
            texture2d = new ImiTexture2D(FrameType, depthRes.Width, depthRes.Height);
            image.texture = texture2d.Texture;
        }
        }
    

    private void Update()
    {
        if (iManager.IEGetDeviceOpenState() != ImiEngineWrapper.DeviceOpenState.WORKING)
        {
            return;
        }

        if (ImiCamera.Ins)
        {        
            iManager.IEUpdateTextureFrame(FrameType,UserExtractMode,ref texture2d);           
        }
      
    }
}

