using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class Booster : DoIt { }

[Serializable]
public abstract class BaseFieldClickBooster : Booster
{
    public UIView view;
    
    public override void Do()
    {
        World.Destroyed += OnWorldDestroyed;
        World.Updated += Update;
        view.Manager.Hiding += OnHiding;
    }

    private void OnHiding()
    {
        view.Manager.Hiding -= OnHiding;
        World.Updated -= Update;
        World.Destroyed -= OnWorldDestroyed;
    }

    private void OnWorldDestroyed()
    {
        World.Destroyed -= OnWorldDestroyed;
        World.Updated -= Update;
    }
    
    private void Update()
    {
        if (LSInput.TouchCount > 0)
        {
            LSTouch touch = LSInput.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                var index = FieldManager.ToIndex(touchPosition);
                if (FieldManager.Grid.HasIndex(index))
                { 
                    OnClicked(index);
                }
            }
        }
    }

    protected abstract void OnClicked(Vector2Int index);
}

[Serializable]
public abstract class BaseSpecialBlockBooster : BaseFieldClickBooster
{
    public abstract Block Prefab { get; }

    protected override void OnClicked(Vector2Int index)
    {
        if(!Prefab) return;
        FieldManager.PlaceBlock(index, Prefab, out var block);
        UIViewBoss.GoBack();
        var h = FieldAnimator.Handlers[Prefab].handler as FieldAnimator.SpecialHandler;
        h!.blocks = new List<(Vector2Int index, Block block)> { (index, block)};
        h.Handle(); 
        h.Animate();
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
}

[Serializable]
public class Hummer : BaseFieldClickBooster
{
    public ParticleSystem fx;
    
    protected override void OnClicked(Vector2Int index)
    {
        var block = FieldManager.Grid.Get(index);
        if (block == null) return;
        UIViewBoss.GoBack();
        var fxPos = FieldManager.ToPos(index);
        Object.Instantiate(fx, fxPos, Quaternion.identity);
        FieldManager.Grid.Set(index, null);
        Object.Destroy(block.gameObject);
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
        states = CoreWorld.Config.AsJ<JObject>("states");
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
        
        if (CoreWorld.Level >= unlockLevel)
        {
            if (states.CheckDiffAndSync<bool>(id.ToString(), true))
            {
                Funds.Earn(id, 3);
                amount = Funds.GetAmount(id);
                unlockAnimation.Animate();
            }
            
            if(amount > 0)
            {
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

    private void OnSubmit() => submitAction?.Invoke();

    private void OnAvailableClicked()
    {
        availableDoIts.Do();
        if(lastCanvas != null) lastCanvas.sortingOrder--;
        lastCanvas = canvas;
        canvas.sortingOrder++;
    }
}