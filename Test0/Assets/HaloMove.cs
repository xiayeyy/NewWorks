using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HaloMove : MonoBehaviour
{
    public Text t1;
    public Transform box1;
    public Transform box2;
    public Image im1;
    public Slider sl1;

    void Start()
    {
        // StartCoroutine(Halomove1());
        Vector3 vt3 = box1.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // dotweentest();

            //cameradt();
            uitest();
            print(1);
        }
        //  follow();
        if (Input.GetKeyDown(KeyCode.D))
        {
            //dazhiji();
            dtqueuq();
        }
    }


    IEnumerator Halomove1()
    {
        this.GetComponent<DOTweenAnimation>().DOPlayForward();
        yield return new WaitForSeconds(this.GetComponent<DOTweenAnimation>().duration);
        StartCoroutine(Halomove2());
    }
    IEnumerator Halomove2()
    {
        this.GetComponent<DOTweenAnimation>().DOPlayBackwards();
        yield return new WaitForSeconds(this.GetComponent<DOTweenAnimation>().duration);
        StartCoroutine(Halomove1());
    }
    public void dotweentest()
    {
        //transform.DOMoveX(1, 5);
        transform.DOLocalMoveX(150, 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    public void dazhiji()
    {
        string str = "主播在微博上晒出一张健身照 网友评论都炸了!";
        //t1.DOText(str, 5);
        //t1.DOText(str, 5).SetRelative () ;  //在原有之后插值

        t1.DOText("<color='blue '>测试字符串今天天气好</color>", 5, true, ScrambleMode.All).SetRelative();  // bool 父文本是否有效，ScrambleMode 以随机英文

        //t1.DOColor (Color.yellow, 5);
        t1.DOBlendableColor(Color.yellow, 2).SetLoops(1, LoopType.Yoyo);  //颜色
    }
    public void cameradt()  //屏幕震动
    {
        //transform.DOShakePosition(1, new Vector3(1, 1, 0));
        Camera.main.transform.DOShakePosition(2f, new Vector3(2, 2, 3));
        //transform.DOShakeScale(2, new Vector3(3, 3, 3));
    }
    public void follow()
    {
        box2.transform.DOMove(box1.position, 2).SetAutoKill(false);
    }
    public void uitest()
    {
        im1.DOFade(0, 1).SetLoops(-1, LoopType.Yoyo); // 图片alpha值

        im1.DOFillAmount(0, 1).SetLoops(-1, LoopType.Yoyo);//图片360度

        sl1.DOValue(1, 1).SetLoops(-1, LoopType.Yoyo); //slider 
    }
    public void dtqueuq()   // dt动画队列  只执行一次？？
    {
        Sequence dtqueue = DOTween.Sequence();
        dtqueue.Append(im1.DOFillAmount(0, 1).SetLoops(-1, LoopType.Yoyo));

        dtqueue.AppendInterval(2f);

        dtqueue.Append(sl1.DOValue(1, 1).SetLoops(-1, LoopType.Yoyo));

        dtqueue.OnComplete(() => {   Debug.Log("队列完成！");   });  //lambda 表达式
    }
    public void asdasd()
    { }
}
