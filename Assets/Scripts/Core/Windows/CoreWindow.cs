using System;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule.Animations;
using LSCore.Async;
using LSCore.Attributes;
using LSCore.ConfigModule;
using LSCore.Extensions.Time;
using SourceGenerators;

[InstanceProxy]
public partial class CoreWindow : BaseWindow<CoreWindow>
{
    [TimeSpan] public long time;
    public LSText timeText;
    private Tween timer; 
    private int pauseCount;
    public CreateSinglePrefab<UIView> freezeUIView;
    public LSButton freezeButton;
    
    protected override void Init()
    {
        base.Init();
        SetTimerText(time.ToTimeSpan());
        IUIView.Showing += PauseOnShowingOtherWindow;
    }

    private void OnDestroy()
    {
        IUIView.Showing -= PauseOnShowingOtherWindow;
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

    private void _StartTimer()
    {
        if(pauseCount > 0) return;
        timer ??= time.ToTimeSpan().Seconder(SetTimerText, false);
    }

    private void _Freeze(float duration)
    {
        freezeButton.raycastTarget = false;
        freezeUIView.Do();
        freezeUIView.obj.Manager.OnlyShow();
        var sliders = freezeUIView.obj.gameObject.GetComponentsInChildren<LSSlider>();
        var anim = new LSSliderAnim();
        anim.targets.Add(null);
        foreach (var slider in sliders)
        {
            slider.minValue = 0;
            slider.maxValue = duration;
            slider.value = duration;
            anim.FirstTarget = slider;
            anim.duration = duration;
            anim.startValue = duration;
            anim.endValue = 0;
            anim.Animate().SetEase(Ease.Linear);
        }
        Wait.Delay(duration, OnComplete);

        void OnComplete()
        {
            freezeButton.raycastTarget = true;
            freezeUIView.obj.Manager.OnlyHide();
        }
    }

    private void SetTimerText(TimeSpan remaining)
    {
        timeText.text = remaining.ToString(@"mm\:ss");
        if(remaining <= TimeSpan.Zero) LoseWindow.Show();
    }
}