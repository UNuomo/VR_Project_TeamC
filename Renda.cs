using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class Renda : MonoBehaviour
{
    [Header("オーディオ設定")]
    [Tooltip("RD.mp3ファイルを直接ドラッグ＆ドロップしてください。ドラッグ＆ドロップしない場合、スクリプトは自動的に同じディレクトリからファイルを読み込もうとします。")]
    public AudioClip audioClip;

    [Header("連続クリック検出設定")]
    //[Tooltip("需要的连续点击次数，默认为2（双击）")]
    //public int requiredClickCount = 2;
    [Tooltip("有効な連続クリック間の最大間隔（秒）")]
    public float clickTimeThreshold = 0.5f;
    [Tooltip("応答しない期間（ブルートフォース攻撃を防ぐため）")]
    public float clickTimeThreshold2 = 0.1f;
    [Header("ファイル名")]
    public string audioFileName = "RD.mp3";

    private AudioSource audioSource;
    private float lastClickTime;
    private int currentClickCount;
    private float timeSinceLastClick;

    private void Start()
    {
        // AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        // Inspector 没拖资源时自动加载
        if (audioClip == null)
        {
            StartCoroutine(LoadAudioFromLocal());
        }
        else
        {
            audioSource.clip = audioClip;
        }

        currentClickCount = 0;
    }

    IEnumerator LoadAudioFromLocal()
    {
        string path = Path.Combine(Application.streamingAssetsPath,audioFileName);
        Debug.Log("尝试加载音频：" + path);
        #if UNITY_ANDROID && !UNITY_EDITOR
            // Quest / Android
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
        #else
        // Windows / Editor
        string url = "file://" + path;
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        #endif

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            audioClip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.clip = audioClip;

            Debug.Log("音频加载成功："+ audioFileName);
        }
        else
        {
            Debug.LogError("音频加载失败："+ request.error);
        }

        request.Dispose();
    }

    private void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            if (currentClickCount == 0)
            {
                //检测到玩家第一次点击鼠标，记录当前时间
                lastClickTime = Time.time;
                currentClickCount++;
                ReplayAudio();
            }
            timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= clickTimeThreshold)
            {
                // 有效连续点击
                if(timeSinceLastClick >= clickTimeThreshold2)
                {
                    lastClickTime = Time.time;
                    ReplayAudio();
                }
                else
                {
                    //不应期，防止鼠标暴力点击
                }
            }
            else
            {
                // 间隔过长，重置计数器
                currentClickCount = 0;
            }
        }
    }

    /// <summary>
    /// 重新播放音频（从头开始）
    /// </summary>
    private void ReplayAudio()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.Stop();
            audioSource.Play();
            Debug.Log("重新播放音频：" + audioClip.name);
        }
        else
        {
            Debug.LogWarning("AudioSource 或 AudioClip 为空，无法播放");
        }
    }
}
