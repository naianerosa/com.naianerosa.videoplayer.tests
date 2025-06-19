using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class VideoPlayerEditorWindowTests
{
    private VideoPlayerEditorWindow window;
    private VideoPlaylist mockPlaylist1;
    private VideoPlaylist mockPlaylist2;

    [SetUp]
    public void Setup()
    {
        VideoPlayerEditorWindow.NewWindow();
        window = EditorWindow.GetWindow<VideoPlayerEditorWindow>();

        mockPlaylist1 = AssetDatabase.LoadAssetAtPath<VideoPlaylist>("Packages/com.naianerosa.videoplayer.tests/Resources/MockPlaylist1.asset");
        mockPlaylist2 = AssetDatabase.LoadAssetAtPath<VideoPlaylist>("Packages/com.naianerosa.videoplayer.tests/Resources/MockPlaylist2.asset");

    }

    [TearDown]
    public void TearDown()
    {
        if (window != null)
        {
            window.Close();
        }
    }

    [Test]
    public void NewWindow_CreatesWindowWithCorrectInitialState()
    {
        Assert.That(window.titleContent.text, Is.EqualTo("Video Player"));
        Assert.That(window.ViewModel, Is.Not.Null);
        Assert.AreEqual(DisplayStyle.Flex, window.ViewModel.NoPlayListSelectedContainer);
    }

    [Test]
    public void CreateGUI_ContainsRequiredComponents()
    {

        var root = window.rootVisualElement;
        var videoDisplay = root.Q<IMGUIContainer>("video-display");
        var playlistPicker = root.Q<ObjectField>("playlist_picker");

        Assert.That(videoDisplay, Is.Not.Null);
        Assert.That(playlistPicker, Is.Not.Null);
    }

    [Test]
    public void OnDisable_CleansUpVideoPlayerComponent()
    {
        window.Close();
        var videoPlayerGO = GameObject.Find("EditorVideoPlayer");
        Assert.That(videoPlayerGO, Is.Null, $"Video Player GameObject should be destroyed on window close.");
    }

    [Test]
    public void PlaylistPicker_LoadsPlaylistWhenValueChanged()
    {
        var root = window.rootVisualElement;
        var playlistPicker = root.Q<ObjectField>("playlist_picker");
        var videoPlayerElement = root.Q<EditorVideoPlayerElement>();

        //Sets the first playlist to the picker        
        playlistPicker.value = mockPlaylist1;

        // Verify root element has data source set and title matches
        Assert.That(window.EditorVideoPlayerElement.dataSource, Is.Not.Null);
        Assert.AreEqual(mockPlaylist1.Title, ((EditorVideoPlayerElementVM)(videoPlayerElement.dataSource)).Title);
        Assert.AreEqual(DisplayStyle.None, window.ViewModel.NoPlayListSelectedContainer);

        //Sets the second playlist to the picker       
        playlistPicker.value = mockPlaylist2;

        // Verify root element has data source set and title matches    
        Assert.That(window.EditorVideoPlayerElement.dataSource, Is.Not.Null);
        Assert.AreEqual(mockPlaylist2.Title, ((EditorVideoPlayerElementVM)(videoPlayerElement.dataSource)).Title);
        Assert.AreEqual(DisplayStyle.None, window.ViewModel.NoPlayListSelectedContainer);
    }



    [Test]
    public void PlaylistPicker_ClearPlayList_ShowNoPlaylistLabel()
    {
        var root = window.rootVisualElement;
        var playlistPicker = root.Q<ObjectField>("playlist_picker");

        //Sets a playlist to the picker        
        playlistPicker.value = mockPlaylist1;
        Assert.AreEqual(DisplayStyle.None, window.ViewModel.NoPlayListSelectedContainer);


        //Clear the playlist on the picker        
        playlistPicker.value = null;

        // Verify view model is set and is on correct state
        Assert.That(window.ViewModel, Is.Not.Null);
        Assert.AreEqual(DisplayStyle.Flex, window.ViewModel.NoPlayListSelectedContainer);

    }
}