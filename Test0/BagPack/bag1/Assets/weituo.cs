using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class MyDelegate
{

    public delegate void LogDelegate(string log);   //定义 委托名为LogDelegate,带一个string参数的 委托类型  

    public static LogDelegate LogEvent;             //声明委托对象,委托实例为LogEvent    

    public static void OnLogEvent(string log)       //可以直接 MyDelegate.LogEvent("")调用委托，这么写方便管理，还可以扩展这个方法;  
    {
        if (LogEvent != null )
        {
            LogEvent(log);
        }
    }

}
public class weituo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MyDelegate.LogEvent += MyLog;
        MyDelegate.LogEvent += MyLog2;

        MyDelegate.OnLogEvent("给你们这些小函数发回调消息了啊！");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    void MyLog(string log)
    {
        Debug.Log("这种委托方法真是好用的不得了！我收到你的消息了：" + log);
    }

    void MyLog2(string log)
    {
        Debug.Log("可以实现消息触发回调，好方便！我也收到了:" + log);
    }
}
