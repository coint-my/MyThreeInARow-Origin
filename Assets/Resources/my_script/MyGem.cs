using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGem : MonoBehaviour
{
    private Coroutine myMoveCoroutine;
    private Coroutine myMovePointToCellCoroutine;
    [SerializeField]
    private float mySpeedAnimate = 5;

    private bool myIsAnimate = false;

    public List<MyActiveCell> myListPoints = new List<MyActiveCell>();

    public bool MyIsAnimate { get { return myIsAnimate; } }

    public MyGemType MyGetType { get {
            return transform.childCount > 0 ? GetComponentInChildren<MyGemType>() : null; } }

    public static event System.Action MyEventEndPlayAnimationCell;

    public void MyMoveGem()
    {
        if (myMoveCoroutine != null)
            StopCoroutine(myMoveCoroutine);

        myMoveCoroutine = StartCoroutine(MyMoveGemCoroutine(0.01f));
    }

    private IEnumerator MyMoveGemCoroutine(float _timeUpdate)
    {
        myIsAnimate = true;

        Vector3 nextPosition = Vector3.zero;

        for (int ind = 0; ind < myListPoints.Count; ind++)
        {
            Vector3 startPosition = transform.position;
            nextPosition = myListPoints[ind].transform.position;
            float startTime = Time.time;
            
            while (Vector3.Distance(transform.position, nextPosition) != 0)
            {
                float distCovered = (Time.time - startTime) * mySpeedAnimate;

                transform.position = Vector3.Lerp(startPosition, nextPosition, distCovered);

                yield return new WaitForSeconds(_timeUpdate);
            }
        }

        myListPoints.Clear();

        myIsAnimate = false;
    }

    public void MyMoveToPointTheCell(MyActiveCell _targetCell)
    {
        if (myMovePointToCellCoroutine != null)
            StopCoroutine(myMovePointToCellCoroutine);

        myMovePointToCellCoroutine = StartCoroutine(MyMovingToPointTheCell(0.01f, _targetCell));
    }

    private IEnumerator MyMovingToPointTheCell(float _timeUpdate, MyActiveCell _targetCell)
    {
        myIsAnimate = true;

        Vector3 nextPosition = _targetCell.transform.position;
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, nextPosition) != 0)
        {
            float distCovered = (Time.time - startTime) * mySpeedAnimate;

            transform.position = Vector3.Lerp(transform.position, nextPosition, distCovered);

            yield return new WaitForSeconds(_timeUpdate);
        }

        myIsAnimate = false;

        transform.SetParent(_targetCell.transform, true);

        yield return new WaitForSeconds(0.01f);

        MyEventEndPlayAnimationCell?.Invoke();
    }

    private void MyResetColor()
    {
        for (int ind = 0; ind < myListPoints.Count; ind++)
        {
            myListPoints[ind].MySetColor(Color.white);
            myListPoints[ind].myIsMove = false;
        }
    }
}
