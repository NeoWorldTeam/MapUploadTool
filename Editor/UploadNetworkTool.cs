using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Text;


public class UnityWebRequestException : Exception
{
    public UnityWebRequest WebRequest { get; private set; }

    public UnityWebRequestException(UnityWebRequest webRequest)
        : base($"UnityWebRequest error: {webRequest.error}")
    {
        WebRequest = webRequest;
    }
}


public static class UnityWebRequestAsyncOperationExtensions
{
    public static Task<UnityWebRequest> AsTask(this UnityWebRequestAsyncOperation asyncOperation)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();

        asyncOperation.completed += operation =>
        {
            if (asyncOperation.webRequest.result == UnityWebRequest.Result.Success)
            {
                tcs.SetResult(asyncOperation.webRequest);
            }
            else
            {
                tcs.SetException(new UnityWebRequestException(asyncOperation.webRequest));
            }
        };

        return tcs.Task;
    }
}

public class UploadNetworkTool
{
    public async Task<string> UploadFile(string filePath)
    {
        filePath = filePath.Trim();
        var url = "http://zingy.land/upload_file";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // formData.Add(new MultipartFormFileSection("file", bytes, filename, "application/octet-stream"));
        byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        string filename = System.IO.Path.GetFileName(filePath);
        formData.Add(new MultipartFormFileSection("file", bytes, filename, "application/octet-stream"));

        try
        {
            using (var request = UnityWebRequest.Post(url, formData))
            {
                await request.SendWebRequest().AsTask();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }

                if (request.downloadHandler.text == null)
                {
                    Debug.LogError("request.downloadHandler.text is null");
                    return null;
                }
                Debug.Log("Form upload complete!");
                Debug.Log(request.downloadHandler.text);

                //json data convert to UploadDTO
                UploadDTO uploadDTO = JsonUtility.FromJson<UploadDTO>(request.downloadHandler.text);
                if (uploadDTO == null)
                {
                    Debug.LogError("uploadDTO is null");
                    return null;
                }

                return uploadDTO.url;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("UploadFile error: {0}", ex.Message);
            return null;
        }
    }

    //set buildin
    public async Task<BuildingResponseDTO> PostBuilding(BuildingDTO buildingDTO)
    {
        try
        {
            var url = "http://zingy.land/set_building";
            string bodyData = JsonUtility.ToJson(buildingDTO);
            Debug.Log("PostBuilding json: " + bodyData);
            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");


                await request.SendWebRequest().AsTask();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }

                if (request.downloadHandler.text == null)
                {
                    Debug.LogError("request.downloadHandler.text is null");
                    return null;
                }
                Debug.Log("Form upload complete!");
                Debug.Log(request.downloadHandler.text);

                //json data convert to BuildingDTOResponse
                BuildingResponseDTO buildingResponseDTO = JsonUtility.FromJson<BuildingResponseDTO>(request.downloadHandler.text);
                if (buildingResponseDTO == null)
                {
                    Debug.LogError("buildingDTOResponse is null");
                    return null;
                }

                if (buildingResponseDTO.building_id != null)
                {
                    Debug.Log("buildingResponseDTO.building_id: " + buildingResponseDTO.building_id);
                }

                return buildingResponseDTO;
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("SetBuilding error");
            return null;
        }

    }

    //DownloadAssetBundle
    public async Task<AssetBundle> DownloadAssetBundle(string url)
    {
        try
        {
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                await request.SendWebRequest().AsTask();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }


                //json data convert to BuildingDTOResponse
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                if (assetBundle == null)
                {
                    Debug.LogError("assetBundle is null");
                    return null;
                }

                return assetBundle;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("DownloadAssetBundle error: {0}", ex.Message);
            return null;
        }
    }

    //PostMapData
    public async Task<bool> PostMapData(string mapId, MapDataDTO mapDataDTO)
    {
        try
        {
            string bodyData = JsonUtility.ToJson(mapDataDTO);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyData);

            var url = string.Format("http://zingy.land/map?id={0}", mapId);
            Debug.Log("PostMapData json: " + bodyData);
            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest().AsTask();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return false;
                }

                if (request.downloadHandler.text == null)
                {
                    Debug.LogError("request.downloadHandler.text is null");
                    return false;
                }

                Debug.Log("Form upload complete!");

                // APIResponse aPIResponse = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                // if (aPIResponse == null)
                // {
                //     Debug.LogError("aPIResponse is null");
                //     return false;
                // }

                // if (aPIResponse.status != 200)
                // {
                //     Debug.LogError("aPIResponse.status != 0");
                //     return false;
                // }

                return true;
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("PostMapData error");
            return false;
        }
    }

}