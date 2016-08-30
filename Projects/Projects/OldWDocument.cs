using System;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using rvtTools;
using eZstd;
using eZstd.Data;

/// <summary>
/// 将Revit中与基坑开挖相关的Document对象转换为OldW程序中的OldWDocument对象。
/// 此OldWDocument对象的标志性特征在于：此Document对象所对应的项目，在其“管理-项目信息”中，有一个参数：OldW_Project。
/// 在此OldWDocument中，可以在Revit的Document中进行与基坑相关的操作，比如搜索基坑开挖土体，记录测点信息等。
/// 此对象可以通过静态函数 Create进行构造。
/// 此类中没有具体的操作方法，要进行具体的操作，请创建对应的的派生类，比如ExcavationDoc用来进行与基坑开挖相关的模拟。
/// </summary>
public class OldWDocument
{
    #region    ---    Properties

    /// <summary> 每一个OldWDocument对象都绑定了一个Revit的Document对象。 </summary>
    private Document _doc;
    /// <summary> 每一个OldWDocument对象都绑定了一个Revit的Document对象。 </summary>
    public Document Document
    {
        get { return this._doc; }
    }

    /// <summary> 每一个OldWDocument对象都绑定了一个Revit的 UIDocument 对象。 </summary>
    protected UIDocument uiDoc;

    /// <summary>
    /// OldWDocument中保存的与基坑开挖有关的信息
    /// </summary>
    /// <remarks></remarks>
    private OldWProjectInfo ProjectInfo;

    #endregion

    #region    ---   构造函数：通过静态函数 Create

    /// <summary> 直接构造 OldWDocument 对象，但是不保证此文档的有效性，即是否有对应的项目参数。
    /// 推荐使用  OldWApplication.SearchOrCreateOldWDocument(doc) 来获取有效的 OldWDocument 对象 </summary>
    /// <param name="doc"></param>
    protected internal OldWDocument(Document doc)
    {
        if (doc.IsValidObject)
        {
            this._doc = doc;
            this.uiDoc = new UIDocument(doc);
        }
        else
        {
            throw new ArgumentException(
                "The specified document to construct an instance of OldWDocument is not a valid object");
        }
    }

    /// <summary> 不在OldWApplication.OpenedDocuments 集合中进行搜索，而直接创建一个OldWDocument对象，并添加到OpenedDocuments集合中。 </summary>
    /// <param name="OldWApp">整个系统的OldWApplication对象</param>
    /// <param name="Doc">用户应该非常确信此Doc并不在OldWApplication的OpenedDocuments集合中，
    /// 否则，会将一个重复的OldWDocument对象再次添加进OpenedDocuments集合。
    /// 这样虽然不会报错，但是不利于程序的高效。</param>
    /// <returns>有相对应的对象，则返回之，否则则抛出异常。</returns>
    /// <remarks></remarks>
    private static OldWDocument Create(OldWApplication OldWApp, Document Doc)
    {
        OldWDocument OldWD = null;
        // 创建一个新的OldW文档
        if (IsOldWDocument(Doc))
        {
            OldWD = new OldWDocument(Doc);
            OldWApp.OpenedDocuments.Add(OldWD);
        }
        else // 说明这个文档不符合OldWDocument的特征，此时为其添加对应的项目参数，使其成为一个OldWDocument对象
        {
            if (Doc.IsFamilyDocument)
            {
                throw new InvalidOperationException("此文档为一个族文档，不能用来创建OldWDocument对象");
                // MessageBox.Show("此文档为一个族文档，不能用来创建OldWDocument对象", "Warnning", MessageBoxButtons.OK, MessageBoxIcon.Error)
            }
            else // 在项目信息中添加一个新的参数
            {
                OldWD = CreateNewAndBindParameter(Doc);
                OldWApp.OpenedDocuments.Add(OldWD);
            }
        }
        return OldWD;
    }

    /// <summary>
    /// 将一个非OldWDocument类型的文档创建成为一个OldWDocument对象，并在其项目信息中添加参数OldW_Project
    /// </summary>
    /// <param name="doc">注意默认OldWDocument.IsOldWDocument(Doc)为false，如果此Doc的项目信息中有参数OldW_Project，则重新进行绑定可能会出错。</param>
    /// <returns></returns>
    /// <remarks></remarks>
    internal static OldWDocument CreateNewAndBindParameter(Document doc)
    {
        Autodesk.Revit.ApplicationServices.Application app = doc.Application;
        DefinitionGroup myGroup = RvtTools.GetOldWDefinitionGroup(app);
        ExternalDefinition OldWDefi = (ExternalDefinition)myGroup.Definitions.get_Item(Constants.SP_OldWProjectInfo);

        // 创建一个“项目信息”类别，用来提供给共享参数进行绑定
        CategorySet myCategories = app.Create.NewCategorySet();
        // use BuiltInCategory to get category of wall
        Category myCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_ProjectInformation);
        myCategories.Insert(myCategory);

