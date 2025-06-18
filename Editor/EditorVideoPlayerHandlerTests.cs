using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class EditorVideoPlayerHandlerTest
{
    private EditorVideoPlayerHandler videoHandler;
    private IMGUIContainer videoDisplay;
    private string videoPath = "Packages/com.naianerosa.videoplayer.tests/Resources/ARCutdown.webm";
    private string videoPath2 = "Packages/com.naianerosa.videoplayer.tests/Resources/CreativeCore.webm";
    private VideoPlayer videoPlayerComponent;

    [SetUp]
    public void Setup()
    {
        videoDisplay = new IMGUIContainer();
        videoDisplay.style.width = 512;
        videoDisplay.style.height = 288;
        
        videoHandler = new EditorVideoPlayerHandler(videoDisplay);

        videoPlayerComponent = GameObject.Find(EditorVideoPlayerHandler.VideoPlayerName)?.GetComponent<VideoPlayer>();
    }

    [TearDown]
    public void TearDown()
    {
        if (videoHandler != null)
        {
            videoHandler.Destroy();
            videoHandler = null;
        }
    }

    [Test]
    public void Constructor_CreatesVideoPlayerWithCorrectSetup()
    {
        Assert.That(videoPlayerComponent, Is.Not.Null, $"{EditorVideoPlayerHandler.VideoPlayerName} game object not found.");
        Assert.That(videoPlayerComponent.renderMode, Is.EqualTo(VideoRenderMode.RenderTexture));
        Assert.That(videoPlayerComponent.audioOutputMode, Is.EqualTo(VideoAudioOutputMode.AudioSource));
        Assert.That(videoPlayerComponent.gameObject.GetComponent<AudioSource>(), Is.Not.Null, "Audio source not found on video player.");
    }

    [Test]
    public void LoadNewVideo_SetsCorrectVideoPath()
    {
        videoHandler.LoadNewVideo(videoPath);
        Assert.That(videoPlayerComponent.url, Is.EqualTo(videoPath));
    }

    [Test]
    public async Task PlayVideo_WithNewPath_LoadsAndPlaysVideo()
    {
        videoHandler.PlayVideo(videoPath);
        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.That(videoPlayerComponent.url, Is.EqualTo(videoPath));
        Assert.That(videoPlayerComponent.isPlaying, Is.True);
      

        videoHandler.PlayVideo(videoPath2);
        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.That(videoPlayerComponent.url, Is.EqualTo(videoPath2));
        Assert.That(videoPlayerComponent.isPlaying, Is.True);
    }

    [Test]
    public async Task Pause_StoresCurrentFrame()
    {
        videoHandler.PlayVideo(videoPath);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.That(videoPlayerComponent.isPlaying, Is.True);

        var frameOnPause = videoPlayerComponent.frame;

        videoHandler.Pause();
        Assert.That(videoPlayerComponent.isPlaying, Is.False);

        await Task.Delay(TimeSpan.FromSeconds(1));
        videoHandler.PlayVideo(videoPath);
        long frameAfterResume = videoPlayerComponent.frame;

        Debug.Log($"Frame on pause: {frameOnPause}, Frame after resume: {frameAfterResume}");
        Assert.IsTrue(frameAfterResume >= frameOnPause && frameAfterResume < frameOnPause + 10,
            $"Video did not resume from the correct frame ({frameOnPause}) after pause. Current Frame:{videoPlayerComponent.frame} ");
               
    }

    [Test]
    public async Task StopVideo_RestartVideo()
    {
        videoHandler.PlayVideo(videoPath);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.That(videoPlayerComponent.isPlaying, Is.True);

        videoHandler.StopVideo();
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.That(videoPlayerComponent.isPlaying, Is.False);
        Assert.That(videoPlayerComponent.frame, Is.EqualTo(-1));
    }

    [Test]
    public void Destroy_CleansUpResources()
    {
        videoHandler.Destroy();
        var videoPlayer = GameObject.Find(EditorVideoPlayerHandler.VideoPlayerName);
        Assert.That(videoPlayer, Is.Null);
    }

    [Test]
    public void DrawVideoFrame_HandlesMissingRenderTexture()
    {
        videoHandler.Destroy();
        Assert.DoesNotThrow(() => videoHandler.DrawVideoFrame());
    }
}