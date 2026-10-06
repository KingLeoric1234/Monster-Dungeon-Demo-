using UnityEngine;
using System.Collections;

public class SpriteFrameAnimation : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private Sprite[] frames;       // 拖入拆好的图片序列
    [SerializeField] private float frameRate = 10f; // 每秒多少帧（10=1秒播10张）
    [SerializeField] private bool loop = true;       // 是否循环播放
    [SerializeField] private bool playOnStart = true;// 启动时自动播放

    [Header("引用")]
    [SerializeField] private SpriteRenderer spriteRenderer;  // 怪物的SpriteRenderer

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (playOnStart && frames != null && frames.Length > 0)
            Play();
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameInterval = 1f / frameRate;

        if (timer >= frameInterval)
        {
            timer -= frameInterval;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;  // 循环，回到第一帧
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    Stop();  // 不循环，播完停在最后一帧
                    return;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    // ===== 公开方法（其他脚本调用）=====

    /// <summary>
    /// 播放动画
    /// </summary>
    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        isPlaying = true;
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    /// <summary>
    /// 切换到新的动画序列（比如从待机切到移动）
    /// </summary>
    public void SetFrames(Sprite[] newFrames, float newFrameRate = 10f, bool newLoop = true)
    {
        frames = newFrames;
        frameRate = newFrameRate;
        loop = newLoop;
        Play();
    }

    /// <summary>
    /// 当前是否在播放
    /// </summary>
    public bool IsPlaying => isPlaying;
}

