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
public class Bomb : Booster
{
    public UIView view;
    public Block prefab;
    
    public override void Do()
    {
        World.Updated += Update;
        view.Manager.Hiding += OnHiding;
    }

    private void OnHiding()
    {
        view.Manager.Hiding -= OnHiding;
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
                if (FieldManager.TryPlaceBlock(index, prefab, out var block))
                {
                    UIViewBoss.GoBack();
                    var h = FieldAnimator.Handlers[prefab].handler as FieldAnimator.SpecialHandler;
                    h!.indices = new List<Vector2Int> {index};
                    h.Handle();
                }
            }
        }
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
        lockLeveText.Localize(unlockLevel);
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
        lastCanvas =  canvas;
        canvas.sortingOrder++;
    }
}