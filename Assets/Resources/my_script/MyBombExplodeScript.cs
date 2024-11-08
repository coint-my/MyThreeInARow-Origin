using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBombExplodeScript : MonoBehaviour
{
    public AnimationCurve myAnimationCurve;

    public Material myMaterialBombExplode;

    public AudioClip myAudioClip;

    [Range(0f, 2.5f)]
    public float mySpeed;

    public void MyStartAnimation(float _timeUpdate)
    {
        MyPlaySound.MyPlayClip(myAudioClip);

        StartCoroutine(MyStartCoroutine(_timeUpdate));
    }

    private IEnumerator MyStartCoroutine(float _timeUpdate)
    {
        float progress = -0.5f;

        while (true)
        {
            progress += mySpeed * Time.deltaTime;
            
            myMaterialBombExplode.SetFloat("_directionFloat", progress);

            //print("pregress = " + progress);
            yield return new WaitForSeconds(_timeUpdate);
        }
    }
}
