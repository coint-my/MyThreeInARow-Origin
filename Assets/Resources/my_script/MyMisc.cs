using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MyMisc
{
    public interface MyISortObjectId
    {
        public int MyGetId();
    }

    public class MySortObjectId : IComparer<MyISortObjectId>
    {
        public int Compare(MyISortObjectId _first, MyISortObjectId _second)
        {
            if (_first.MyGetId() > _second.MyGetId())
                return 1;
            else if (_first.MyGetId() < _second.MyGetId())
                return -1;

            else return 0;
        }
    }

    public static int MyStringToInt(string _name)
    {
        StringBuilder buildString = new StringBuilder();

        for (int ind = 0; ind < _name.Length; ind++)
        {
            if (_name[ind] >= '0' && _name[ind] <= '9')
                buildString.Append(_name[ind]);
        }

        if (buildString.Length == 0)
        {
            Debug.Log("not number exeption method MyStringToInt string = " + _name);
            return -1;
        }

        return int.Parse(buildString.ToString());
    }
}
