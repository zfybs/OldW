using System.IO;
using System.Reflection;

namespace OldW.GlobalSettings
{
    /// <summary>
    /// 项目中各种文件的路径
    /// </summary>
    /// <remarks></remarks>
    public static class ProjectPath
    {
        /// <summary>
        /// Application的Dll所对应的路径，也就是“bin”文件夹的目录。
        /// </summary>
        /// <remarks>等效于：Dim thisAssemblyPath As String = System.Reflection.Assembly.GetExecutingAssembly().Location</remarks>
        private static  string Path_Dlls
        {
            get
            {
                // 在最终的发布中，Path_Dlls的值是等于“ My.Application.Info.DirectoryPath”的，但是，在调试过程中，如果使用Revit的AddinManager插件进行调试，
                // 则在每次调试时，AddinManager都会将对应的dll复制到一个新的临时文件夹中，如果将此复制后的dll的路作为Path_Dlls的值，那么其后面的Path_Solution等路径都是无效路径了。
                if (Directory.Exists("F:\\Software\\Revit\\RevitDevelop\\OldW\\bin"))
                {
                    return "F:\\Software\\Revit\\RevitDevelop\\OldW\\bin";
                }
                else
                {
                    return Assembly.GetExecutingAssembly().Location;
                    // 等效于：Dim thisAssemblyPath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
                    // 等效于：(new Microsoft.VisualBasic.ApplicationServices.ConsoleApplicationBase()).Info.DirectoryPath
                }
            }
        }

        /// <summary>
        /// Solution文件所在的文件夹
        /// </summary>
        private static readonly string Path_Solution = new DirectoryInfo(Path_Dlls).Parent.FullName;

        /// <summary>
        /// Resources 文件所在的文件夹
        /// </summary>
        private static readonly string Path_Resources = Path.Combine(Path_Solution, "Resources");

        /// <summary>
        /// 存放图标的文件夹
        /// </summary>
        private static readonly string Path_icons = Path.Combine(Path_Resources, "icons");

        /// <summary>
        /// 存放族与族共享参数的文件夹
        /// </summary>
        public static readonly string Path_family = Path.Combine(Path_Resources, "Family");

        /// <summary>
        /// 存放数据文件
        /// </summary>
        /// <remarks></remarks>
        public static readonly string Path_data = Path.Combine(Path_Resources, "Data");

        /// <summary>
        ///   族共享参数的文件的绝对路径（不是文件夹的路径）。
        /// </summary>
        /// <remarks>先通过application.SharedParametersFilename来设置当前的SharedParametersFilename的路径，
        /// 然后通过application.OpenSharedParameterFile方法来返回对应的DefinitionFile对象。</remarks>
        public static readonly string Path_SharedParametersFile =Path.Combine(Path_family, "global.txt");

        /// <summary>
        /// 监测警戒规范的绝对文件路径
        /// </summary>
        public static readonly string Path_WarningValueUsing =Path.Combine(Path_data, "WarningValueUsing.dat");
    }
}