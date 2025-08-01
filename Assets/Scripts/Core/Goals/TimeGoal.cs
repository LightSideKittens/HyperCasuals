using System;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.Attributes;
using LSCore.Extensions.Time;
using UnityEngine;

public class TimeGoal : MonoBehaviour
{
    [TimeSpan(options = TimeAttribute.Options.Minute | TimeAttribute.Options.Second)] 
    public long time;
    
    public LSText timeText;
    private Tween timer; 
    private int pauseCount;
    
    protected void Awake()
    {
        SetTimerText(time.ToTimeSpan());
        IUIView.Showing += PauseOnShowingOtherWindow;
        FieldManager.DragStarted += StartTimer;
    }

    private void OnDestroy()
    {
        IUIView.Showing -= PauseOnShowingOtherWindow;
        FieldManager.DragStarted -= StartTimer;
    }

    private void PauseOnShowingOtherWindow(IUIView view)
    {
        if (view.GetType() != typeof(CoreWindow))
        {
            timer.Pause();
            pauseCount++;
            view.Manager.Hiding += OnHide;

            void OnHide()
            {
                view.Manager.Hiding -= OnHide;
                pauseCount--;
                if(pauseCount == 0) timer.Play();
            }
        }
    }

    private void StartTimer()
    {
        if(pauseCount > 0) return;
        timer ??= time.ToTimeSpan().Seconder(SetTimerText, false);
    }

    private void SetTimerText(TimeSpan remaining)
    {
        timeText.text = remaining.ToString(@"mm\:ss");
        if(remaining <= TimeSpan.Zero) LoseWindow.Show();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsPlaying || !timeText) return;
        timeText.text = time.ToTimeSpan().ToString(@"mm\:ss");
    }
#endif
}