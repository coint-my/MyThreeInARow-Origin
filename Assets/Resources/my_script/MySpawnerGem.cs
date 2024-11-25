using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySpawnerGem : MonoBehaviour
{
    private MyGem myPrefabGem;
    private MyGemType[] myListTypeGems;

    public bool MyIsHaveGem { get { return GetComponentInChildren<MyGem>(); } }

    private void Awake()
    {
        myListTypeGems = new MyGemType[6];
        myListTypeGems[0] = Resources.Load<MyGemType>("my_prefab/my_prefab_romb");
        myListTypeGems[1] = Resources.Load<MyGemType>("my_prefab/my_prefab_rectangle");
        myListTypeGems[2] = Resources.Load<MyGemType>("my_prefab/my_prefab_oval");
        myListTypeGems[3] = Resources.Load<MyGemType>("my_prefab/my_prefab_circle");
        myListTypeGems[4] = Resources.Load<MyGemType>("my_prefab/my_prefab_octagon");
        myListTypeGems[5] = Resources.Load<MyGemType>("my_prefab/my_prefab_triangle");

        myPrefabGem = Resources.Load<MyGem>("my_prefab/my_prefab_gem");
    }

    public void MySpawnGem()
    {
        if (transform.childCount == 0)
        {
            MyGem gem = Instantiate(myPrefabGem);
            MyGemType typeGem = myListTypeGems[Random.Range(0, myListTypeGems.Length - 1)];
            MyGemType objGemType = Instantiate(typeGem);//17
            //string strCutStart = UnityEditor.AssetDatabase.GetAssetPath(typeGem).Substring(17);
            //string strCutEnd = strCutStart.Remove((strCutStart.Length) - 7);
            //objGemType.myPathPrefab = strCutEnd;
            objGemType.transform.SetParent(gem.transform, false);
            gem.transform.SetParent(transform, false);
        }
    }
}
