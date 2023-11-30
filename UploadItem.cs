using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UploadItem : MonoBehaviour
{
    public enum ItemTypeEnum
    {
        Building,
        Background,
    }
    public enum TagIdEnum
    {
        Tag_1001 = 1001,
        Tag_1002 = 1002,
        Tag_1003 = 1003,
        Tag_1004 = 1004,
        Tag_1005 = 1005,
        Tag_1006 = 1006,
        Tag_1007 = 1007,
        Tag_1008 = 1008,
        Tag_1009 = 1009,

        Tag_1010 = 1010,
        Tag_1011 = 1011,
        Tag_1012 = 1012,
        Tag_1013 = 1013,
        Tag_1014 = 1014,

        Tag_1015 = 1015,
        Tag_1016 = 1016,
        Tag_1017 = 1017,
        Tag_1018 = 1018,
        Tag_1019 = 1019,
        
        Tag_1020 = 1020,
        Tag_1021 = 1021,
        Tag_1022 = 1022,
        Tag_1023 = 1023,
        Tag_1024 = 1024,
        Tag_1025 = 1025,
        Tag_1026 = 1026,
    }

    [SerializeField]
    public ItemTypeEnum itemEnum = ItemTypeEnum.Building;

    [SerializeField]
    public TagIdEnum tagIdEnum = TagIdEnum.Tag_1001;



    [SerializeField] public string videoFilePath;

    [SerializeField] public string buildingTypeId;
    [SerializeField] public string effectTypeId;
    [SerializeField] public string functionTypeId;
    [SerializeField] public string funciton;

}   
