using System.Collections;
using UnityEngine;

public class AutoDebugLog : MonoBehaviour
{
    private void Start() 
    {
        StartCoroutine(AutoDebugLogCoroutine());
    }

    private IEnumerator AutoDebugLogCoroutine()
    {
        while (true)
        {
            Debug.Log("This is a log message.");
            Debug.LogWarning("This is a warning message.");
            Debug.LogError("This is an error message.");
            yield return new WaitForSeconds(1f);
        }
    }
}
