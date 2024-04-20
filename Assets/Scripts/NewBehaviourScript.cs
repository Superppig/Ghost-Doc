using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyTimer;

public class NewBehaviourScript : MonoBehaviour
{
    private TimerOnly timer;
    // Start is called before the first frame update
    void Start()
    {
        timer.Initialize(1f);
        timer.AfterCompelete += Do;
        timer.Paused = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        Do(default);
    }

    private void Do(float _)
    {

    }
}
