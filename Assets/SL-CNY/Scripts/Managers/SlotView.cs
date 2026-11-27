using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlotView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Regular Reel symbols - Assign by Name")]
    [SerializeField] private Sprite spriteTiger;           // ID: 0
    [SerializeField] private Sprite spriteOwl;             // ID: 2
    [SerializeField] private Sprite spriteCat;             // ID: 3
    [SerializeField] private Sprite spriteFrog;            // ID: 4
    [SerializeField] private Sprite spriteDeer;            // ID: 5
    [SerializeField] private Sprite spriteFox;             // ID: 6
    [SerializeField] private Sprite spriteRedGem;          // ID: 7
    [SerializeField] private Sprite spriteOrangeGem;       // ID: 8
    [SerializeField] private Sprite spritePinkGem;         // ID: 9
    [SerializeField] private Sprite spriteGreenGem;        // ID: 10
    [SerializeField] private Sprite spriteRedStrawberry;   // ID: 11
    [SerializeField] private Sprite spriteBlueberry;       // ID: 13

    [Header("Special Symbol")]
    [SerializeField] private Sprite spriteGoldCoin;        // ID: 1

    [Header("Feature Trigger Symbols")]
    [SerializeField] private Sprite spriteGoldStrawberry;  // ID: 12
    [SerializeField] private Sprite spriteGoldBerries;     // ID: 14


    // Internal array built from named sprites
    private Sprite[] symbolSprites;

    [Header("Win Animation Sprite Arrays")]
    [Tooltip("Animation sprite arrays for symbols. USpin = ID 11")]
    [SerializeField] private List<Sprite> animSpritesUSpin;           // ID: 11

    // Internal array of animation sprite lists
    private List<Sprite>[] animationSpriteArrays;

    [Header("Reel Containers")]
    [SerializeField] private Transform[] reelTransforms;

    [Header("Reel Configuration")]
    [Tooltip("Starting index of visible symbols in the reel strip images list (5 for 12-image strip)")]
    [SerializeField] private int visibleStartIndex = 5;

    [Header("Reel Images")]
    [SerializeField] private List<ReelImages> reelImagesList;

    [Header("Continuous Spin Animation Settings")]
    [Tooltip("Total vertical distance the reel container scrolls per loop cycle (975 = exactly 3 symbols: 3 * 325px)")]
    [SerializeField] private float spinLoopDistance = 975f;
    [Tooltip("Duration in seconds for one complete loop cycle")]
    [SerializeField] private float spinLoopDuration = 0.20f;
    [Tooltip("Stagger delay between each reel starting its spin")]
    [SerializeField] private float reelStartStagger = 0.08f;
    [Tooltip("Stagger delay between each reel stopping")]
    [SerializeField] private float reelStopStagger = 0.15f;
    [Tooltip("Minimum spin duration before reels begin stopping")]
    [SerializeField] private float minSpinDuration = 0.4f;

    [Header("Reel Resting Position")]
    [Tooltip("Target anchored Y position for reels at rest")]
    [SerializeField] private float restingAnchoredY = 111.8f;

    [Header("Reel Dimensions")]
    [SerializeField] private float symbolHeight = 225f;
    [SerializeField] private float spinSpeed = 2000f;

    [Header("Animation Settings - Casino Style")]
    [SerializeField] private float anticipationUpDistance = 20f;
    [SerializeField] private float anticipationUpDuration = 0.12f;

    [Header("Win Animation Settings")]
    [SerializeField] private float winPopDuration = 0.4f;
    [SerializeField] private int winPopRepeat = 3;


    [Header("Stop Animation Settings")]
    [SerializeField] private float stopOvershootDistance = 50f;
    [SerializeField] private float stopOvershootDuration = 0.20f;
    [SerializeField] private float stopSettleDuration = 0.30f;

    [Header("Quick Spin Settings")]
    [SerializeField] private float quickStopStagger = 0.06f;
    [SerializeField] private float quickStopOvershoot = 20f;
    [SerializeField] private float quickStopDuration = 0.2f;
    [SerializeField] private int minSpinCyclesBeforeStop = 3;

    [Header("Reel Anticipation & Flower Settings")]
    [Tooltip("Extra spin duration in seconds when a reel is anticipating a feature trigger")]
    [SerializeField] private float anticipationDuration = 1.5f;
    [SerializeField] private GameObject flowerCol3;
    [SerializeField] private GameObject flowerCol4;
    [SerializeField] private Sprite flowerClosedSprite;
    [SerializeField] private Sprite flowerOpenSprite;


    [Header("Win Animation Settings")]
    [SerializeField] private float winAnimationDuration = 3.0f; // Total duration each win symbol animation plays
    [SerializeField] private float winSymbolLoopDuration = 1.5f;
    [SerializeField] private int winSymbolLoopCount = 3;
    [Tooltip("Delay between enabling winBox overlay and starting the ImageAnimation - for sync timing")]
    [SerializeField] private float winLineBoxToAnimationDelay = 0.05f;

    [Header("Phase 1 Total Win Presentation")]
    [SerializeField] private TMPro.TMP_Text phase1TotalWinText;

    [Header("Star Boom Animation")]
    [Tooltip("Full-screen star boom animation played when win activates")]
    [SerializeField] private GameObject starAnimationObject;
    [Tooltip("Small delay before win line text pops out, so it appears out of the star explosion")]
    [SerializeField] private float starToWinTextDelay = 0.05f;

    [Header("Mystery Coin Feature")]
    [Tooltip("Prefab for falling coins - assign in Inspector")]
    [SerializeField] private GameObject mysteryCoinPrefab;
    [Tooltip("UI container for falling coins - assign in Inspector")]
    [SerializeField] private Transform mysteryCoinsContainer;
    [Tooltip("Time in seconds for a coin to fall from top of screen to its symbol position")]
    [SerializeField] private float mysteryCoinFallDuration = 0.75f;
    [Tooltip("Stagger delay in seconds between consecutive falling coins")]
    [SerializeField] private float mysteryCoinStaggerDelay = 0.12f;

    [Header("Win Overlays Architecture (Optimized Separate Objects)")]
    [Tooltip("The GreyOverlay GameObject that dims all base reel icons and background")]
    [SerializeField] private GameObject greyOverlay;

    [Header("Win Frames (Slots_Frame: 5 Columns, 3 Rows each with ImageAnimation)")]
    [SerializeField] private ColumnOverlays[] winFrameColumns = new ColumnOverlays[5];

    [Header("Win Symbols / Win Slots (WinSymbols: 5 Columns, 3 Rows each with Image)")]
    [SerializeField] private ColumnOverlays[] winSymbolColumns = new ColumnOverlays[5];

    [Header("Legacy Overlay References (Optional / Fallback)")]
    [SerializeField] private ColumnOverlays[] winBoxColumns = new ColumnOverlays[5];
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];

    [Header("Safari Ride Win Highlight Frame Animation (31 frames from Frame_Animation)")]
    [SerializeField] private List<Sprite> highlightFrameSprites = new List<Sprite>();


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;


    private RectTransform[] reelRectTransforms;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix;

    private bool isSpinning;

    // Fast 1-time cached overlay components for 0-allocation, 0-GetComponent runtime execution
    private Image[,] cachedWinSymbolImages = new Image[5, 3];
    private ImageAnimation[,] cachedWinFrameAnims = new ImageAnimation[5, 3];
    private ImageAnimation cachedStarAnim;
    private List<GameObject> pooledMysteryCoins = new List<GameObject>();
    private Coroutine mysteryCoinCoroutine;

    #region Initialization

    private void Awake()
    {
        BuildSymbolSpriteArray();
        InitializeReels();
        CacheWinOverlayComponents();
    }

    private void CacheWinOverlayComponents()
    {
        EnsureMysteryCoinPool();

        // Cache Star Boom Animation
        if (starAnimationObject != null)
        {
            var img = starAnimationObject.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;

            var rt = starAnimationObject.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta == new Vector2(100f, 100f))
            {
                rt.sizeDelta = new Vector2(1920f, 1080f);
            }

            cachedStarAnim = starAnimationObject.GetComponent<ImageAnimation>();
            if (cachedStarAnim != null)
            {
                cachedStarAnim.doLoopAnimation = false;
                cachedStarAnim.onLoopComplete = (count) =>
                {
                    if (cachedStarAnim != null && !cachedStarAnim.doLoopAnimation && starAnimationObject != null)
                    {
                        starAnimationObject.SetActive(false);
                    }
                };
            }
        }
        else
        {
            Debug.LogWarning("[SlotView] starAnimationObject is not assigned in the Inspector!");
        }

        for (int c = 0; c < 5; c++)
        {
            for (int r = 0; r < 3; r++)
            {
                // Cache Win Symbol Images
                if (winSymbolColumns != null && c < winSymbolColumns.Length && winSymbolColumns[c]?.rows != null && r < winSymbolColumns[c].rows.Length)
                {
                    var go = winSymbolColumns[c].rows[r];
                    if (go != null)
                    {
                        cachedWinSymbolImages[c, r] = go.GetComponent<Image>();
                    }
                }

                // Cache Win Frame Animations
                GameObject frameGO = null;
                if (winFrameColumns != null && c < winFrameColumns.Length && winFrameColumns[c]?.rows != null && r < winFrameColumns[c].rows.Length)
                {
                    frameGO = winFrameColumns[c].rows[r];
                }
                else if (winAnimationColumns != null && c < winAnimationColumns.Length && winAnimationColumns[c]?.rows != null && r < winAnimationColumns[c].rows.Length)
                {
                    frameGO = winAnimationColumns[c].rows[r];
                }

                if (frameGO != null)
                {
                    var anim = frameGO.GetComponent<ImageAnimation>();
                    if (anim != null)
                    {
                        if ((anim.textureArray == null || anim.textureArray.Count == 0) && highlightFrameSprites != null && highlightFrameSprites.Count > 0)
                        {
                            anim.textureArray = highlightFrameSprites;
                        }
                        if (anim.rendererDelegate == null)
                        {
                            anim.rendererDelegate = frameGO.GetComponent<Image>();
                        }
                        anim.doLoopAnimation = true;
                        cachedWinFrameAnims[c, r] = anim;
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
        }

        DisableAllOverlays();
        SanitizeAllReelStripImages();
        SetupSymbolButtons();
        ResetFlowers();

        // Ensure every symbol on every reel strip is active, enabled, and fully opaque
        if (reelImagesList != null)
        {
            foreach (var reel in reelImagesList)
            {
                if (reel?.images != null)
                {
                    foreach (var img in reel.images)
                    {
                        if (img != null)
                        {
                            img.gameObject.SetActive(true);
                            img.enabled = true;
                            Color c = img.color;
                            img.color = new Color(c.r, c.g, c.b, 1f);
                        }
                    }
                }
            }
        }

        // Lock all reels to their exact resting anchored position on game start
        if (reelRectTransforms != null)
        {
            for (int col = 0; col < reelRectTransforms.Length; col++)
            {
                if (reelRectTransforms[col] != null)
                {
                    reelRectTransforms[col].anchoredPosition = new Vector2(
                        reelRectTransforms[col].anchoredPosition.x,
                        restingAnchoredY
                    );
                }
            }
        }
    }

    private void SanitizeAllReelStripImages()
    {
        if (reelImagesList == null) return;
        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;
            for (int i = 0; i < reel.images.Count; i++)
            {
                if (reel.images[i] != null)
                {
                    if (!IsBaseSymbolSprite(reel.images[i].sprite))
                    {
                        reel.images[i].sprite = GetSymbolSprite(GetRandomBaseSymbolId());
                    }
                }
            }
        }
    }

    private bool IsBaseSymbolSprite(Sprite s)
    {
        if (s == null || symbolSprites == null) return false;
        foreach (int id in baseSymbolIds)
        {
            if (id >= 0 && id < symbolSprites.Length && symbolSprites[id] == s)
                return true;
        }
        return false;
    }

    private void DisableAllOverlays()
    {
        if (greyOverlay != null)
        {
            greyOverlay.SetActive(false);
        }

        for (int c = 0; c < 5; c++)
        {
            for (int r = 0; r < 3; r++)
            {
                var winImg = cachedWinSymbolImages[c, r];
                if (winImg != null) winImg.gameObject.SetActive(false);

                var frameAnim = cachedWinFrameAnims[c, r];
                if (frameAnim != null)
                {
                    frameAnim.StopAnimation();
                    frameAnim.gameObject.SetActive(false);
                }
            }
        }

        HideAllWinLineTexts();
        HidePhase1TotalWinText();
        if (symbolInfoCard) symbolInfoCard.HideCard();

        if (mysteryCoinCoroutine != null)
        {
            StopCoroutine(mysteryCoinCoroutine);
            mysteryCoinCoroutine = null;
        }
        if (pooledMysteryCoins != null)
        {
            foreach (var coin in pooledMysteryCoins)
            {
                if (coin != null) coin.SetActive(false);
            }
        }

        if (cachedStarAnim != null)
        {
            cachedStarAnim.StopAnimation();
        }
        if (starAnimationObject != null)
        {
            starAnimationObject.SetActive(false);
        }
    }

    private void DisableAllHighlightFrames()
    {
        DisableAllOverlays();
    }

    private void SetupSymbolButtons()
    {
        if (reelImagesList == null) return;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;

            for (int row = 0; row < rowCount; row++)
            {
                Image img = GetVisibleImage(col, row);
                if (img != null)
                {
                    SymbolButtonHandler btnHandler = img.GetComponent<SymbolButtonHandler>();
                    if (btnHandler == null)
                    {
                        btnHandler = img.gameObject.AddComponent<SymbolButtonHandler>();
                    }
                    btnHandler.Init(col, row, this);
                }
            }
        }
    }

    internal void HideSymbolInfoCard()
    {
        if (symbolInfoCard != null) symbolInfoCard.HideCard();
    }

    internal void OnBetChanged()
    {
        if (symbolInfoCard != null && symbolInfoCard.gameObject.activeSelf)
        {
            symbolInfoCard.RefreshCard(gameManager);
        }
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
        if (isSpinning)
        {
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
            return;
        }

        if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count)
        {
            return;
        }

        int symbolId = currentDisplayMatrix[col][row];
        if (symbolInfoCard != null)
        {
            symbolInfoCard.ShowCard(symbolId, col, row, symbolRect, gameManager);
        }
    }

    private static void DisableColumns(ColumnOverlays[] cols)
    {
        // Safe no-op: never deactivate reel symbol GameObjects!
    }

    private GameObject GetWinAnimationObject(int col, int row)
    {
        var rawGO = WinBox(winAnimationColumns, col, row);
        if (rawGO == null) return null;

        // If the referenced GameObject is a SlotIcon (contains AnimObject child),
        // target its child "AnimObject" instead of the symbol itself, so the symbol never gets deactivated!
        Transform animChild = rawGO.transform.Find("AnimObject");
        if (animChild != null)
        {
            return animChild.gameObject;
        }

        if (rawGO.GetComponent<ImageAnimation>() != null)
        {
            return rawGO;
        }

        var childAnim = rawGO.GetComponentInChildren<ImageAnimation>(true);
        if (childAnim != null)
        {
            return childAnim.gameObject;
        }

        return rawGO;
    }

    private static GameObject WinBox(ColumnOverlays[] cols, int col, int row)
        => (col >= 0 && col < cols?.Length && cols[col]?.rows != null && row >= 0 && row < cols[col].rows.Length)
            ? cols[col].rows[row] : null;

    private void BuildSymbolSpriteArray()
    {
        // Build the symbol sprite array from named sprite fields (IDs 0-14)
        symbolSprites = new Sprite[15];

        symbolSprites[0] = spriteTiger;
        symbolSprites[1] = spriteGoldCoin;
        symbolSprites[2] = spriteOwl;
        symbolSprites[3] = spriteCat;
        symbolSprites[4] = spriteFrog;
        symbolSprites[5] = spriteDeer;
        symbolSprites[6] = spriteFox;
        symbolSprites[7] = spriteRedGem;
        symbolSprites[8] = spriteOrangeGem;
        symbolSprites[9] = spritePinkGem;
        symbolSprites[10] = spriteGreenGem;
        symbolSprites[11] = spriteRedStrawberry;
        symbolSprites[12] = spriteGoldStrawberry;
        symbolSprites[13] = spriteBlueberry;
        symbolSprites[14] = spriteGoldBerries;

        // Validate
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] == null)
            {
                Debug.LogError($"[SlotView] Symbol sprite at index {i} is not assigned in inspector!");
            }
        }

        // Build the animation sprite arrays
        animationSpriteArrays = new List<Sprite>[15];
        // Only USpin has an animation in this game - SFR has different animations.
        //animationSpriteArrays[11] = animSpritesUSpin;
    }

    private void InitializeReels()
    {
        cycleDistance = symbolHeight;
        if (reelTransforms != null && reelTransforms.Length > 0)
        {
            reelRectTransforms = new RectTransform[reelTransforms.Length];
            for (int i = 0; i < reelTransforms.Length; i++)
            {
                if (reelTransforms[i] != null)
                {
                    reelRectTransforms[i] = reelTransforms[i] as RectTransform;
                }
            }
        }

        // Ensure all reel strip images are active from Awake
        if (reelImagesList != null)
        {
            for (int col = 0; col < reelImagesList.Count; col++)
            {
                var reel = reelImagesList[col];
                if (reel?.images == null) continue;
                for (int i = 0; i < reel.images.Count; i++)
                {
                    if (reel.images[i] != null)
                    {
                        reel.images[i].gameObject.SetActive(true);
                        reel.images[i].enabled = true;
                    }
                }
            }
        }

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        currentDisplayMatrix = new List<List<int>>();
        for (int col = 0; col < 5; col++)
        {
            var defaultCol = new List<int>();
            for (int r = 0; r < rowCount; r++)
            {
                defaultCol.Add(0);
            }
            currentDisplayMatrix.Add(defaultCol);
            reelCycleCount.Add(0);
        }
    }

    internal void SetInitialMatrix(List<List<int>> matrix)
    {
        if (matrix == null || matrix.Count != 5) return;

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        for (int col = 0; col < 5; col++)
        {
            if (matrix[col].Count != rowCount) return;
        }

        currentDisplayMatrix = matrix;

        for (int col = 0; col < 5; col++)
        {
            SetReelSymbols(col, matrix[col], true);
        }
    }

    #endregion

    #region Symbol Display
    // Base symbols allowed on the spinning strip and initial view:
    // 0: Tiger, 2: Owl, 3: Cat, 4: Frog, 5: Deer, 6: Fox
    // 7: RedGem, 8: OrangeGem, 9: PinkGem, 10: GreenGem
    // 11: RedStrawberry (scatterPick), 13: BlueBerries (scatterFree)
    private readonly List<int> baseSymbolIds = new List<int> { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13 };

    public int VisibleStartIndex => visibleStartIndex;

    public Image GetVisibleImage(int col, int row)
    {
        if (col < 0 || col >= reelImagesList.Count) return null;
        var reel = reelImagesList[col];
        if (reel == null || reel.images == null) return null;
        int imageIndex = visibleStartIndex + row;
        if (imageIndex < 0 || imageIndex >= reel.images.Count) return null;
        return reel.images[imageIndex];
    }

    private int GetRandomBaseSymbolId()
    {
        int randomIndex = Random.Range(0, baseSymbolIds.Count);
        return baseSymbolIds[randomIndex];
    }

    private int GetRandomRegularSymbolId() => GetRandomBaseSymbolId();

    private void SetReelSymbols(int columnIndex, List<int> visibleSymbolIds, bool isInitial = false)
    {
        if (columnIndex >= reelImagesList.Count)
        {
            Debug.LogError($"SetReelSymbols: Invalid column index {columnIndex}, max is {reelImagesList.Count - 1}");
            return;
        }

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        if (visibleSymbolIds == null || visibleSymbolIds.Count != rowCount)
        {
            Debug.LogError($"SetReelSymbols: Invalid visibleSymbolIds count {visibleSymbolIds?.Count}, expected {rowCount}");
            return;
        }

        var reel = reelImagesList[columnIndex];

        if (reel.images == null || reel.images.Count < (visibleStartIndex + rowCount))
        {
            Debug.LogError($"SetReelSymbols: Reel {columnIndex} has invalid image count {reel.images?.Count}, expected at least {visibleStartIndex + rowCount}");
            return;
        }

        for (int row = 0; row < rowCount; row++)
        {
            int imageIndex = visibleStartIndex + row;
            if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
            {
                int symbolId = visibleSymbolIds[row];
                reel.images[imageIndex].gameObject.SetActive(true);
                reel.images[imageIndex].enabled = true;
                Color c = reel.images[imageIndex].color;
                reel.images[imageIndex].color = new Color(c.r, c.g, c.b, 1f);
                reel.images[imageIndex].sprite = GetSymbolSprite(symbolId);
            }
        }

        for (int i = 0; i < visibleStartIndex; i++)
        {
            reel.images[i].sprite = GetSymbolSprite(GetRandomBaseSymbolId());
        }

        for (int i = visibleStartIndex + rowCount; i < reel.images.Count; i++)
        {
            reel.images[i].sprite = GetSymbolSprite(GetRandomBaseSymbolId());
        }

        if (isInitial && reelRectTransforms != null && columnIndex < reelRectTransforms.Length && reelRectTransforms[columnIndex] != null)
        {
            reelRectTransforms[columnIndex].anchoredPosition = new Vector2(
                reelRectTransforms[columnIndex].anchoredPosition.x,
                restingAnchoredY
            );
        }
    }

    private Sprite GetSymbolSprite(int symbolId)
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
        }

        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogError("[SlotView] symbolSprites array is null or empty after BuildSymbolSpriteArray!");
            return null;
        }

        // Validate symbolId range (0-12)
        if (symbolId < 0 || symbolId >= symbolSprites.Length)
        {
            Debug.LogWarning($"[SlotView] Invalid symbolId {symbolId}, using default sprite 0. Total sprites: {symbolSprites.Length}");
            return symbolSprites[0];
        }

        if (symbolSprites[symbolId] == null)
        {
            Debug.LogError($"[SlotView] Symbol sprite for ID {symbolId} is null!");
            return symbolSprites[0];
        }

        return symbolSprites[symbolId];
    }

    #endregion

    #region Continuous Reel Spin Animation

    internal void StartSpin()
    {
        if (isSpinning) return;

        if (symbolInfoCard != null) symbolInfoCard.HideCard();

        isSpinning = true;
        KillAllTweens();
        DisableAllOverlays();
        ResetFlowers();

        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
        }
        spinRoutine = StartCoroutine(StartSpinSequence());
    }

    private Coroutine spinRoutine;

    private IEnumerator StartSpinSequence()
    {
        for (int col = 0; col < reelTransforms.Length && col < 5; col++)
        {
            InitializeLoopingTween(col);
            if (reelStartStagger > 0f)
            {
                yield return new WaitForSeconds(reelStartStagger);
            }
        }
    }

    private void InitializeLoopingTween(int col)
    {
        if (reelRectTransforms == null || col >= reelRectTransforms.Length || reelRectTransforms[col] == null) return;

        RectTransform slotRect = reelRectTransforms[col];
        slotRect.DOKill();
        slotRect.anchoredPosition = new Vector2(slotRect.anchoredPosition.x, restingAnchoredY);

        Tweener loopTweener = slotRect.DOAnchorPosY(restingAnchoredY - spinLoopDistance, spinLoopDuration)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        loopTweener.Play();

        if (spinTweens.Count <= col)
            spinTweens.Add(loopTweener);
        else
            spinTweens[col] = loopTweener;
    }

    #endregion

    #region Stop Spin

    internal void StopSpin(List<List<int>> resultMatrix, System.Action onComplete)
    {
        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            for (int col = 0; col < 5; col++)
            {
                SetReelSymbols(col, resultMatrix[col], false);
            }
            ResetFlowers();
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, false));
    }

    private IEnumerator StopSpinSequence(List<List<int>> resultMatrix, System.Action onComplete, bool isQuickStop)
    {
        currentDisplayMatrix = resultMatrix;

        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
            spinRoutine = null;
        }

        // Ensure all reels have started their looping tween before stopping
        for (int col = 0; col < 5; col++)
        {
            if (spinTweens.Count <= col || spinTweens[col] == null || !spinTweens[col].IsActive())
            {
                InitializeLoopingTween(col);
            }
        }

        if (!isQuickStop && minSpinDuration > 0f)
        {
            yield return new WaitForSeconds(minSpinDuration);
        }

        float stagger = isQuickStop ? quickStopStagger : reelStopStagger;
        int strawberryCount = 0;
        int blueberryCount = 0;

        for (int col = 0; col < 5; col++)
        {
            // Ensure slot 4's flower (flowerCol3) is turned off before starting animation/spin of slot 5 (col 4)
            if (col == 4)
            {
                ResetSingleFlower(flowerCol3);
            }

            // Anticipation check on reels 3 and 4 (the 4th and 5th reels)
            // Only trigger tease if at least 2 reels have landed Strawberries OR at least 2 reels have landed Blueberries!
            bool shouldTease = (col == 3 || col == 4) && (strawberryCount >= 2 || blueberryCount >= 2);
            if (!isQuickStop && shouldTease)
            {
                TriggerAnticipation(col);
                yield return new WaitForSeconds(anticipationDuration);
            }

            StopSingleReel(col, resultMatrix[col], isQuickStop);

            // Once reel 3 (slot 4) stops and displays its result, turn its flower off immediately!
            if (col == 3)
            {
                ResetSingleFlower(flowerCol3);
            }
            else if (col == 4)
            {
                ResetSingleFlower(flowerCol4);
            }

            // Count landed feature scatters on this reel (strawberry and blueberry)
            if (resultMatrix != null && col < resultMatrix.Count && resultMatrix[col] != null)
            {
                bool hasStrawberry = false;
                bool hasBlueberry = false;
                foreach (int symId in resultMatrix[col])
                {
                    if (IsStrawberrySymbol(symId)) hasStrawberry = true;
                    if (IsBlueberrySymbol(symId)) hasBlueberry = true;
                }
                if (hasStrawberry) strawberryCount++;
                if (hasBlueberry) blueberryCount++;
            }

            if (!isQuickStop && stagger > 0f)
            {
                yield return new WaitForSeconds(stagger);
            }
        }

        float longestDuration = isQuickStop ? quickStopDuration : (stopOvershootDuration + stopSettleDuration);
        yield return new WaitForSeconds(longestDuration);

        // All reels have finished spinning and final result is now displayed -> stop flower tease animations
        ResetFlowers();

        isSpinning = false;
        onComplete?.Invoke();
    }

    private void StopSingleReel(int columnIndex, List<int> targetSymbols, bool isQuickStop)
    {
        if (columnIndex < spinTweens.Count && spinTweens[columnIndex] != null)
        {
            spinTweens[columnIndex].Kill();
        }

        if (reelRectTransforms == null || columnIndex >= reelRectTransforms.Length) return;
        RectTransform slotRect = reelRectTransforms[columnIndex];
        if (slotRect == null) return;

        slotRect.DOKill();

        // 1. Silently update visible target symbols on the reel strip
        SetReelSymbols(columnIndex, targetSymbols, false);

        // 2. Play reel-stop audio
        AudioManager.Instance?.PlayReelStop();

        // 3. Elastic settling stop
        if (isQuickStop)
        {
            Sequence quickStopSequence = DOTween.Sequence();
            slotRect.anchoredPosition = new Vector2(slotRect.anchoredPosition.x, restingAnchoredY);
            quickStopSequence.Append(
                slotRect.DOAnchorPosY(restingAnchoredY - quickStopOvershoot, quickStopDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );
            quickStopSequence.Append(
                slotRect.DOAnchorPosY(restingAnchoredY, quickStopDuration * 0.7f)
                    .SetEase(Ease.InOutQuad)
            );
            quickStopSequence.OnComplete(() => PlayStopAnimationsForColumn(columnIndex));
            quickStopSequence.Play();

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(quickStopSequence);
            else
                spinTweens[columnIndex] = quickStopSequence;
        }
        else
        {
            Sequence stopSequence = DOTween.Sequence();
            slotRect.anchoredPosition = new Vector2(slotRect.anchoredPosition.x, restingAnchoredY);
            stopSequence.Append(
                slotRect.DOAnchorPosY(restingAnchoredY - stopOvershootDistance, stopOvershootDuration)
                    .SetEase(Ease.OutQuad)
            );
            stopSequence.Append(
                slotRect.DOAnchorPosY(restingAnchoredY, stopSettleDuration)
                    .SetEase(Ease.OutBack)
            );
            stopSequence.OnComplete(() => PlayStopAnimationsForColumn(columnIndex));
            stopSequence.Play();

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(stopSequence);
            else
                spinTweens[columnIndex] = stopSequence;
        }
    }

    private void TriggerAnticipation(int col)
    {
        AudioManager.Instance?.PlayAnticipation();
        AudioManager.Instance?.PlayFlowerOpen();

        if (col == 3)
        {
            AnimateFlowerOpen(flowerCol3);
        }
        else if (col == 4)
        {
            AnimateFlowerOpen(flowerCol4);
        }
    }

    private void AnimateFlowerOpen(GameObject flowerGO)
    {
        if (flowerGO == null) return;
        flowerGO.SetActive(true);

        Image img = flowerGO.GetComponent<Image>();
        if (img != null && flowerOpenSprite != null)
        {
            img.sprite = flowerOpenSprite;
        }

        ImageAnimation anim = flowerGO.GetComponent<ImageAnimation>();
        if (anim != null)
        {
            anim.StartAnimation();
        }
        else
        {
            flowerGO.transform.DOKill();
            flowerGO.transform.localScale = Vector3.one * 0.8f;
            flowerGO.transform.DOScale(1.25f, 0.25f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                flowerGO.transform.localScale = Vector3.one;
            });
        }
    }

    private void ResetFlowers()
    {
        ResetSingleFlower(flowerCol3);
        ResetSingleFlower(flowerCol4);
    }

    private void ResetSingleFlower(GameObject flowerGO)
    {
        if (flowerGO == null) return;
        flowerGO.transform.DOKill();
        flowerGO.transform.localScale = Vector3.one;

        ImageAnimation anim = flowerGO.GetComponent<ImageAnimation>();
        if (anim != null)
        {
            anim.StopAnimation();
            anim.CancelInvoke();
        }
        Image img = flowerGO.GetComponent<Image>();
        if (img != null && flowerClosedSprite != null)
        {
            img.sprite = flowerClosedSprite;
        }

        flowerGO.SetActive(false);
    }

    #endregion

    #region Quick Spin

    internal void QuickStop(List<List<int>> resultMatrix, System.Action onComplete = null)
    {
        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            for (int col = 0; col < 5; col++)
            {
                if (col < reelTransforms.Length)
                {
                    SetReelSymbols(col, resultMatrix[col], false);
                    reelTransforms[col].localPosition = new Vector3(
                        reelTransforms[col].localPosition.x,
                        // middlePosition,
                        0
                    );
                    if (reelRectTransforms != null && col < reelRectTransforms.Length && reelRectTransforms[col] != null)
                    {
                        reelRectTransforms[col].anchoredPosition = new Vector2(
                            reelRectTransforms[col].anchoredPosition.x,
                            restingAnchoredY
                        );
                    }
                    PlayStopAnimationsForColumn(col);
                }
            }
            
            ResetFlowers();
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, true));
    }

    #endregion

    #region Stop Symbol Animations

    public bool IsWildSymbol(int symId)
    {
        // Wild variants: ID 1 (Gold Coin), ID 16 (Wild Yin Yang), ID 17 (Wild Strawberry)
        // Note: ID 10 is Green Gem (regular base symbol) and is NOT wild.
        if (symId == 1 || symId == 16 || symId == 17) return true;
        if (gameManager?.gameConfig != null && symId == gameManager.gameConfig.wildSymbolId && symId != 10) return true;
        if (gameManager?.gameConfig?.symbols != null)
        {
            var sym = gameManager.gameConfig.symbols.Find(s => s.id == symId);
            if (sym != null) return sym.isWild;
        }
        return false;
    }

    public bool IsStrawberrySymbol(int symId)
    {
        // Strawberry feature symbols: ID 11 (Red Strawberry), ID 12 (Gold Strawberry), ID 17 (Wild Strawberry)
        return symId == 11 || symId == 12 || symId == 17;
    }

    public bool IsBlueberrySymbol(int symId)
    {
        // Blueberry feature symbols: ID 13 (Blueberry), ID 14 (Gold Berries), ID 15, ID 16
        return symId == 13 || symId == 14 || symId == 15 || symId == 16;
    }

    public bool IsScatterSymbol(int symId)
    {
        return IsStrawberrySymbol(symId) || IsBlueberrySymbol(symId);
    }

    private void PlayStopAnimationsForColumn(int col)
    {
        // No scale animations on symbol stop in Safari Ride
    }

    internal void AnimateAllScatters(int loopCount)
    {
        if (currentDisplayMatrix == null) return;

        // Clear any individual hit animations before starting the collective one
        KillWinTweens();

        int actualScatterId = gameManager?.gameConfig != null ? gameManager.gameConfig.scatterSymbolId : -1;
        if (actualScatterId < 0) return;
        
        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == actualScatterId)
                {
                    AnimateSymbolSingleLoop(col, row, loopCount);
                }
            }
        }
    }

    internal void AnimateUSpinWin(System.Action onComplete = null)
    {
        if (currentDisplayMatrix == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        List<ImageAnimation> activeUSpinAnims = new List<ImageAnimation>();
        int completedCount = 0;
        int targetLoops = 2; // Exactly 2 loops of full animation

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == 11) // USpin Symbol ID
                {
                    EnableWinBox(col, row);
                    
                    var animGO = GetWinAnimationObject(col, row);
                    if (animGO != null)
                    {
                        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
                        Image symbolImage = GetVisibleImage(col, row);

                        if (imageAnim != null)
                        {
                            activeUSpinAnims.Add(imageAnim);

                            List<Sprite> animSprites = animationSpriteArrays[11];
                            imageAnim.textureArray = animSprites;
                            imageAnim.doLoopAnimation = true;

                            animGO.SetActive(true);
                            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : animGO.GetComponent<Image>();
                            if (animRenderer != null)
                            {
                                animRenderer.DOKill();
                                Color c = animRenderer.color;
                                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                            }
                            if (symbolImage != null)
                            {
                                symbolImage.DOKill();
                                symbolImage.DOFade(0f, 0.2f);
                            }

                            imageAnim.onLoopComplete = (loopCount) =>
                            {
                                if (loopCount >= targetLoops)
                                {
                                    imageAnim.onLoopComplete = null;
                                    imageAnim.StopAnimation();
                                    if (animGO != null && animGO.GetComponent<SymbolButtonHandler>() == null)
                                    {
                                        animGO.SetActive(false);
                                    }

                                    if (symbolImage != null)
                                    {
                                        symbolImage.DOKill();
                                        symbolImage.DOFade(1f, 0.2f);
                                    }

                                    completedCount++;
                                    if (completedCount >= activeUSpinAnims.Count)
                                    {
                                        onComplete?.Invoke();
                                    }
                                }
                            };

                            imageAnim.StartAnimation();
                        }
                    }
                }
            }
        }

        if (activeUSpinAnims.Count == 0)
        {
            onComplete?.Invoke();
        }
    }

    internal void AnimateMoneyBagWin()
    {
        if (currentDisplayMatrix == null) return;

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == 12) // MoneyBag Symbol ID
                {
                    EnableWinBox(col, row);
                }
            }
        }
    }

    internal void AnimatePickBonusWin()
    {
        if (currentDisplayMatrix == null) return;

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        const int redStrawberrySymbolId = 11;
        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == redStrawberrySymbolId)
                {
                    EnableWinBox(col, row);
                }
            }
        }
    }

    private void AnimateSymbolSingleLoop(int column, int row, int loopCount = 1)
    {
        Image symbolImage = GetVisibleImage(column, row);
        if (symbolImage == null) return;

        var animGO = GetWinAnimationObject(column, row);
        if (animGO == null) return;

        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null) return;

        int symbolId = currentDisplayMatrix[column][row];
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) return;

        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0) return;

        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();
        
        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);
            
            imageAnim.StartAnimation();
        });

        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null && animGO.GetComponent<SymbolButtonHandler>() == null)
            {
                animGO.SetActive(false);
            }

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    #endregion

    #region Win Line Animation

    internal void ShowWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
    {
        if ((winLines == null || winLines.Count == 0) && (gameManager?.lastResult == null || gameManager.lastResult.winAmount <= 0))
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        winAnimationCoroutine = StartCoroutine(PlayWinPresentation(winLines ?? new List<WinLine>(), onComplete));
    }

    private HashSet<int> CollectPaylinePositions(WinLine winLine)
    {
        var positions = new HashSet<int>();
        if (winLine == null) return positions;

        if (winLine.positions != null && winLine.positions.Count > 0)
        {
            foreach (int flatIndex in winLine.positions)
            {
                positions.Add(flatIndex);
            }
            return positions;
        }

        if (gameManager?.gameConfig?.paylines != null && winLine.lineId >= 0 && winLine.lineId < gameManager.gameConfig.paylines.Count)
        {
            var lineCoords = gameManager.gameConfig.paylines[winLine.lineId];
            int count = winLine.symbolIds != null && winLine.symbolIds.Count > 0 ? winLine.symbolIds.Count : (lineCoords != null ? lineCoords.Count : 3);
            for (int c = 0; c < count && c < 5 && c < lineCoords.Count; c++)
            {
                int r = lineCoords[c];
                positions.Add(r * 5 + c);
            }
        }

        return positions;
    }

    private IEnumerator PlayWinPresentation(List<WinLine> winLines, System.Action onComplete)
    {
        // 1. Collect per-payline position sets and union all winning positions for Phase 1
        var paylinePositionSets = new List<HashSet<int>>();
        var allWinPositions = new HashSet<int>();
        double totalWinAmount = 0;

        if (winLines != null)
        {
            foreach (var winLine in winLines)
            {
                totalWinAmount += winLine.winAmount;
                HashSet<int> linePositions = CollectPaylinePositions(winLine);
                if (linePositions.Count > 0)
                {
                    paylinePositionSets.Add(linePositions);
                    allWinPositions.UnionWith(linePositions);
                }
            }
        }

        if (totalWinAmount <= 0 && gameManager != null && gameManager.lastResult != null)
        {
            totalWinAmount = gameManager.lastResult.winAmount;
        }

        // Fallback: if no payline positions were resolved but totalWinAmount > 0, find matching line symbols
        if (allWinPositions.Count == 0 && totalWinAmount > 0 && currentDisplayMatrix != null)
        {
            int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;
            for (int r = 0; r < rowCount; r++)
            {
                if (currentDisplayMatrix.Count > 0 && r < currentDisplayMatrix[0].Count)
                {
                    int firstSym = currentDisplayMatrix[0][r];
                    int matchLen = 1;
                    for (int c = 1; c < 5 && c < currentDisplayMatrix.Count; c++)
                    {
                        if (r < currentDisplayMatrix[c].Count && (currentDisplayMatrix[c][r] == firstSym || currentDisplayMatrix[c][r] == 1 || currentDisplayMatrix[c][r] == 16 || currentDisplayMatrix[c][r] == 17))
                        {
                            matchLen++;
                        }
                        else break;
                    }
                    if (matchLen >= 3)
                    {
                        var fallbackLine = new HashSet<int>();
                        for (int c = 0; c < matchLen; c++) fallbackLine.Add(r * 5 + c);
                        paylinePositionSets.Add(fallbackLine);
                        allWinPositions.UnionWith(fallbackLine);
                    }
                }
            }
            if (allWinPositions.Count == 0)
            {
                var defaultLine = new HashSet<int>();
                for (int c = 0; c < 3; c++) defaultLine.Add(1 * 5 + c);
                paylinePositionSets.Add(defaultLine);
                allWinPositions.UnionWith(defaultLine);
            }
        }

        Debug.Log($"[PlayWinPresentation] Phase 1: Highlighting {allWinPositions.Count} winning icons across {paylinePositionSets.Count} paylines");

        // Phase 1: Star boom, Total Win text, and all winning symbols highlighted simultaneously
        PlayStarAnimation();

        if (starToWinTextDelay > 0)
        {
            yield return new WaitForSeconds(starToWinTextDelay);
        }

        ShowPhase1TotalWin(totalWinAmount);
        AudioManager.Instance?.PlayWinLinePhase1Start();
        HighlightWinningPositions(allWinPositions);

        yield return new WaitForSeconds(winAnimationDuration);

        // Notify GameManager after Phase 1 so autoplay/free spins/popups/controls proceed normally
        onComplete?.Invoke();

        // Phase 2: Cycle through individual winning paylines indefinitely until next spin
        if (paylinePositionSets.Count > 1)
        {
            HidePhase1TotalWinText();
            while (true)
            {
                for (int i = 0; i < paylinePositionSets.Count; i++)
                {
                    HighlightWinningPositions(paylinePositionSets[i]);
                    yield return new WaitForSeconds(winSymbolLoopDuration);
                }
            }
        }
    }

    private void HighlightWinningPositions(HashSet<int> winPositions)
    {
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        // 1. Activate grey overlay so all non-winning reel icons behind are dimmed
        if (greyOverlay != null)
        {
            greyOverlay.SetActive(true);
        }

        // 2. Direct activation using cached components (0 lookups, 0 GetComponent calls)
        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row < rowLimit; row++)
            {
                int flatIndex = row * 5 + col;
                bool isWinning = winPositions != null && winPositions.Contains(flatIndex);

                var winImg = cachedWinSymbolImages[col, row];
                var frameAnim = cachedWinFrameAnims[col, row];

                if (isWinning)
                {
                    // A. Setup winning duplicate symbol: replace sprite FIRST, then turn ON
                    if (winImg != null)
                    {
                        int symbolId = (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
                            ? currentDisplayMatrix[col][row]
                            : 0;

                        winImg.sprite = GetSymbolSprite(symbolId);
                        winImg.color = Color.white;
                        winImg.raycastTarget = false;
                        winImg.gameObject.SetActive(true);
                    }

                    // B. Setup and play highlight frame animation
                    if (frameAnim != null)
                    {
                        frameAnim.gameObject.SetActive(true);
                        frameAnim.StartAnimation();
                    }
                }
                else
                {
                    // Non-winning: ensure overlay frame and symbol are disabled
                    if (frameAnim != null)
                    {
                        frameAnim.StopAnimation();
                        frameAnim.gameObject.SetActive(false);
                    }
                    if (winImg != null)
                    {
                        winImg.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void ShowWinLineTextOnIcon(int col, int row, double winAmount)
    {
        Image symbolImage = GetVisibleImage(col, row);
        if (symbolImage == null) return;

        Transform textTransform = symbolImage.transform.Find("WinLineText");
        if (textTransform != null)
        {
            var tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = winAmount.ToString("0.###");
            }
            AnimateTextScaleAppear(textTransform);
        }
    }

    private void HideAllWinLineTexts()
    {
        if (reelImagesList == null) return;
        foreach (var reel in reelImagesList)
        {
            if (reel.images != null)
            {
                foreach (var image in reel.images)
                {
                    if (image != null)
                    {
                        Transform textTransform = image.transform.Find("WinLineText");
                        if (textTransform != null)
                        {
                            textTransform.DOKill();
                            textTransform.localScale = Vector3.one;
                            textTransform.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void PlayStarAnimation()
    {
        if (starAnimationObject != null)
        {
            starAnimationObject.SetActive(true);
            if (cachedStarAnim != null)
            {
                cachedStarAnim.doLoopAnimation = false;
                cachedStarAnim.StartAnimation();
            }
        }
    }

    private void ShowPhase1TotalWin(double totalWinAmount)
    {
        if (phase1TotalWinText != null)
        {
            phase1TotalWinText.text = totalWinAmount.ToString("0.###");
            AnimateTextScaleAppear(phase1TotalWinText.transform);
        }
    }

    private void HidePhase1TotalWinText()
    {
        if (phase1TotalWinText != null)
        {
            phase1TotalWinText.transform.DOKill();
            phase1TotalWinText.transform.localScale = Vector3.one;
            phase1TotalWinText.gameObject.SetActive(false);
        }
    }

    private void AnimateTextScaleAppear(Transform textTransform, float popScale = 1.2f, float durationUp = 0.15f, float durationDown = 0.10f)
    {
        if (textTransform == null) return;
        textTransform.DOKill();
        textTransform.localScale = Vector3.zero;
        textTransform.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(textTransform.DOScale(popScale, durationUp).SetEase(Ease.OutQuad));
        seq.Append(textTransform.DOScale(1.0f, durationDown).SetEase(Ease.InQuad));
        winTweens.Add(seq);
    }
    private void EnableWinBox(int col, int row)
    {
        // No-op in Safari Ride: highlight frame is handled directly on each SlotIcon via AnimObject
    }

    private void DisableWinBox(int col, int row)
    {
        // No-op in Safari Ride: highlight frame is handled directly on each SlotIcon via AnimObject
    }

    private void ResetSymbolScale(int col, int row)
    {
        if (col >= 0 && col < 5 && row >= 0 && row < 3)
        {
            var frameAnim = cachedWinFrameAnims[col, row];
            if (frameAnim != null)
            {
                frameAnim.StopAnimation();
                frameAnim.gameObject.SetActive(false);
            }

            var winImg = cachedWinSymbolImages[col, row];
            if (winImg != null)
            {
                winImg.gameObject.SetActive(false);
            }
        }
    }


    private void AnimateWinSymbol(int column, int row)
    {
        Image symbolImage = GetVisibleImage(column, row);
        if (symbolImage == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Image out of range or null for col {column}, row {row}");
            return;
        }



        // Get the animation GameObject for this position
        var animGO = GetWinAnimationObject(column, row);
        if (animGO == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Animation GameObject is NULL at col: {column}, row: {row}");
            return;
        }

        // Get the ImageAnimation component
        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null)
        {
            Debug.LogError($"[AnimateWinSymbol] ImageAnimation component not found on animation object at col: {column}, row: {row}");
            return;
        }

        // Get the current symbol ID at this position
        if (column >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[column].Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid matrix position col: {column}, row: {row}");
            return;
        }

        int symbolId = currentDisplayMatrix[column][row];
        
        // Validate symbolId
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid symbolId {symbolId} at col: {column}, row: {row}");
            return;
        }

        // Get the animation sprite array for this symbol
        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0)
        {
            // Expected for most symbols now
            return;
        }

        // Set the sprite array on the ImageAnimation component
        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();
        
        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);
        });

        if (winLineBoxToAnimationDelay > 0)
        {
            seq.AppendInterval(winLineBoxToAnimationDelay);
        }

        seq.AppendCallback(() => {
            imageAnim.StartAnimation();
        });

        int loopCount = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) ? 1 : winSymbolLoopCount;
        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null && animGO.GetComponent<SymbolButtonHandler>() == null)
            {
                animGO.SetActive(false);
            }

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    private void KillWinTweens(bool stopCoroutine = true)
    {
        foreach (var tween in winTweens)
        {
            tween?.Kill();
        }
        winTweens.Clear();

        if (stopCoroutine && winAnimationCoroutine != null)
        {
            StopCoroutine(winAnimationCoroutine);
            winAnimationCoroutine = null;
        }

        DisableAllOverlays();

        // Restore all symbol image alphas to full opacity and ensure active
        if (reelImagesList != null)
        {
            foreach (var reel in reelImagesList)
            {
                if (reel?.images != null)
                {
                    foreach (var image in reel.images)
                    {
                        if (image != null)
                        {
                            image.gameObject.SetActive(true);
                            image.enabled = true;
                            image.DOKill();
                            image.transform.DOKill();
                            image.transform.localScale = Vector3.one;
                            Color c = image.color;
                            image.color = new Color(c.r, c.g, c.b, 1f);
                        }
                    }
                }
            }
        }
    }

    #endregion


    
    internal List<List<int>> GetCurrentDisplayMatrix()
    {
        return currentDisplayMatrix;
    }

    internal bool IsSpinning()
    {
        return isSpinning;
    }


    internal void KillAllTweens()
    {
        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
            spinRoutine = null;
        }

        foreach (var tween in spinTweens)
        {
            tween?.Kill();
        }
        spinTweens.Clear();

        KillWinTweens();

        if (reelRectTransforms != null)
        {
            for (int i = 0; i < reelRectTransforms.Length; i++)
            {
                if (reelRectTransforms[i] != null)
                {
                    reelRectTransforms[i].DOKill();
                    reelRectTransforms[i].anchoredPosition = new Vector2(reelRectTransforms[i].anchoredPosition.x, restingAnchoredY);
                }
            }
        }
    }

    #region Mystery Coin Animation

    private const int MaxMysteryCoins = 7;

    private void EnsureMysteryCoinPool()
    {
        if (mysteryCoinsContainer == null)
        {
            Debug.LogError("[SlotView] mysteryCoinsContainer is not assigned in the Inspector! Please assign it.");
            return;
        }

        if (mysteryCoinPrefab == null)
        {
            Debug.LogError("[SlotView] mysteryCoinPrefab is not assigned in the Inspector! Please assign it.");
            return;
        }

        if (pooledMysteryCoins == null)
        {
            pooledMysteryCoins = new List<GameObject>(MaxMysteryCoins);
        }

        while (pooledMysteryCoins.Count < MaxMysteryCoins)
        {
            GameObject coinObj = Instantiate(mysteryCoinPrefab, mysteryCoinsContainer);
            coinObj.SetActive(false);
            pooledMysteryCoins.Add(coinObj);
        }
    }

    internal void PlayMysteryCoinAnimation(WildCoinMysteryData mysteryData, System.Action onComplete)
    {
        if (mysteryData == null || !mysteryData.triggered || mysteryData.positions == null || mysteryData.positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (mysteryCoinsContainer == null || mysteryCoinPrefab == null)
        {
            Debug.LogError("[SlotView] Cannot play mystery coin animation: mysteryCoinsContainer or mysteryCoinPrefab is not assigned in the Inspector!");
            onComplete?.Invoke();
            return;
        }

        if (mysteryCoinCoroutine != null)
        {
            StopCoroutine(mysteryCoinCoroutine);
        }

        mysteryCoinCoroutine = StartCoroutine(ExecuteMysteryCoinFall(mysteryData, mysteryData.positions, onComplete));
    }

    private IEnumerator ExecuteMysteryCoinFall(WildCoinMysteryData mysteryData, List<ServerPosition> positions, System.Action onComplete)
    {
        EnsureMysteryCoinPool();

        Sequence masterSeq = DOTween.Sequence();
        int count = Mathf.Min(positions.Count, MaxMysteryCoins);

        for (int i = 0; i < count; i++)
        {
            var pos = positions[i];
            int col = pos.col;
            int row = pos.row;

            Image targetSymbolImage = GetVisibleImage(col, row);
            if (targetSymbolImage == null) continue;

            // Target position in mysteryCoinsContainer coordinates
            Vector3 targetWorld = targetSymbolImage.transform.position;
            Vector3 targetLocal = mysteryCoinsContainer.InverseTransformPoint(targetWorld);

            // Start position: out of screen above the top of the canvas (Y = 750f) aligned with target column
            Vector3 startLocal = new Vector3(targetLocal.x, 750f, targetLocal.z);

            // Determine replacement symbol from backend matrixAfter
            int replacementSymbolId = (mysteryData.matrixAfter != null && col < mysteryData.matrixAfter.Count && row < mysteryData.matrixAfter[col].Count)
                ? mysteryData.matrixAfter[col][row]
                : 1;

            Sprite replacementSprite = GetSymbolSprite(replacementSymbolId);

            GameObject coinObj = pooledMysteryCoins[i];
            if (coinObj == null) continue;

            coinObj.transform.localPosition = startLocal;
            coinObj.transform.localScale = Vector3.one;
            coinObj.SetActive(true);

            float delay = i * mysteryCoinStaggerDelay;
            float fallDuration = mysteryCoinFallDuration;

            Sequence coinSeq = DOTween.Sequence();
            if (delay > 0) coinSeq.AppendInterval(delay);

            // Smooth linear/ease-out trajectory so the falling coin is clearly visible on screen as it travels
            coinSeq.Append(coinObj.transform.DOLocalMove(targetLocal, fallDuration).SetEase(Ease.OutCubic));
            
            // Brief landing punch on the coin itself before cross-fading into the reel symbol
            coinSeq.Append(coinObj.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.15f, 3, 0.5f));
            coinSeq.AppendCallback(() =>
            {
                coinObj.SetActive(false);

                // Replace visible reel symbol sprite
                if (targetSymbolImage != null)
                {
                    targetSymbolImage.sprite = replacementSprite;
                    targetSymbolImage.color = Color.white;
                    targetSymbolImage.transform.DOKill();
                    targetSymbolImage.transform.localScale = Vector3.one;
                    targetSymbolImage.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.25f, 4, 0.5f);
                }

                // If WinSymbols overlay for this cell exists, also update its sprite so winning highlights reflect the coin
                if (cachedWinSymbolImages != null && col < 5 && row < 3 && cachedWinSymbolImages[col, row] != null)
                {
                    cachedWinSymbolImages[col, row].sprite = replacementSprite;
                }

                // Update display matrix so win lines & highlights reflect the replaced coins
                if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
                {
                    currentDisplayMatrix[col][row] = replacementSymbolId;
                }

                AudioManager.Instance?.PlayMysteryCoin();
            });

            masterSeq.Insert(0f, coinSeq);
        }

        winTweens.Add(masterSeq);
        yield return masterSeq.WaitForCompletion();

        // Brief settling pause after all coins have fallen
        yield return new WaitForSeconds(0.25f);

        mysteryCoinCoroutine = null;
        onComplete?.Invoke();
    }

    [ContextMenu("Test Mystery Coin Fall (3 Coins)")]
    public void TestMysteryCoinFall()
    {
        var testPositions = new List<ServerPosition>
        {
            new ServerPosition { col = 1, row = 1 },
            new ServerPosition { col = 2, row = 0 },
            new ServerPosition { col = 3, row = 2 }
        };
        var testData = new WildCoinMysteryData
        {
            triggered = true,
            coinCount = 3,
            positions = testPositions,
            matrixBefore = currentDisplayMatrix,
            matrixAfter = null
        };
        PlayMysteryCoinAnimation(testData, () => Debug.Log("[SlotView] Test Mystery Coin Fall (3 Coins) Complete!"));
    }

    [ContextMenu("Test Mystery Coin Fall (Random 2-7 Coins)")]
    public void TestMysteryCoinFallRandom()
    {
        int coinCount = UnityEngine.Random.Range(2, 8); // 2 to 7
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < 15; i++) availableIndices.Add(i);

        var testPositions = new List<ServerPosition>();
        for (int i = 0; i < coinCount; i++)
        {
            int rIdx = UnityEngine.Random.Range(0, availableIndices.Count);
            int flat = availableIndices[rIdx];
            availableIndices.RemoveAt(rIdx);

            int col = flat % 5;
            int row = flat / 5;
            testPositions.Add(new ServerPosition { col = col, row = row });
        }

        var testData = new WildCoinMysteryData
        {
            triggered = true,
            coinCount = coinCount,
            positions = testPositions,
            matrixBefore = currentDisplayMatrix,
            matrixAfter = null
        };
        PlayMysteryCoinAnimation(testData, () => Debug.Log($"[SlotView] Test Mystery Coin Fall Complete with {coinCount} random coins!"));
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion
}

[System.Serializable]
public class ReelImages
{
    public List<Image> images = new List<Image>(16);
}


[System.Serializable]
public class ColumnOverlays
{
    [Tooltip("Row 0 = top, Row 1, Row 2 = bottom")]
    public GameObject[] rows = new GameObject[3];
}