using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMIForUnity;
using UnityEngine.UI;

public class Treo : MonoBehaviour
{

    IManager iManager;
    internal Vector3[] worldPos = new Vector3[Info.LEN_MAINJOINT];

    [SerializeField]
    int width;
    [SerializeField]
    int height;

    [SerializeField]
    int bodynum;

    [SerializeField]
    GameObject[] Particles;
    private GameObject[] tx = new GameObject[10];

    public GameObject PersonDepth;
    public GameObject texiao;
    public GameObject texiao2;

    // TexPoint lhead;

    void Start()
    {
        iManager = IManagerFactory.GetInstance();
        //  width = (int)person.GetComponent<RectTransform>().sizeDelta.x;
        // height  = (int)person.GetComponent<RectTransform>().sizeDelta.y;
        ParticlesLoad();
    }


    void Update()
    {

        width = (int)PersonDepth.GetComponent<RectTransform>().sizeDelta.x;
        height = (int)PersonDepth.GetComponent<RectTransform>().sizeDelta.y;

        ShowBody();
        // ShowPart(bodynum);
        // ShowPart2();
        ShowPart0(5, 0);
        ShowPart0(8, 1);

        if (Input.GetMouseButtonDown(0))
        {

        }

        //texiao.transform.localPosition = new Vector3(lhead.x, lhead.y, 0);
    }
    public void ShowBody()
    {
        Dictionary<int, IPlayerInfo> playerInfos = iManager.IEUpdatePlayerInfos();

        foreach (int key in playerInfos.Keys)
        {

            IPlayerInfo playerInfo = playerInfos[key];
            ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons();
            worldPos[0] = skeletons[3].position;//HEAD
            worldPos[1] = skeletons[2].position;//NECK
            worldPos[2] = skeletons[1].position;//TORSO
            worldPos[3] = skeletons[4].position;//LEFT SHOULDER
            worldPos[4] = skeletons[5].position;//LEFT ELBOW
            worldPos[5] = skeletons[6].position;//LEFT HAND
            worldPos[6] = skeletons[8].position;//RIGHT SHOULDER
            worldPos[7] = skeletons[9].position;//RIGHT ELBOW
            worldPos[8] = skeletons[10].position;//RIGHT HAND
            worldPos[9] = skeletons[12].position;//LEFT HIP
            worldPos[10] = skeletons[13].position;//LEFT KNEE
            worldPos[11] = skeletons[14].position;//LEFT FOOT
            worldPos[12] = skeletons[16].position;//RIGHT HIP
            worldPos[13] = skeletons[17].position;//RIGHT KNEE
            worldPos[14] = skeletons[18].position;//RIGHT FOOT    

            //worldPos[BodyNum] = skeletons[BodyNum +1].position;//LEFT HAND
        }
    }
    public void ParticlesLoad()
    {

        tx[0] = Instantiate(Particles[0], PersonDepth.transform);
        tx[1] = Instantiate(Particles[1], PersonDepth.transform);
    }
    public void ShowPart(int BodyNum)
    {
        TexPoint bodypoint = GetTexPoint(worldPos[BodyNum]);
        AnchorChange(bodypoint, texiao);
        texiao.transform.localPosition = new Vector3(bodypoint.x - width / 2, height / 2 - bodypoint.y, 0);
    }



    public void ShowPart0(int BodyNum, int ParticleNum)
    {

        TexPoint bodypoint = GetTexPoint(worldPos[BodyNum]);
        if (!tx[ParticleNum].activeSelf)
        {
            tx[ParticleNum].SetActive(true);
        }
        AnchorChange(bodypoint, tx[ParticleNum]);
        tx[ParticleNum].transform.localPosition = new Vector3(bodypoint.x - width / 2, height / 2 - bodypoint.y, 0);
    }

    public void AnchorChange(TexPoint body, GameObject texiao)
    {
        if (body.x - width / 2 <= 0)
        {
            texiao.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        }
        if (body.x - width / 2 > 0)
        {
            texiao.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        }
        if (height / 2 - body.y >= 0)
        {
            texiao.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        }
        if (height / 2 - body.y < 0)
        {
            texiao.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        }
    }
    public static ImiEngineWrapper.ImiVector4I ConvertSkeletonPointToDepthPoint(Vector3 skeletonPosition, int width, int height)
    {
        ImiEngineWrapper.ImiVector4I depthVec = new ImiEngineWrapper.ImiVector4I();
        ImiEngineWrapper.ImiVector4 temp = new ImiEngineWrapper.ImiVector4();
        temp.x = skeletonPosition.x;
        temp.y = skeletonPosition.y;
        temp.z = skeletonPosition.z;
        temp.w = 0;
        ImiEngineWrapper.IEConvertSkeletonPointToDepthPoint(temp, width, height, ref depthVec);
        return depthVec;
    }
    TexPoint GetTexPoint(Vector3 vec3)
    {
        ImiEngineWrapper.ImiVector4I iv = ConvertSkeletonPointToDepthPoint(vec3, width, height);
        TexPoint vec2 = new TexPoint(iv.x, height - iv.y);
        return vec2;
    }

    public static class Info
    {
        public const int LEN_MAINJOINT = 15;
    }

    public struct TexPoint
    {
        public int x;
        public int y;

        public TexPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
