using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    [SerializeField] int maxPage;
    int currentPage;
    Vector3 targetPos;
    [SerializeField] Vector3 pageStep;
    [SerializeField] RectTransform stagePageRect;

    [SerializeField] float tweenTime;
    [SerializeField] LeanTweenType tweenType;

    private void Awake()
    {
        currentPage = 1;
        targetPos = stagePageRect.localPosition;
    }
    public void Next()
    {
        if(currentPage < maxPage)
        {
            currentPage++;
            targetPos += pageStep;
            MovePage();
        }
    }

    public void Previous()
    {
        Debug.Log("previous button clicked");
        if (currentPage > 1)
        {
            currentPage--;
            targetPos -= pageStep;
            MovePage();
        }
    }

    void MovePage()
    {
        stagePageRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);
    }

}
