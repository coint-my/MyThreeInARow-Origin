using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCell : MonoBehaviour, MyMisc.MyISortObjectId
{
    private int myId;

    public MyActiveCell MyGetActiveCell { get { return transform.childCount > 0 ?
                transform.GetComponentInChildren<MyActiveCell>() : null; } }

    public int MyGetId()
    {
        myId = MyMisc.MyStringToInt(name);
        return myId;
    }
}
