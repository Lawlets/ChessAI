using UnityEngine;
using System.Collections;
using System;

public class ExtendedBehaviour : MonoBehaviour
{
    public void Wait(float seconds, Action action)
    {
        StartCoroutine(_wait(seconds, action));
    }

    IEnumerator _wait(float time, Action callBack)
    {
        yield return new WaitForSeconds(time);
        callBack();
    }

    private float timer = 0.0f;
    public void Wait(float seconds)
    {
        while (timer < seconds)
        {
            timer += 0.1f * Time.deltaTime;
        }
        timer = 0.0f;
    }
}
