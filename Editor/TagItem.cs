


using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TagItemDTO {
    public string tagid;
    public string up;
    public string bottom;
    public string left;
    public string right;
}

[System.Serializable]
public class TagItemDTOList {
    public TagItemDTO[] data;
}


[System.Serializable]
public class UploadDTO {
    public string url;
}

//sd_gen_data
[System.Serializable]
public class SDGenData {
    public string url;
    public float[] vertex;
    public string video_url;
}

[System.Serializable]
public class BuildingDTO {
    public string user_id;
    public string buildingTypeId;
    public string effectTypeId;
    public string functionTypeId;
    public string funciton;
    public string src_type = "video";
    public string src_url;

    public SDGenData sd_gen_data;
}

[System.Serializable]
public class BuildingResponseDTO : BuildingDTO {
    public string building_id;
}


// [System.Serializable]
// public class BuildingResponse {
//     public BuildingResponseDTO data;
//     public int status;
// }

// [System.Serializable]
// public class APIResponse {
//     public int status;
// }

[System.Serializable]
public class TagItem
{
    public string tagid;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public TagItem(TagItemDTO tagItemDTO) {
        tagid = tagItemDTO.tagid;
        float[] leftPoint = tagItemDTO.left.Split(',').Select(float.Parse).ToArray();
        minX = leftPoint[0];

        float[] rightPoint = tagItemDTO.right.Split(',').Select(float.Parse).ToArray();
        maxX = rightPoint[0];

        float[] upPoint = tagItemDTO.up.Split(',').Select(float.Parse).ToArray();
        maxY = upPoint[1];

        float[] bottomPoint = tagItemDTO.bottom.Split(',').Select(float.Parse).ToArray();
        minY = bottomPoint[1];

    }
}

[System.Serializable]
public class BuildingItem {
    public string id;
    public int gridPosX;
    public int gridPosY;
    public int multiplyingFactor = 1;
    public int sampleIndex = -1;
    public int type = -1;

    public BuildingItem(string id, int gridPosX, int gridPosY) {
        this.id = id;
        this.gridPosX = gridPosX;
        this.gridPosY = gridPosY;
    }
}

[System.Serializable]
public class MapBackgroundItem {
    public string url;
    public float positionX;
    public float positionY;
    public float scaleX;
    public float scaleY;

    public float pixelsPerUnit;
}

[System.Serializable]
public class MapDataDTO {
    public List<BuildingItem> building;
    public MapBackgroundItem background;
}