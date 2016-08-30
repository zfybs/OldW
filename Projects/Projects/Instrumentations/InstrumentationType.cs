using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using eZstd.Data;
using OldW.GlobalSettings;

namespace OldW.Instrumentations // 与 OldW.Instrumentation 命名空间相关的一些接口、枚举等的定义
{
    /// <summary>
    /// (位编码)监测仪器的族名称（也是族文件的名称），同时也作为监测仪器的类型判断
    /// </summary>
    /// <remarks>从枚举值返回对应的枚举字符的方法：GlobalSettings.InstrumentationType.沉降测点.ToString</remarks>
    [Flags()]
    public enum InstrumentationType : int
    {

        /// <summary> 并不是任何一种已经指定的线测点或者点测点类型 </summary>
        未指定 = 0,

        /// <summary> 并不是任何一种已经特殊处理过的点测点类型 </summary>
        其他点测点 = 1,

        /// <summary> 并不是任何一种已经特殊处理过的线测点类型 </summary>
        其他线测点 = 2,

        /// <summary> 比如地下连续墙的水平位移 </summary>
        墙体测斜 = 4,

        /// <summary> 比如土体中的测斜管的水平位移，它与墙体测斜的区别在于墙体测斜是嵌在地下连续墙中，
        /// 而且墙体测斜与土体测斜的安全警戒准则并不相同。 </summary>
        土体测斜 = 8,

        /// <summary> 墙顶位移的监测数据包括水平位移与垂直位移。
        /// 在Excel中通过两张表“墙顶水平位移”与“墙顶垂直位移”来保存。 </summary>
        墙顶位移 = 16,

        /// <summary> 比如基坑外地表的垂直位移 </summary>
        地表隆沉 = 32,

        /// <summary> 比如基坑中立柱的垂直位移 </summary>
        立柱隆沉 = 64,

        /// <summary> 比如基坑中支撑的轴力 </summary>
        支撑轴力 = 128,

        /// <summary> 比如基坑中水位测点处的水位高低 </summary>
        水位 = 256,

        /// <summary> 通过位运算进行组合的非数值线测点的集合。不包括墙体测斜这种子节点有数值意义的线测点 </summary>
        非数值线测点集合 = 墙顶位移,

        /// <summary> 通过位运算进行组合的数值线测点的集合。不包括墙顶位移这种子节点没有数值意义的线测点 </summary>
        数值线测点集合 = 其他线测点 | 墙体测斜 | 土体测斜,

        /// <summary> 通过位运算进行组合的所有线测点的集合。包括墙顶位移 </summary>
        线测点集合 = 数值线测点集合 | 非数值线测点集合,

        /// <summary> 通过位运算进行组合的所有点测点的集合。 </summary>
        点测点集合 = 其他点测点 | 地表隆沉 | 立柱隆沉 | 支撑轴力 | 水位,


    }
}