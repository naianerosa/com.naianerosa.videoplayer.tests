using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class EditorVideoPlayerElementTests
{
    private EditorVideoPlayerElement videoPlayerElement;
    private VideoPlaylist playlist1;
    private VideoPlaylist playlist2;

    [SetUp]
    public void Setup()
    {
        // Setup root element
        var template = EditorGUIUtility.Load("Packages/com.naianerosa.videoplayer/Editor/EditorVideoPlayerElement/EditorVideoPlayerElement.uxml") as VisualTreeAsset;

        var root = new VisualElement();

        template.CloneTree(root);

        videoPlayerElement = root.Q<EditorVideoPlayerElement>();
        videoPlayerElement.Init();

        // Load mock playlists
        playlist1 = AssetDatabase.LoadAssetAtPath<VideoPlaylist>("Packages/com.naianerosa.videoplayer.tests/Resources/MockPlaylist1.asset");
        playlist2 = AssetDatabase.LoadAssetAtPath<VideoPlaylist>("Packages/com.naianerosa.videoplayer.tests/Resources/MockPlaylist2.asset");
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void LoadPlaylist_HasCorrectNumberOfVideosAndState()
    {
        videoPlayerElement.LoadPlayList(playlist1);

        //Check items and buttons states
        Assert.IsNotNull(videoPlayerElement.viewModel);
        Assert.That(videoPlayerElement.viewModel.Videos.Count, Is.EqualTo(playlist1.Videos.Length));
        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 0);

        //Check containers visibility
        Assert.AreEqual(DisplayStyle.Flex, videoPlayerElement.viewModel.VideoContainerVisibility, "Videos container should be visible");
        Assert.AreEqual(DisplayStyle.None, videoPlayerElement.viewModel.NoVideosLabelVisibility, "No videos label should NOT be visible");

        //Change Playlist
       
        videoPlayerElement.LoadPlayList(playlist2);

        //Check items and buttons states
        Assert.IsNotNull(videoPlayerElement.viewModel);
        Assert.That(videoPlayerElement.viewModel.Videos.Count, Is.EqualTo(playlist2.Videos.Length));
        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 0);

        //Check containers visibility
        Assert.AreEqual(DisplayStyle.Flex, videoPlayerElement.viewModel.VideoContainerVisibility, "Videos container should be visible");
        Assert.AreEqual(DisplayStyle.None, videoPlayerElement.viewModel.NoVideosLabelVisibility, "No videos label should NOT be visible");

    }

    [Test]
    public void LoadPlaylist_WithEmptyPlaylist()
    {
        var emptyPlaylist = ScriptableObject.CreateInstance<VideoPlaylist>();

        videoPlayerElement.LoadPlayList(emptyPlaylist);

        Assert.IsNotNull(videoPlayerElement.viewModel, "ViewModel should not be null for an empty playlist.");
        Assert.AreEqual(DisplayStyle.Flex, videoPlayerElement.viewModel.NoVideosLabelVisibility, "No videos label should be visible");
        Assert.AreEqual(DisplayStyle.None, videoPlayerElement.viewModel.VideoContainerVisibility, "Videos container should not be visible");

        Object.DestroyImmediate(emptyPlaylist);
    }


    [Test]
    public void LoadPlaylist_WithNull()
    {
        videoPlayerElement.LoadPlayList(null);

        Assert.IsNotNull(videoPlayerElement.viewModel, "ViewModel should not be null for an empty playlist.");
        Assert.AreEqual(DisplayStyle.None, videoPlayerElement.viewModel.NoVideosLabelVisibility, "No videos label should not be visible");
        Assert.AreEqual(DisplayStyle.None, videoPlayerElement.viewModel.VideoContainerVisibility, "Videos container should not be visible");
    }

    [Test]
    public void Play_ButtonsAreOnCorrectState()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Pause();
        videoPlayerElement.Play();
        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 0);
    }

    [Test]
    public void Next_ButtonsAreOnCorrectState()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Next();

        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 1);
    }

    [Test]
    public void Next_MoreTimesThanVideos_GoesBackToFirst()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        for (int i = 1; i < playlist1.Videos.Length; i++)
        {
            videoPlayerElement.Next();
        }
        videoPlayerElement.Next(); //Once more, should go back to first video

        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 0);
    }

    [Test]
    public void Previous_ButtonsAreOnCorrectState()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Next(); // Move to second video
        videoPlayerElement.Previous(); // Move back to first

        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: 0);
    }

    [Test]
    public void Previous_OnFirstVideo_GoesBackToLast()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Previous(); // Move to last video

        AssertButtonStates(mainButtonPlaying: true, indexOfPlayingVideo: playlist1.Videos.Length - 1);
    }

    [Test]
    public void Stop_ButtonsAreOnCorrectState()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Stop();

        AssertButtonStates(mainButtonPlaying: false, indexOfPlayingVideo: -1);
    }

    [Test]
    public void Pause_ButtonsAreOnCorrectState()
    {
        videoPlayerElement.LoadPlayList(playlist1);
        videoPlayerElement.Pause();

        AssertButtonStates(mainButtonPlaying: false, indexOfPlayingVideo: -1);
    }

    private void AssertButtonStates(bool mainButtonPlaying, int indexOfPlayingVideo)
    {
        var playlist = videoPlayerElement.viewModel;
        var currentVideos = playlist.Videos;

        DisplayStyle expectedMainButtonPlay = mainButtonPlaying ? DisplayStyle.None : DisplayStyle.Flex;
        DisplayStyle expectedMainButtonPause = mainButtonPlaying ? DisplayStyle.Flex : DisplayStyle.None;

        Assert.AreEqual(expectedMainButtonPlay, playlist.PlayButtonVisibility);
        Assert.AreEqual(expectedMainButtonPause, playlist.PauseButtonVisibility);

        for (int i = 0; i < currentVideos.Count; i++)
        {
            DisplayStyle expectedPlay = (i == indexOfPlayingVideo) ? DisplayStyle.None : DisplayStyle.Flex;
            DisplayStyle expectedPause = (i == indexOfPlayingVideo) ? DisplayStyle.Flex : DisplayStyle.None;
            Assert.AreEqual(expectedPlay, currentVideos[i].PlayButtonVisibility, $"Wrong play visibility on video {i}");
            Assert.AreEqual(expectedPause, currentVideos[i].PauseButtonVisibility, $"Wrong pause visibility on video {i}");
        }
    }
}