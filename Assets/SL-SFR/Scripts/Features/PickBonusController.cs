using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickBonusController : MonoBehaviour
{
    [Header("Phase 1 - Tree Trunk Screen")]
    [SerializeField] private GameObject phase1Screen;
    [SerializeField] private List<Button> treeFlowerButtons = new List<Button>();
    [SerializeField] private List<ImageAnimation> treeFlowerAnimations = new List<ImageAnimation>();
    [SerializeField] private List<TMP_Text> treeFlowerPicksTexts = new List<TMP_Text>();
    [SerializeField] private TMP_Text phase1PicksPopupText;
    [SerializeField] private float phase1OpenAnimDelay = 0.6f;
    [SerializeField] private float phase1PostRevealDelay = 1.5f;

    [Header("Phase 2 - Grid Reveal Screen")]
    [SerializeField] private GameObject phase2Screen;
    [SerializeField] private List<Button> gridFlowerButtons = new List<Button>();
    [SerializeField] private List<ImageAnimation> gridFlowerOpenAnimations = new List<ImageAnimation>();
    [SerializeField] private Sprite closedFlowerSprite;
    [SerializeField] private TMP_Text picksRemainingText;
    [SerializeField] private TMP_Text picksPlayedText;
    [SerializeField] private float gridOpenAnimDelay = 0.25f;
    [SerializeField] private float postAllRevealsDelay = 1.2f;

    [Header("Animal On Flower Sprites")]
    [SerializeField] private Sprite bunnySprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite bullSprite;
    [SerializeField] private Sprite horseSprite;
    [SerializeField] private Sprite lionSprite;

    // Hook for left/right animal combo banner highlighting (next step)
    internal event Action<string, int> OnAnimalRevealed;

    private Dictionary<string, int> runningAnimalCounts = new Dictionary<string, int>();

    private bool phase1Picked;
    private int phase1AwardedPicks;
    private Action onPhase1PickedCallback;
    private Action onPhase1CompleteCallback;

    private List<PickBonusRevealData> sortedReveals = new List<PickBonusRevealData>();
    private int currentRevealStep;
    private bool isPhase2Active;
    private bool isRevealingStep;
    private Action onPhase2CompleteCallback;

    private void Awake()
    {
        BindButtons();
        gameObject.SetActive(false);
    }

    private void BindButtons()
    {
        for (int i = 0; i < treeFlowerButtons.Count; i++)
        {
            int index = i;
            if (treeFlowerButtons[i] != null)
            {
                treeFlowerButtons[i].onClick.AddListener(() => OnTreeFlowerClicked(index));
            }
        }

        for (int i = 0; i < gridFlowerButtons.Count; i++)
        {
            int index = i;
            if (gridFlowerButtons[i] != null)
            {
                gridFlowerButtons[i].onClick.AddListener(() => OnGridFlowerClicked(index));
            }
        }
    }

    #region Phase 1 - Tree Trunk

    internal void StartPhase1(int awardedPicks, Action onTreeFlowerPicked, Action onPhase1Complete)
    {
        phase1AwardedPicks = awardedPicks;
        onPhase1PickedCallback = onTreeFlowerPicked;
        onPhase1CompleteCallback = onPhase1Complete;
        phase1Picked = false;
        isPhase2Active = false;

        if (phase2Screen != null) phase2Screen.SetActive(false);
        if (phase1Screen != null) phase1Screen.SetActive(true);
        if (phase1PicksPopupText != null) phase1PicksPopupText.gameObject.SetActive(false);

        for (int i = 0; i < treeFlowerButtons.Count; i++)
        {
            if (treeFlowerButtons[i] != null)
            {
                treeFlowerButtons[i].interactable = true;
                treeFlowerButtons[i].gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < treeFlowerPicksTexts.Count; i++)
        {
            if (treeFlowerPicksTexts[i] != null)
            {
                treeFlowerPicksTexts[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    private void OnTreeFlowerClicked(int pickedIndex)
    {
        if (phase1Picked) return;
        phase1Picked = true;

        foreach (var btn in treeFlowerButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        onPhase1PickedCallback?.Invoke();
        StartCoroutine(Phase1RevealRoutine(pickedIndex));
    }

    private IEnumerator Phase1RevealRoutine(int pickedIndex)
    {
        if (pickedIndex >= 0 && pickedIndex < treeFlowerAnimations.Count && treeFlowerAnimations[pickedIndex] != null)
        {
            treeFlowerAnimations[pickedIndex].StartAnimation();
        }

        yield return new WaitForSeconds(phase1OpenAnimDelay);

        if (pickedIndex >= 0 && pickedIndex < treeFlowerPicksTexts.Count && treeFlowerPicksTexts[pickedIndex] != null)
        {
            treeFlowerPicksTexts[pickedIndex].text = phase1AwardedPicks.ToString();
            treeFlowerPicksTexts[pickedIndex].gameObject.SetActive(true);
        }

        if (phase1PicksPopupText != null)
        {
            phase1PicksPopupText.text = $"You won {phase1AwardedPicks} picks!";
            phase1PicksPopupText.gameObject.SetActive(true);
        }

        for (int i = 0; i < treeFlowerButtons.Count; i++)
        {
            if (i == pickedIndex) continue;

            if (i < treeFlowerAnimations.Count && treeFlowerAnimations[i] != null)
            {
                treeFlowerAnimations[i].StartAnimation();
            }

            if (i < treeFlowerPicksTexts.Count && treeFlowerPicksTexts[i] != null)
            {
                int offset = (i % 2 == 0) ? 1 : -1;
                int cosmeticValue = Mathf.Max(1, phase1AwardedPicks + offset);
                treeFlowerPicksTexts[i].text = cosmeticValue.ToString();
                treeFlowerPicksTexts[i].gameObject.SetActive(true);
            }
        }

        yield return new WaitForSeconds(phase1PostRevealDelay);

        onPhase1CompleteCallback?.Invoke();
    }

    #endregion

    #region Phase 2 - Grid Reveal

    internal void StartPhase2(PickBonusResultData completionData, Action onPhase2Complete)
    {
        onPhase2CompleteCallback = onPhase2Complete;
        currentRevealStep = 0;
        isRevealingStep = false;
        isPhase2Active = true;
        runningAnimalCounts.Clear();

        if (phase1Screen != null) phase1Screen.SetActive(false);
        if (phase2Screen != null) phase2Screen.SetActive(true);

        sortedReveals = (completionData != null && completionData.revealed != null)
            ? completionData.revealed.OrderBy(r => r.index).ToList()
            : new List<PickBonusRevealData>();

        for (int i = 0; i < gridFlowerButtons.Count; i++)
        {
            if (i < gridFlowerOpenAnimations.Count && gridFlowerOpenAnimations[i] != null)
            {
                gridFlowerOpenAnimations[i].StopAnimation();
            }

            if (gridFlowerButtons[i] != null)
            {
                gridFlowerButtons[i].interactable = true;
                gridFlowerButtons[i].gameObject.SetActive(true);

                Image cellImage = GetCellImage(i);
                if (cellImage != null && closedFlowerSprite != null)
                {
                    cellImage.sprite = closedFlowerSprite;
                }
            }
        }

        UpdatePhase2Counters();
        gameObject.SetActive(true);
    }

    private void OnGridFlowerClicked(int _)
    {
        if (!isPhase2Active || isRevealingStep) return;
        if (currentRevealStep >= sortedReveals.Count) return;

        StartCoroutine(RevealNextGridEntryRoutine());
    }

    private IEnumerator RevealNextGridEntryRoutine()
    {
        isRevealingStep = true;

        PickBonusRevealData entry = sortedReveals[currentRevealStep];
        currentRevealStep++;
        UpdatePhase2Counters();

        int cellIndex = entry.index;
        if (cellIndex >= 0 && cellIndex < gridFlowerButtons.Count && gridFlowerButtons[cellIndex] != null)
        {
            gridFlowerButtons[cellIndex].interactable = false;
        }

        if (cellIndex >= 0 && cellIndex < gridFlowerOpenAnimations.Count && gridFlowerOpenAnimations[cellIndex] != null)
        {
            gridFlowerOpenAnimations[cellIndex].StartAnimation();
            yield return new WaitForSeconds(gridOpenAnimDelay);
            gridFlowerOpenAnimations[cellIndex].StopAnimation();
        }

        Image cellImage = GetCellImage(cellIndex);
        Sprite animalSprite = GetAnimalSprite(entry.animalId);
        if (cellImage != null && animalSprite != null)
        {
            cellImage.sprite = animalSprite;
        }

        if (!string.IsNullOrEmpty(entry.animalId))
        {
            runningAnimalCounts.TryGetValue(entry.animalId, out int count);
            count++;
            runningAnimalCounts[entry.animalId] = count;
            OnAnimalRevealed?.Invoke(entry.animalId, count);
        }

        isRevealingStep = false;

        if (currentRevealStep >= sortedReveals.Count)
        {
            isPhase2Active = false;
            foreach (var btn in gridFlowerButtons)
            {
                if (btn != null) btn.interactable = false;
            }

            yield return new WaitForSeconds(postAllRevealsDelay);
            onPhase2CompleteCallback?.Invoke();
        }
    }

    private Image GetCellImage(int cellIndex)
    {
        if (cellIndex >= 0 && cellIndex < gridFlowerOpenAnimations.Count && gridFlowerOpenAnimations[cellIndex] != null && gridFlowerOpenAnimations[cellIndex].rendererDelegate != null)
        {
            return gridFlowerOpenAnimations[cellIndex].rendererDelegate;
        }

        if (cellIndex >= 0 && cellIndex < gridFlowerButtons.Count && gridFlowerButtons[cellIndex] != null)
        {
            return gridFlowerButtons[cellIndex].image;
        }

        return null;
    }

    private Sprite GetAnimalSprite(string animalId)
    {
        if (string.IsNullOrEmpty(animalId)) return null;

        switch (animalId.ToLowerInvariant())
        {
            case "rabbit": return bunnySprite;
            case "fox": return foxSprite;
            case "buffalo": return bullSprite;
            case "horse": return horseSprite;
            case "lion": return lionSprite;
            default: return null;
        }
    }

    private void UpdatePhase2Counters()
    {
        int remaining = Mathf.Max(0, sortedReveals.Count - currentRevealStep);
        if (picksRemainingText != null) picksRemainingText.text = remaining.ToString();
        if (picksPlayedText != null) picksPlayedText.text = currentRevealStep.ToString();
    }

    #endregion
}