        //将外部共享参数绑定到项目信息中
        InstanceBinding instanceBinding = app.Create.NewInstanceBinding(myCategories);
        BindingMap bindingMap = doc.ParameterBindings;
        // Bind the definitions to the document
        using (Transaction resource = new Transaction(doc, "添加项目参数"))
        {
            resource.Start();
            bool instanceBindOK =
                Convert.ToBoolean(bindingMap.Insert(OldWDefi, instanceBinding, BuiltInParameterGroup.PG_TEXT));
            resource.Commit();
        }
        //
        return new OldWDocument(doc);
    }

    /// <summary>
    /// 判断一个Document文档是否是一个OldW的项目文档，其判断的依据是：：此Document对象所对应的项目，在其“管理-项目信息”中，有一个参数：OldW_Project。
    /// </summary>
    /// <param name="doc">要进行判断的Revit的Document文档</param>
    /// <returns></returns>
    internal static bool IsOldWDocument(Document doc)
    {
        bool bln = false;
        ProjectInfo proInfo = doc.ProjectInformation; // 族文件中的ProjectInformation属性为Nothing
        if (proInfo != null)
        {
            Parameter pa = proInfo.get_Parameter(Constants.SP_OldWProjectInfo_Guid);
            if (pa != null)
            {
                bln = true;
            }
        }
        return bln;
    }

    #endregion

    #region    ---    OldW项目信息 ProjectInformation 的保存与提取

    private OldWProjectInfo _oldWProjectInfo;
    /// <summary>
    /// 将与基坑开挖有关的信息保存到Document的相关参数中
    /// </summary>
    /// <param name="ProjInfo"></param>
    /// <remarks></remarks>
    public void SetProjectInfo(OldWProjectInfo ProjInfo)
    {
        Parameter pa = _doc.ProjectInformation.get_Parameter(Constants.SP_OldWProjectInfo_Guid);
        string Info = StringSerializer.Encode64(this.ProjectInfo);
        using (Transaction tran = new Transaction(_doc, "将OldW项目信息保存到Document中"))
        {
            tran.Start();
            pa.Set(Info);
            tran.Commit();
            tran.Commit();
        }
        _oldWProjectInfo = ProjInfo;
    }

    /// <summary>
    /// 从Document中提取OldWDocument中保存的与基坑开挖有关的信息
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public OldWProjectInfo GetProjectInfo()
    {
        if (_oldWProjectInfo != null)
        {
            return _oldWProjectInfo;
        }
        else
        {
            Parameter pa = this._doc.ProjectInformation.get_Parameter(Constants.SP_OldWProjectInfo_Guid);
            string info = Convert.ToString(pa.AsString());
            OldWProjectInfo proinfo = (OldWProjectInfo)StringSerializer.Decode64(info);
            return proinfo;
        }
    }

    #endregion

    #region    ---    不同Documente对象之间的比较

    /// <summary>
    /// 比较指定的Document对象与此OldWDocument对象中的Document对象是否是同一个Revit文档
    /// </summary>
    /// <returns>如果这两个Document对象是同一个Revit文档，则返回True，否则返回False。</returns>
    public new bool Equals(Document ComparedDocument)
    {
        if (IsOldWDocument(_doc))
        {
            return CompareDocuments(this.Document, ComparedDocument);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 比较两个Document对象是否是同一个Revit文档。
    /// 如果这两个Document对象是同一个Revit文档，则返回True，否则返回False。
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    private bool CompareDocuments(Document Doc1, Document Doc2)
    {
        bool IsEqual = false;
        if (Doc1.Equals(Doc2))
        {
            IsEqual = true;
        }
        else // 比较两个 Document 的绝对路径是否相同
        {
            string path1 = Convert.ToString(Doc1.PathName);
            string path2 = Convert.ToString(Doc2.PathName);
            if (path1.CompareTo(path2) == 0)
            {
                IsEqual = true;
            }
        }
        return IsEqual;
    }

    #endregion
}