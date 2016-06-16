using System;

namespace OldW.GlobalSettings
{
    /// <summary>
    /// OldW程序中所用到的一些全局性的常数
    /// </summary>
    /// <remarks></remarks>
    public static class Constants
    {
        /// <summary>
        /// 整个程序的标志性名称
        /// </summary>
        /// <remarks></remarks>
        public const string AppName = "OldW";

        #region    ---   程序中的各种族文件的名称

        /// <summary>
        /// 基坑中的土体族文件，此土体族在整个模型中只有一个。
        /// </summary>
        /// <remarks></remarks>
        public const string FamilyName_Soil = "基坑土体";

        /// <summary>
        /// 用来模拟开挖的分块土体的族
        /// </summary>
        /// <remarks></remarks>
        public const string FamilyName_SoilRemove = "开挖土体";

        #endregion

        #region    ---   程序中的各种族样板文件的名称

        /// <summary>
        /// 基坑中的土体族的族样板文件，用来创建基坑中的土体，以及用来模拟开挖的分块小土体。
        /// </summary>
        /// <remarks></remarks>
        public const string FamilyTemplateName_Soil = "公制常规模型.rft";

        #endregion

        #region    ---   共享参数系列 Global.txt 共享文件中存储的共享参数

        //  对应的共享参数文档中的内容如下：
        //  # This is a Revit shared parameter file.
        //  # Do not edit manually.
        //  *META	VERSION	MINVERSION
        //  META	2	1
        //  *GROUP	ID	NAME
        //  GROUP	1	OldW
        //  *PARAM	GUID	NAME	DATATYPE	DATACATEGORY	GROUP	VISIBLE	DESCRIPTION	USERMODIFIABLE
        //  PARAM	0948fd00-11d6-4ee1-beb7-66ee43fecf75	开挖完成	TEXT		1	1		1
        //  PARAM	2284a656-0770-481e-b251-496cde4e7f6d	OldW_Project	TEXT		1	1		0
        //  PARAM	c3d04d9e-aa78-4328-90c5-cf58167d1f09	监测数据	TEXT		1	1		0

        /// <summary>
        /// 共享参数组的名称
        /// </summary>
        /// <remarks></remarks>
        public const string SP_GroupName = "OldW";

        /// <summary>
        /// 作为基坑模型的标志判断的参数的GUID值。
        /// Revit中与基坑开挖相关的Document对象的标志性特征在于：此Document对象所对应的项目，在其“管理-项目信息”中，有一个参数：OldW_Project。
        /// 在此OldWDocument中，可以在Revit的Document中进行与基坑相关的操作，比如搜索基坑开挖土体，记录测点信息等。
        /// </summary>
        /// <returns>标识OldWDocument对象的项目信息（共享参数）OldW_Project的Guid值。</returns>
        public static Guid SP_OldWProjectInfo_Guid
        {
            get { return new Guid("2284a656-0770-481e-b251-496cde4e7f6d"); }
        }

        /// <summary>
        /// 作为基坑模型的标志判断的参数的名称。
        ///  Revit中与基坑开挖相关的Document对象的标志性特征在于：此Document对象所对应的项目，在其“管理-项目信息”中，有一个参数：OldW_Project。
        /// 在此OldWDocument中，可以在Revit的Document中进行与基坑相关的操作，比如搜索基坑开挖土体，记录测点信息等。
        /// </summary>
        /// <remarks></remarks>
        public const string SP_OldWProjectInfo = "OldW_Project";

        /// <summary> 每一个开挖土体单元，都有一个对应的开挖完成的日期数据。 </summary>
        public static Guid SP_ExcavationCompleted_Guid
        {
            get { return new Guid("0948fd00-11d6-4ee1-beb7-66ee43fecf75"); }
        }

        /// <summary> 每一个开挖土体单元，都有一个对应的开挖完成的日期数据。 </summary>
        public const string SP_ExcavationCompleted = "开挖完成";

        /// <summary> 每一个开挖土体单元，都有一个对应的开始开挖的日期数据。 </summary>
        public static Guid SP_ExcavationStarted_Guid
        {
            get { return new Guid("ee7193d1-e388-4053-bd6b-d903fad4ad8e"); }
        }

        /// <summary> 每一个开挖土体单元，都有一个对应的开始开挖的日期数据。 </summary>
        public const string SP_ExcavationStarted = "开挖开始";

        /// <summary> 每一个开挖土体单元，都有一个对应的开始开挖的日期数据。 </summary>
        public const string SP_MonitorData = "监测数据";

        /// <summary>
        /// 监测数据点的Element中，用一个共享参数来存储此测点的“监测数据”
        /// </summary>
        /// <returns>测点元素中表示监测数据的共享参数的Guid值。</returns>
        public static Guid SP_MonitorData_Guid
        {
            get { return new Guid("c3d04d9e-aa78-4328-90c5-cf58167d1f09"); }
        }

        /// <summary> 每一个开挖土体单元，都有一个对应的开始开挖的日期数据。 </summary>
        public const string SP_MonitorName = "测点编号";

        /// <summary>
        /// 监测数据点的Element中，用一个共享参数来存储此测点的“监测数据”
        /// </summary>
        /// <returns>测点元素中表示监测数据的共享参数的Guid值。</returns>
        public static Guid SP_MonitorName_Guid
        {
            get { return new Guid("98e55c99-5da9-4ef3-aa6b-7f94131538df"); }
        }

        /// <summary> 模型土体或者开挖土体单元的深度，此参数是通过API代码创建的实例参数，而且它与Revit中的模型尺寸相关联。 </summary>
        public const string SP_SoilDepth = "模型深度";

        /// <summary>
        /// 模型土体或者开挖土体单元的深度，此参数是通过API代码创建的实例参数，而且它与Revit中的模型尺寸相关联。
        /// </summary>
        /// <returns>土体单元中表示模型深度的共享参数的Guid值。</returns>
        public static Guid SP_SoilDepth_Guid
        {
            get { return new Guid("70abedd7-dd12-4b30-ab2a-c1dcabe62f37"); }
        }

        #endregion

        #region    ---   Excel 监测数据库的名称规范

        /// <summary> 墙体测斜,每一个测斜点的数据用一个工作表来保存 </summary>
        public const string ExcelDatabaseSheet_WallIncline = "CX";
        /// <summary> 土体测斜,每一个测斜点的数据用一个工作表来保存 </summary>
        public const string ExcelDatabaseSheet_SoilIncine = "TX";

        /// <summary> 墙顶位移在Revit中是属于线测点，但是出于习惯，在Excel中通过两个点测点的格式来放置监测数据 </summary>
        public const string ExcelDatabaseSheet_WallTopH = "墙顶水平位移";
        /// <summary> 墙顶位移在Revit中是属于线测点，但是出于习惯，在Excel中通过两个点测点的格式来放置监测数据 </summary>
        public const string ExcelDatabaseSheet_WallTopV = "墙顶垂直位移";
        /// <summary>  </summary>
        public const string ExcelDatabaseSheet_GroundHeave = "地表隆沉";
        /// <summary>  </summary>
        public const string ExcelDatabaseSheet_ColumnHeave = "立柱隆沉";
        /// <summary> 存 </summary>
        public const string ExcelDatabaseSheet_StrutForce = "支撑轴力";
        /// <summary>  </summary>
        public const string ExcelDatabaseSheet_WaterTable = "水位";

        /// <summary>  Excel数据库中，每一个工作表的第一个字段名称，即主键名称必须是中文“时间” </summary>
        public const string ExcelDatabasePrimaryKeyName = "时间";

        #endregion
    }
}