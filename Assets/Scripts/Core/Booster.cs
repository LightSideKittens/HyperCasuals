using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.Async;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[Serializable]
public abstract class Booster : DoIt
{
    public static Action<Block[,], Block[,]> Used;
    [Id(typeof(CurrencyIdGroup))] public Id id;
    public GameObject tutorialPointer;
    public AnimSequencer tutorialAnim;
    public static bool isTutorial;

    public override void Do()
    {
        tutorialPointer.SetActive(isTutorial);
        if (isTutorial)
        {
            tutorialAnim.Animate();
        }
    }

    protected virtual void OnUsed()
    {
        Funds.ForceSpend(id, 1);
        Analytic.LogEvent("booster_used", ("id", id.ToString()));
        if (isTutorial)
        {
            tutorialAnim.Kill();
            tutorialPointer.SetActive(false);
        }
    }
}

[Serializable]
public abstract class BaseFieldClickBooster : Booster
{
    public LSButton button;
    protected Vector2Int index;
    public Vector2Int tutorialGridIndex;
    
    public override void Do()
    {
        base.Do();
        if (isTutorial)
        {
            tutorialPointer.transform.position = FieldManager.ToPos(tutorialGridIndex);
        }
        button.Submitted -= OnSubmitted;
        button.Submitted += OnSubmitted;
    }
    
    private void OnSubmitted()
    {
        LSTouch touch = LSInput.GetTouch(0);
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        index = FieldManager.ToIndex(touchPosition);
        if (FieldManager.Grid.HasIndex(index))
        {
            OnClicked();
        }
    }

    protected abstract void OnClicked();
}

[Serializable]
public abstract class BaseSpecialBlockBooster : BaseFieldClickBooster
{
    public abstract Block Prefab { get; }

    protected override void OnClicked()
    {
        if(!Prefab) return;
        OnUsed();
    }

    protected override void OnUsed()
    {
        FieldManager.PlaceBlock(index, Prefab, out var block);
        var h = FieldAnimator.Handlers[Prefab].handler as FieldAnimator.SpecialHandler;
        h!.blocks = new List<(Vector2Int index, Block block)> { (index, block)};
        var lastGrid = FieldManager.CopyGrid();
        h.Handle();
        h.Animate();
        Used?.Invoke(lastGrid, FieldManager.Grid);
        base.OnUsed();
    }
}

[Serializable]
public class Bomb : BaseSpecialBlockBooster
{
    public Block prefab;
    public override Block Prefab => prefab;
}

[Serializable]
public class Rocket : BaseSpecialBlockBooster
{
    public Block xRocket;
    public Block yRocket;
    public LSToggle xRocketToggle;
    public LSToggle yRocketToggle;
    
    public override Block Prefab => xRocketToggle.IsOn ? xRocket : (yRocketToggle.IsOn ? yRocket : null);

    public override void Do()
    {
        base.Do();
        if (isTutorial)
        {
            CanvasUpdateRegistry.Updated += TryStartTutorial;
        }
    }

    private void TryStartTutorial()
    {
        CanvasUpdateRegistry.Updated -= TryStartTutorial;
        tutorialPointer.transform.position = xRocketToggle.transform.position;
        xRocketToggle.Submitted += OnSelected;
        yRocketToggle.Submitted += OnSelected;
    }

    private void OnSelected()
    {
        tutorialPointer.transform.position = FieldManager.ToPos(tutorialGridIndex);
        xRocketToggle.Submitted -= OnSelected;
        yRocketToggle.Submitted -= OnSelected;
    }
}

[Serializable]
public class Hummer : BaseFieldClickBooster
{
    public LaLa.PlayOneShot sound;
    public ParticleSystem fx;
    private Block block;
    
    protected override void OnClicked()
    {
        block = FieldManager.Grid.Get(index);
        if (block == null) return;
        OnUsed();
    }

    protected override void OnUsed()
    {
        sound.Do();
        var fxPos = FieldManager.ToPos(index);
        Object.Instantiate(fx, fxPos, Quaternion.identity);
        var lastGrid = FieldManager.CopyGrid();
        FieldManager.Grid.Set(index, null);
        Object.Destroy(block.gameObject);
        Used?.Invoke(lastGrid, FieldManager.Grid);
        base.OnUsed();
    }
}

[Serializable]
public class BoosterButton : DoIt, ILocalizationArgument
{
    [GetContext] public Object root;
    [Id(typeof(CurrencyIdGroup))] public Id id;
    public LSButton button;
    public LSText amountText;
    public GameObject countLabel;
    public GameObject plusLabel;
    public LocalizationText lockLeveText;
    public Canvas canvas;
    private static Canvas lastCanvas;
    [SerializeReference] public DoIt[] availableDoIts;
    [SerializeReference] public DoIt[] unavailableDoIts;
    public int unlockLevel;
    
    public AnimSequencer unlockAnimation;
    private JObject states;
    private Action submitAction;
    
    public override string ToString()
    {
        return unlockLevel.ToString();
    }

    public override void Do()
    {
        button.Submitted += OnSubmit;
        lockLeveText.LocalizeArguments(unlockLevel);
        states = GameSave.Config.AsJ<JObject>("states");
        states.As(id.ToString(), false);
        if (states[id.ToString()].ToBool())
        {
            unlockAnimation.Animate().Complete();
        }
        Funds.AddOnChanged(id, UpdateState, true);
        DestroyEvent.AddOnDestroy(root, () => Funds.RemoveOnChanged(id, UpdateState));
    }

    private void UpdateState(int amount)
    {
        amountText.text = amount.ToString();
        countLabel.SetActive(false);
        plusLabel.SetActive(false);
        
        if (GameSave.Level >= unlockLevel)
        {
            if (states.CheckDiffAndSync<bool>(id.ToString(), true))
            {
                Funds.Earn(id, 3);
                amount = Funds.GetAmount(id);
                unlockAnimation.Animate();
            }
            
            if(amount > 0)
            {
                Wait.Frames(1, TryStartTutorial);
                submitAction = OnAvailableClicked;
                countLabel.SetActive(true);
            }
            else
            {
                submitAction = unavailableDoIts.Do;
                plusLabel.SetActive(true);
            }
        }
        else
        {
            submitAction = null;
        }
    }

    private void TryStartTutorial()
    {
        if (FirstTime.IsNot($"Booster used {id}", out var pass))
        {
            Booster.isTutorial = true;
            button.Submit();
            UIViewBoss.IsGoBackBlocked = true;
            Booster.Used += OnUsed;
                    
            void OnUsed(Block[,] _, Block[,] __)
            {
                Booster.isTutorial = false;
                UIViewBoss.IsGoBackBlocked = false;
                UIViewBoss.GoBack();
                Booster.Used -= OnUsed;
                Analytic.LogEvent("booster_tutorial_completed");
                pass();
            }
        }
    }

    private void OnSubmit() => submitAction?.Invoke();

    private bool clicked;
    private void OnAvailableClicked()
    {
        if (clicked)
        {
            if (!UIViewBoss.IsGoBackBlocked)
            {
                UIViewBoss.GoBack();
            }
            
            return;
        }
        
        IUIView.Hiding += OnHiding;
        clicked = true;
        availableDoIts.Do();
        if(lastCanvas != null) lastCanvas.sortingOrder--;
        lastCanvas = canvas;
        canvas.sortingOrder++;
        
        void OnHiding(IUIView obj)
        {
            IUIView.Hiding -= OnHiding;
            clicked = false;
        }
    }
}