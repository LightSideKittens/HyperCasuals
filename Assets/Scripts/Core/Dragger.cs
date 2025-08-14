using System;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    public Feel.SoundAndHaptic pickFeel;
    public Feel.SoundAndHaptic releaseFeel;
    
    private bool isDragging = false;
    private Vector3 offset;
    public event Action<Shape> Started;
    public event Action<Shape> Ended;
    
    [NonSerialized] public Spawner currentSpawner;
    
    private Tween tween;
    public Shape currentShape => currentSpawner.currentShape;

    private void Update()
    {
        if (LSInput.TouchCount > 0)
        {
            LSTouch touch = LSInput.GetTouch(0);
            if(touch.IsPointerOverUI && !isDragging) return;
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0;
            var needMove = (touch.phase == TouchPhase.Moved && isDragging) || (tween?.active ?? false);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    CheckTouch(touchPosition);
                    if (isDragging)
                    {
                        pickFeel.Do();
                        Started?.Invoke(currentShape);
                        foreach (var block in currentShape.blocks)
                        {
                            block.sortingOrder = 200;
                        }
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging)
                    {
                        releaseFeel.Do();
                        tween?.Complete();
                        tween = null;
                        
                        Ended?.Invoke(currentShape);
                        isDragging = false;
                        foreach (var block in currentShape.blocks)
                        {
                            block.sortingOrder = block.defaultSortingOrder;
                        }
                    }
                    break;
            }
            
            if (needMove)
            {
                currentShape.transform.position = touchPosition + offset;
            }
        }
    }

    private void CheckTouch(Vector3 touchPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(touchPosition);
        if (hitCollider != null && hitCollider.TryGetComponent(out currentSpawner))
        {
            if(!currentSpawner.currentShape) return;
            isDragging = true;
            offset = hitCollider.transform.position - touchPosition;
        }
    }
}