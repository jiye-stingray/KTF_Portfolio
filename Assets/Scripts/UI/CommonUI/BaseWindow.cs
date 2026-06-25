using System.Collections;
using UnityEngine;

public class BaseWindow : MonoBehaviour
{
    public Animator animator;

    public void OnClickCloseButton()
    {
        DestroyWindow();
    }

    private void DestroyWindow()
    {
        animator.SetTrigger("Close");
        StartCoroutine(CoDestroy());
    }

    private IEnumerator CoDestroy()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
    
}