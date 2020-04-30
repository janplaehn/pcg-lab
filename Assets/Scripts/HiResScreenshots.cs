using UnityEngine;
using System;
using System.IO;
using UnityEditor;

public class HiResScreenshots : MonoBehaviour {
    [Header("Resolution")]
    public int resWidth = 500;
    public int resHeight = 500;

    [Header("Dependencies")]
    public Camera _camera = null;

    [Header("Input")]
    public KeyCode _screenshotKey = KeyCode.F2;
    private bool takeHiResShot = false;
    private int _testIndex = -1;

    public enum SaveLocation {
        Desktop,
        Documents,
        Pictures,
        ProjectPath,
        Appdata
    }

    [Header("Data")]
    public SaveLocation _saveLocation = SaveLocation.Pictures;
    public string _folderName = "Unity Screenshots";

    private void Awake() {
        FindCamera();
    }

    public string ScreenShotName(int width, int height) {
        if (_testIndex != -1) {
            return string.Format("{0}/screen_{1}.png",
                             GetSavePath(),
                             _testIndex);
        }
        return string.Format("{0}/screen_{1}x{2}_{3}.png",
                             GetSavePath(),
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void TakeHiResShot() {
        takeHiResShot = true;
    }

    public void TakeHiResShot(int testIndex) {
        takeHiResShot = true;
        _testIndex = testIndex;
        LateUpdate();
    }

    void LateUpdate() {
        takeHiResShot |= Input.GetKeyDown(_screenshotKey);
        if (takeHiResShot) {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            _camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            _camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            _camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("<color=#08320e> Saved Screenshot:</color> {0}", filename));
            takeHiResShot = false;
        }
    }

    private void Reset() {
        FindCamera();
        SetDefaultFolderPath();
    }

    private void SetDefaultFolderPath() {
        _folderName = PlayerSettings.productName + " Screenshots";
    }

    private void FindCamera() {
        if (!_camera) {
            _camera = Camera.main;
        }
    }

    public string GetSavePath() {
        string savePath = "";
        switch (_saveLocation) {
            case SaveLocation.Desktop:
                savePath += Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                break;
            case SaveLocation.Documents:
                savePath += Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                break;
            case SaveLocation.Pictures:
                savePath += Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                break;
            case SaveLocation.ProjectPath:
                savePath += Application.dataPath;
                break;
            case SaveLocation.Appdata:
                savePath += Application.persistentDataPath;
                break;
            default:
                Debug.LogWarning("Unknown Save Path set!");
                break;
        }
        if (_folderName != "") {
            savePath += "/" + _folderName;
        }
        if (!Directory.Exists(savePath)) {
            Directory.CreateDirectory(savePath);
        }
        return savePath;
    }
}