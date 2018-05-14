using UnityEngine;

using System.Collections;

public class weituo2 : MonoBehaviour

{
    //声明一个委托类型，它的实例引用一个方法

    internal delegate void MyDelegate(int num);

    MyDelegate myDelegate;

    void Start()
    {

        //委托类型MyDelegate的实例myDelegate引用的方法

        //是PrintNum

        myDelegate += PrintNum;

        myDelegate(10);

        //委托类型MyDelegate的实例myDelegate引用的方法

        //DoubleNum       

        myDelegate += DoubleNum;

        myDelegate(50);

    }


    void PrintNum(int num)
    {
        Debug.Log("Print Num: " + num);
    }


    void DoubleNum(int num)
    {
        Debug.Log("Double Num: " + num * 2);
    }

}
