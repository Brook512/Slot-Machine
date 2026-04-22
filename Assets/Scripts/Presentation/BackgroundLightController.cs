using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Framework.Event;
using Framework.Procedure;
using Framework.Runtime;

public class BackgroundLightController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ProcedureManager procedureManager;
    [SerializeField] private Image[] lightBars;
    [SerializeField] private CanvasGroup[] lightGroups;

    [Header("Idle")]
    [SerializeField] private float idleMinAlpha = 0.55f;
    [SerializeField] private float idleMaxAlpha = 0.9f;
    [SerializeField] private float idleDuration = 1.5f;

    [Header("Spin")]
    [SerializeField] private float spinMinAlpha = 0.35f;
    [SerializeField] private float spinMaxAlpha = 1.0f;
    [SerializeField] private float spinDuration = 0.18f;

    [Header("Lose")]
    [SerializeField] private Image overlayDimImage;
    [SerializeField] private float loseDimAlpha = 0.12f;
    [SerializeField] private float loseDimDuration = 0.18f;

    private EventBus _eventBus;
    private Sequence[] _barSequences;

    private void Start()
    {
        if (procedureManager == null)
            procedureManager = FindObjectOfType<ProcedureManager>();

        if (procedureManager == null || procedureManager.Runtime == null)
        {
            enabled = false;
            return;
        }

        _eventBus = procedureManager.Runtime.EventBus;
        _eventBus.Subscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
        _eventBus.Subscribe<SettlementCompletedEvent>(OnSettlementCompleted);

        StartIdleLoop();
    }

    private void OnDestroy()
    {
        if (_eventBus != null)
        {
            _eventBus.Unsubscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Unsubscribe<SettlementCompletedEvent>(OnSettlementCompleted);
        }

        KillAllLoops();
    }

    private void OnRunPhaseChanged(RunPhaseChangedEvent e)
    {
        if (e.NewPhase == RunPhase.Spinning)
        {
            StartSpinLoop();
        }
        else if (e.NewPhase == RunPhase.Betting || e.NewPhase == RunPhase.PrepareRound)
        {
            StartIdleLoop();
        }
    }

    private void OnSettlementCompleted(SettlementCompletedEvent e)
    {
        if (e.IsWin)
        {
            PlayWinPulse();
        }
        else
        {
            PlayLoseDim();
            StartIdleLoop();
        }
    }

    private void StartIdleLoop()
    {
        BuildLoop(idleMinAlpha, idleMaxAlpha, idleDuration);
    }

    private void StartSpinLoop()
    {
        BuildLoop(spinMinAlpha, spinMaxAlpha, spinDuration);
    }

    private void PlayWinPulse()
    {
        foreach (var img in lightBars)
        {
            if (img == null) continue;
            img.DOKill();
            img.DOFade(1f, 0.08f).OnComplete(() =>
            {
                img.DOFade(idleMaxAlpha, 0.25f);
            });
        }

        foreach (var cg in lightGroups)
        {
            if (cg == null) continue;
            cg.DOKill();
            cg.DOFade(1f, 0.08f).OnComplete(() =>
            {
                cg.DOFade(idleMaxAlpha, 0.25f);
            });
        }
    }

    private void PlayLoseDim()
    {
        if (overlayDimImage == null) return;

        overlayDimImage.DOKill();
        var c = overlayDimImage.color;
        c.a = 0f;
        overlayDimImage.color = c;

        overlayDimImage.DOFade(loseDimAlpha, loseDimDuration)
            .OnComplete(() => overlayDimImage.DOFade(0f, loseDimDuration));
    }

    private void BuildLoop(float minAlpha, float maxAlpha, float duration)
    {
        KillAllLoops();

        if (lightBars != null)
        {
            foreach (var img in lightBars)
            {
                if (img == null) continue;
                img.DOKill();
                img.DOFade(maxAlpha, duration)
                    .From(minAlpha)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        if (lightGroups != null)
        {
            foreach (var cg in lightGroups)
            {
                if (cg == null) continue;
                cg.DOKill();
                cg.DOFade(maxAlpha, duration)
                    .From(minAlpha)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
    }

    private void KillAllLoops()
    {
        if (lightBars != null)
        {
            foreach (var img in lightBars)
            {
                if (img != null) img.DOKill();
            }
        }

        if (lightGroups != null)
        {
            foreach (var cg in lightGroups)
            {
                if (cg != null) cg.DOKill();
            }
        }
    }
}