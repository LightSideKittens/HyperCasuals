using System;
using DG.Tweening;
using LSCore;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using TouchPhase = UnityEngine.TouchPhase;

public class Dragger : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector2 touchOffset = new (0f, 2f);
    public event Action<Shape> Started;
    public event Action<Shape> Ended;

    [NonSerialized] public Vector3 shapeStartPos; 
    [NonSerialized] public Shape currentShape;
    private bool isStarted = false;
    private Tween tween;
    
    private void Update()
    {
        if (LSInput.TouchCount > 0)
        {
            LSTouch touch = LSInput.GetTouch(0);
            if(touch.IsPointerOverUI && !isDragging) return;
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    CheckTouch(touchPosition);
                    if (isDragging)
                    {
                        var pos = currentShape.transform.position;
                        pos += (Vector3) touchOffset;
                        shapeStartPos = currentShape.transform.position;
                        Started?.Invoke(currentShape);
                        foreach (var block in currentShape.blocks)
                        {
                            block.sortingOrder = 10;
                        }
                        isStarted = true;
                        tween = currentShape.transform.DOMove(pos, 0.2f);
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging && isStarted)
                    {
                        currentShape.transform.position = touchPosition + (Vector3) touchOffset + offset;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging)
                    {
                        tween?.Kill();
                        tween = null;
                        isStarted = false;
                        
                        Ended?.Invoke(currentShape);
                        isDragging = false;
                        foreach (var block in currentShape.blocks)
                        {
                            block.sortingOrder = block.defaultSortingOrder;
                        }
                    }
                    break;
            }
        }
    }

    private void CheckTouch(Vector3 touchPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition);
        if (hitCollider != null && hitCollider.TryGetComponent(out currentShape))
        {
            isDragging = true;
            offset = hitCollider.transform.position - touchPosition;
        }
    }
}