using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDrawLines : MonoBehaviour
{
    private float myLineAnimation;

    private Coroutine myCoroutineAnimation;

    public Material myMaterialLine;

    public void MyInitialize(MyActiveCell[] _arrTarget)
    {
        int count = _arrTarget.Length;
        int index = 0;
        myLineAnimation = -1;

        if (myCoroutineAnimation == null)
            myCoroutineAnimation = StartCoroutine(MyCoroutineStartEffectLine(0.05f));
        else
        {
            StopCoroutine(myCoroutineAnimation);
            myCoroutineAnimation = StartCoroutine(MyCoroutineStartEffectLine(0.05f));
        }
        
        for (int ind = 0; ind < count; ind++)
        {
            LineRenderer line = new GameObject("line" + index++).AddComponent<LineRenderer>();
            line.transform.position = new Vector3(0, 0, -5);
            line.material = myMaterialLine;
            line.useWorldSpace = false;
            line.startWidth = 30;
            line.endWidth = 30;
            line.transform.SetParent(transform);

            Vector3[] pos = new Vector3[2];
            pos[0] = GetComponentInParent<MyCell>().transform.position;
            pos[1] = _arrTarget[ind].transform.position;

            line.SetPositions(pos);
        }
    }

    private IEnumerator MyCoroutineStartEffectLine(float _time)
    {
        while(myLineAnimation < 0.78f)
        {
            myLineAnimation += 0.2f;

            myMaterialLine.SetFloat("_my_position", myLineAnimation);

            yield return new WaitForSeconds(_time);
        }
    }
}
