using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyTypeGem { ROMB, RECTANGLE, OVAL, CIRCLE, OCTAGON, TRIANGLE, NONE }

public class MyGemType : MonoBehaviour
{
    public MyTypeGem myType = MyTypeGem.NONE;
    public string myPathPrefab;
}
