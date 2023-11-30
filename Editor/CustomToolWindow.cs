using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;


struct UploadingResult
{
    public bool success;
    public string error;

    public UploadingResult(bool success, string error)
    {
        this.success = success;
        this.error = error;
    }

    public bool HasError()
    {
        return !string.IsNullOrEmpty(error);
    }
}

enum RunningState
{
    Idle,
    Init,
    InitFail,
    InitSuccess,
    Uploading,
    UploadSuccess,
    UploadFail,
}

public class CustomToolWindow : EditorWindow
{
    private string displayStatusText = "";

    private string officeMapId = "1";

    private bool allowUpload = false;

    private RunningState state = RunningState.Idle;

    private UploadItem[] buildingUploadItems;
    private UploadItem[] backgourndUploadItems;

    private TagItem[] tagItemsCache;

    private List<BuildingItem> buildingItemsCache = new List<BuildingItem>();

    private int uploadCount = 0;
    

    [MenuItem("Tools/地图上传工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<CustomToolWindow>("地图上传工具");
        window.StartInitAsync();
    }

    private void OnGUI()
    {
        GUILayout.Label("输入参数", EditorStyles.boldLabel);
        GUILayout.Space(10);
        officeMapId = EditorGUILayout.TextField("官方地图ID", officeMapId);
        GUILayout.Space(10);

        GUILayout.Label("状态", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label(displayStatusText, EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (allowUpload && GUILayout.Button("同步地图"))
        {
            UploadFile();
        }
    }

    private void UploadFile()
    {
        StartUploadAsync();
    }

    private async void StartInitAsync()
    {
        EditorApplication.update += EditorUpdate;
        await StartInitTask();
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (state == RunningState.Init)
        {
            displayStatusText = "初始化中....";
            allowUpload = false;
            state = RunningState.Idle;
        }
        else if (state == RunningState.InitFail)
        {
            displayStatusText = "初始化失败";
            allowUpload = false;
            state = RunningState.Idle;
        }
        else if (state == RunningState.InitSuccess)
        {
            displayStatusText = "检测到建筑数量:" + buildingUploadItems.Length + ", 背景数量:" + backgourndUploadItems.Length;
            allowUpload = true;
            state = RunningState.Idle;
        }else if (state == RunningState.Uploading)
        {
            displayStatusText = "上传中...." + uploadCount + "/" + buildingUploadItems.Length;
            allowUpload = false;
        }
        else if (state == RunningState.UploadSuccess)
        {
            displayStatusText = "上传成功";
            allowUpload = false;
            state = RunningState.Idle;
        }
        else if (state == RunningState.UploadFail)
        {
            displayStatusText = "上传失败";
            allowUpload = true;
            state = RunningState.Idle;
        }




        
    }



    private async Task StartInitTask()
    {
        // 在这里执行你的任务
        Debug.Log("初始化开始");
        state = RunningState.Init;
        


        //load conimgs_output.json from resources
        TextAsset textAsset = Resources.Load<TextAsset>("conimgs_output");
        if (textAsset == null)
        {
            Debug.LogError("conimgs_output.json is null");
            state = RunningState.InitFail;
            return;
        }
        await Task.Yield();

        //parse json
        // Debug.Log("textAsset.text: " + textAsset.text);
        TagItemDTOList tagItemDTOList = JsonUtility.FromJson<TagItemDTOList>(textAsset.text);
        if (tagItemDTOList == null)
        {
            Debug.LogError("tagItemDTOList is null");
            state = RunningState.InitFail;
            return;
        }
        await Task.Yield();

        //convert to TagItem
        tagItemsCache = System.Array.ConvertAll(tagItemDTOList.data, item => new TagItem(item));
        if (tagItemsCache.Length == 0)
        {
            Debug.LogError("tagItemsCache is empty");
            state = RunningState.InitFail;
            return;
        }
        await Task.Yield();



        UploadItem[] uploadItems = GameObject.FindObjectsOfType<UploadItem>();
        await Task.Yield();

        buildingUploadItems = System.Array.FindAll(uploadItems, item => item.itemEnum == UploadItem.ItemTypeEnum.Building);
        await Task.Yield();

        backgourndUploadItems = System.Array.FindAll(uploadItems, item => item.itemEnum == UploadItem.ItemTypeEnum.Background);
        await Task.Yield();

        Debug.Log("初始化结束");
        state = RunningState.InitSuccess;
        await Task.Yield();
    }

    private async void StartUploadAsync()
    {
        EditorApplication.update += EditorUpdate;
        await UploadTaskAsync();
        await Task.Yield();
        await Task.Yield();
        await Task.Yield();
        EditorApplication.update -= EditorUpdate;
    }

    private async Task UploadTaskAsync()
    {
        state = RunningState.Uploading;
        //初始化
        UploadNetworkTool uploadNetworkTool = new UploadNetworkTool();
        GridCoordinate gridCoordinate = new GridCoordinate();
        uploadCount = 0;
        buildingItemsCache.Clear();

        

        Debug.Log("任务开始");
        string tempFolderPath = Path.Combine(Application.temporaryCachePath, "Temp");
        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }
        else
        {
            Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);
        }
        await Task.Yield();





        foreach (var uploadItem in buildingUploadItems)
        {   
            uploadCount++;
            //get tagIdEnum raw int value
            string tagIdName = string.Format("{0}", (int)uploadItem.tagIdEnum);
            //定位tag
            TagItem tagItem = System.Array.Find(tagItemsCache, item => item.tagid == tagIdName);
            if (tagItem.tagid == null)
            {
                Debug.LogError("tagItem.tagid is null");
                state = RunningState.UploadFail;
                return;
            }
            
            //保存纹理为图片
            string imageFilePath = SaveTexturesAsImagesAsync(tempFolderPath, uploadItem);
            if (string.IsNullOrEmpty(imageFilePath))
            {
                Debug.LogError("imageFilePath is null");
                state = RunningState.UploadFail;
                return;
            }
            await Task.Yield();

            //上传图片
            string uploadURL = await uploadNetworkTool.UploadFile(imageFilePath);
            if (string.IsNullOrEmpty(uploadURL))
            {
                Debug.LogError("uploadURL is null");
                state = RunningState.UploadFail;
                return;
            }

            //判断用户是否有视频，如果有视频，上传视频
            string videoUploadURL = null;
            if (!string.IsNullOrEmpty(uploadItem.videoFilePath))
            {
                videoUploadURL = await uploadNetworkTool.UploadFile(uploadItem.videoFilePath);
                if (string.IsNullOrEmpty(videoUploadURL))
                {
                    Debug.LogError("videoUploadURL is null");
                    state = RunningState.UploadFail;
                    return;
                }
            }
            
            //生成建筑
            BuildingDTO buildingDTO = new BuildingDTO() {
                user_id = officeMapId,
                buildingTypeId = uploadItem.buildingTypeId,
                effectTypeId = uploadItem.effectTypeId,
                functionTypeId = uploadItem.functionTypeId,
                funciton = uploadItem.funciton,
                src_url = videoUploadURL,
                sd_gen_data = new SDGenData() {
                    url = uploadURL,
                    vertex = new float[] { tagItem.minX, tagItem.maxX, tagItem.minY, tagItem.maxY},
                    video_url = videoUploadURL,
                }
            };

            BuildingResponseDTO buildingDTOResponse = await uploadNetworkTool.PostBuilding(buildingDTO);
            if (buildingDTOResponse == null)
            {
                Debug.LogError("buildingDTOResponse is null");
                state = RunningState.UploadFail;
                return;
            }

            //get bounds
            Bounds bounds = uploadItem.GetComponent<SpriteRenderer>().bounds;
            float centerX = (bounds.min.x + bounds.max.x) / 2;
            float lowerThirdY = bounds.min.y + (bounds.max.y - bounds.min.y) / 3;
            Vector3 targetPosition = new Vector3(centerX, lowerThirdY, 0);
            //get grid position
            Vector2Int gridPosition = gridCoordinate.GetGridPosition(targetPosition);
            Debug.Log("gridPosition: " + gridPosition);
            buildingItemsCache.Add(new BuildingItem(buildingDTOResponse.building_id, gridPosition.x, gridPosition.y));


            
            await Task.Delay(500);
        }

        await Task.Yield();

        //上传地图背景
        if (backgourndUploadItems == null || backgourndUploadItems.Length == 0)
        {
            Debug.LogError("backgourndUploadItems is empty");
            state = RunningState.UploadFail;
            return;
        }

        UploadItem backgourndUploadItem = backgourndUploadItems[0];
        string backgroundUrl = await UploadMapBackground(uploadNetworkTool, backgourndUploadItem, tempFolderPath);
        if (backgroundUrl == null)
        {
            Debug.LogError("backgroundUrl is null");
            state = RunningState.UploadFail;
            return;
        }   


        //生成地图
        MapBackgroundItem mapBackgroundItem = new MapBackgroundItem() {
            url = backgroundUrl,
            positionX = backgourndUploadItem.transform.position.x,
            positionY = backgourndUploadItem.transform.position.y,
            scaleX = backgourndUploadItem.transform.localScale.x,
            scaleY = backgourndUploadItem.transform.localScale.y,
            pixelsPerUnit = backgourndUploadItem.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit,
        };
        MapDataDTO mapDataDTO = new MapDataDTO() {
            building = buildingItemsCache,
            background = mapBackgroundItem,
        };

        bool postMapResult = await uploadNetworkTool.PostMapData(officeMapId, mapDataDTO);
        if (!postMapResult)
        {
            Debug.LogError("postMapResult is false");
            state = RunningState.UploadFail;
            return;
        }

        state = RunningState.UploadSuccess;
        await Task.Yield();


        Debug.Log("任务结束");
        //test

        // await Task.Delay(5000);
        // AssetBundle assetBundle = await uploadNetworkTool.DownloadAssetBundle(backgroundUrl);
        // if (assetBundle == null)
        // {
        //     Debug.LogError("assetBundle is null");
        //     state = RunningState.UploadFail;
        //     return ;
        // }
        // GameObject backgroundPrefab = assetBundle.LoadAsset<GameObject>("Background");
        // if (backgroundPrefab == null)
        // {
        //     Debug.LogError("backgroundPrefab is null");
        //     state = RunningState.UploadFail;
        //     return ;
        // }
        // GameObject testObject = Instantiate(backgroundPrefab);
        // testObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // await Task.Delay(50000);

        // DestroyImmediate(testObject);
        // assetBundle.Unload(true);
    }

    private string SaveGameObjectAsPrefab(GameObject objectToSave, string prefabDirectory) {
        if (objectToSave == null)
        {
            Debug.LogError("No object selected to save as prefab.");
            return null;
        }

        if (string.IsNullOrEmpty(prefabDirectory))
        {
            Debug.LogError("Invalid prefab path.");
            return null;
        }

        if (!Directory.Exists(prefabDirectory)) 
        {
            Directory.CreateDirectory(prefabDirectory);
        }
        string prefabName = "Background.prefab";
        string prefabPath = Path.Combine(prefabDirectory, prefabName);
        
        

        GameObject objectToSaveCopy = Instantiate(objectToSave);
        UploadItem UploadItemCompenent = objectToSaveCopy.GetComponent<UploadItem>();
        if (UploadItemCompenent != null) DestroyImmediate(UploadItemCompenent, true);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(objectToSaveCopy, prefabPath);

        DestroyImmediate(objectToSaveCopy, true);
        
        if (savedPrefab != null)
        {
            Debug.Log("Prefab saved successfully at: " + prefabPath);
            return prefabPath;
        }
        else
        {
            Debug.LogError("Failed to save prefab.");
            return null;
        }
    }

    private async Task<string> UploadMapBackground(UploadNetworkTool uploadNetworkTool, UploadItem uploadItem, string tempFolderPath) {
        string prefabDirectory = "Assets/MapUploadTool/Prefabs/";
        string prefabPath = SaveGameObjectAsPrefab(uploadItem.gameObject, prefabDirectory);
        if (prefabPath == null) {
            Debug.LogError("isSavedPrefab is false");
            state = RunningState.UploadFail;
            return null;
        }
        await Task.Yield();


        //把背景对象 设置标签和名字，然后打包成 ios平台的ab包
        string backgroundABFolder = Path.Combine(tempFolderPath, "AssetBundles");
        string backgroundABName = "Background.assetbundle";
        if (!Directory.Exists(backgroundABFolder))
        {
            Directory.CreateDirectory(backgroundABFolder);
        }



        try
        {
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = backgroundABName;
            buildMap[0].assetNames = new string[] {prefabPath  };
            BuildPipeline.BuildAssetBundles(backgroundABFolder, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("BuildAssetBundles error: {0}", ex.Message);
            state = RunningState.UploadFail;
            return null;
        }
        await Task.Yield();

        


        //上传文件
        string backgroundABPath = Path.Combine(backgroundABFolder, backgroundABName);
        string uploadURL = await uploadNetworkTool.UploadFile(backgroundABPath);
        if (string.IsNullOrEmpty(uploadURL))
        {
            Debug.LogError("uploadURL is null");
            state = RunningState.UploadFail;
            return null;
        }


        return uploadURL;
    }

    private string SaveTexturesAsImagesAsync(string tempFolderPath, UploadItem uploadItem, bool checkTextureSize = true) {


        SpriteRenderer spriteRenderer = uploadItem.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            Debug.LogError("SpriteRenderer is null");
            return null;
        }
        Sprite sprite = spriteRenderer.sprite;
        if (sprite == null) {
            Debug.LogError("Sprite is null");
            return null;
        }

        Texture2D texture = sprite.texture;
        if (texture == null) {
            Debug.LogError("Texture is null");
            return null;
        }

        if (checkTextureSize && (texture.width != 1024 || texture.height != 1024)) {
            Debug.LogError("Texture size is not 1024x1024");
            return null;
        }

        byte[] bytes = texture.EncodeToPNG();
        string fileName = uploadItem.gameObject.name.Replace("-", "_");
        string path = Path.Combine(tempFolderPath, fileName + ".png");
        File.WriteAllBytes(path, bytes);

        return path;
    }
}