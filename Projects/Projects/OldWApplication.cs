using System.Collections.Generic;
using System;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

/// <summary>
/// OldW程序中的应用程序级的操作。通常用来存储或者处理Document或者Element之上的对象。比如程序中所有打开的OldWDocument对象。
/// </summary>
/// <remarks></remarks>
public class OldWApplication
{

    #region    ---   Properties
    
    /// <summary> 当前正在运行的Revit的Application程序对象 </summary>
    /// <remarks>在每一次通过IExternalCommand接口执行的外部命令中，都可以从中提取出一个Application对象，
    /// 从变量上来说，每次的这个Application之间都是 not equal的，但是，这些Application对象都是代表Revit当前正在运行的应用程序，即其本质上是相同的。</remarks>
    public UIApplication UIApplication { get; }
    /// <summary> 当前正在运行的Revit的Application程序对象 </summary>
    /// <remarks>在每一次通过IExternalCommand接口执行的外部命令中，都可以从中提取出一个Application对象，
    /// 从变量上来说，每次的这个Application之间都是 not equal的，但是，这些Application对象都是代表Revit当前正在运行的应用程序，即其本质上是相同的。</remarks>
    public Application Application { get; }
    
    /// <summary>
    /// 整个系统中，所有打开的 OldWDocument 文档
    /// </summary>
    /// <remarks></remarks>
    private List<OldWDocument> _openedDocuments = new List<OldWDocument>();
    /// <summary> 整个系统中，所有打开的 OldWDocument 文档 </summary>
    /// <remarks></remarks>
    public List<OldWDocument> OpenedDocuments
    {
        get
        {
            return this._openedDocuments;
        }
    }

    #endregion

    #region    ---   Fields


    #endregion

    #region    ---   构造函数：构造唯一实例的经典思路。其实例可以通过OldWApplication.Create方法获得。

    /// <summary> 程序中已经加载进来的唯一的OldWApplication实例 </summary>
    private static OldWApplication _uniqueApplication;


    /// <summary>
    /// OldWApplication类在整个程序中只有一个实例，为了保证这一点，会在Create方法中进行判断，
    /// 看在程序中是否已经存在对应的OldWApplication实例，如果有，则直接返回，如果没有，则创建一个新的。
    /// </summary>
    /// <param name="uiApp"></param>
    /// <returns>返回程序中唯一的那一个OldWApplication对象，如果不能正常返回，则抛出异常。</returns>
    /// <remarks>
    /// 1.   由于OldWApplication中，对Revit的Application对象进行了很多事件处理，
    ///      所以，如果程序中有n个OldWApplication实例，那么，每一次触发Revit的Application中的事件，
    ///      在OldWApplication中，每一个实例都会对此事件进行一次操作，这会极大地造成程序的混乱。
    /// 2..  即使同时开启多个Revit进程，每个进程中的内存信息肯定是相互独立的，所以不用担心
    ///      出现两个Revit进程对应同一个 OldWApplication 对象的情况。</remarks>
    public static OldWApplication GetUniqueApplication(UIApplication uiApp)
    {
        if (_uniqueApplication == null)
        {
            _uniqueApplication = new OldWApplication(uiApp);
            return _uniqueApplication;
        }
        else // 直接返回现有的
        {
            if (_uniqueApplication.Application.IsValidObject)
            {
                return _uniqueApplication;
            }
            else
            {
                throw new ArgumentException(@"在程序测试中，出现某一个外部命令的Application对象的IsValidObject属性为False的情况，请及时调试并解决");
                //MessageBox.Show(@"在程序测试中，出现某一个外部命令的Application对象的IsValidObject属性为False的情况，请及时调试并解决", "warnning");
                //_uniqueApplication = new OldWApplication(App);
                //return _uniqueApplication;
            }
        }
    }

    /// <summary> 构造函数。 为了确保程序中只有唯一的OldWApplication实例，应该将其New方法设置为Private，然后将Create设置为Shared。 </summary>
    /// <param name="uiApp"></param>
    /// <remarks></remarks>
    private OldWApplication(UIApplication uiApp)
    {
        UIApplication = uiApp;
        Application app = uiApp.Application;
        if (app.IsValidObject)
        {
            this.Application = app;
           // this.Application.DocumentClosing += this.App_DocumentClosing;
            this.Application.DocumentClosed += new System.EventHandler<DocumentClosedEventArgs>(this.App_DocumentClosed);
        }
        else
        {
            throw (new ArgumentException("The specified application to construct an instance of OldWApplication is not a valid object"));
        }
    }

    #endregion

    #region    ---    整个程序中打开的 OldWDocument 文档的集合

    /// <summary> 在OldWApplication.OpenedDocuments 集合中，搜索是否有与指定的Document相对应的OldWDocument对象 </summary>
    /// <returns>有相对应的对象，则返回之，否则则抛出异常。</returns>
    /// <remarks></remarks>
    internal OldWDocument SearchOrCreateOldWDocument(Document doc)
    {

        OldWDocument oldWDoc = null;
        if (OldWDocument.IsOldWDocument(doc))
        {
            // 先搜索系统集合中是否有对应的 OldWDocument 对象
            int closingDocumentIndex = -1;
            oldWDoc = this.MatchOldWDocument(doc, matchedOldDocumentIndex: out closingDocumentIndex);
            if (oldWDoc == null) // 说明此文档在OldWApp的集合中没有找到对应的项。
            {
                oldWDoc = new OldWDocument(doc);
                this.OpenedDocuments.Add(oldWDoc);
            }
            else
            {
                return oldWDoc;
            }
        }
        else // 说明这个文档不符合OldWDocument的特征，此时为其添加对应的项目参数，使其成为一个OldWDocument对象
        {
            if (doc.IsFamilyDocument)
            {
                throw new InvalidOperationException("此文档为一个族文档，不能用来创建OldWDocument对象");
                // MessageBox.Show("此文档为一个族文档，不能用来创建OldWDocument对象", "Warnning", MessageBoxButtons.OK, MessageBoxIcon.Error)
            }
            else // 在项目信息中添加一个新的参数
            {
                oldWDoc = OldWDocument.CreateNewAndBindParameter(doc);
                this.OpenedDocuments.Add(oldWDoc);
            }
        }
        return oldWDoc;
    }

    /// <summary> 在整个系统的集合中，搜索是否有与指定的Document相对应的OldWDocument对象，如果没有，则返回Nothing。 </summary>
    /// <param name="doc"></param>
    /// <param name="matchedOldDocumentIndex"> OpenedDocuments 集合中匹配的 OldWDocument 的下标位置，
    /// 如果此Document文档不在OpenedDocuments集合中，则其值为-1。</param>
    /// <returns>有相对应的对象，则返回之，否则返回Nothing。</returns>
    /// <remarks></remarks>
    private OldWDocument MatchOldWDocument(Document doc, out int matchedOldDocumentIndex)
    {
        OldWDocument oldWDoc = null;

        // 搜索是否有对应的 OldWDocument 对象
        for (matchedOldDocumentIndex = 0; matchedOldDocumentIndex < OpenedDocuments.Count; matchedOldDocumentIndex++)
        {
            var od = OpenedDocuments[matchedOldDocumentIndex];
            if (od.Equals(doc))
            {
                oldWDoc = od;
                break;
            }
        }
        if (oldWDoc == null)
        {
            matchedOldDocumentIndex = -1;
        }
        return oldWDoc;
    }

    #endregion

    #region    ---    DocumentClose 事件处理：在 Document 关闭时同步删除其在 OpenedDocuments 集合中的对应项

    ///// <summary>
    ///// 在 Application.DocumentClosing 事件中，即将要关闭的那个Document的Id值。
    ///// 对于同一个Revit文档，在其每次触发 Application.DocumentClosing事件时，其对应的DocumentId值都是不一样的。
    ///// 这个DocumentId值只是为了与对应的Application.DocumentClosed事件中的DocumentId值进行匹配。
    ///// </summary>
    ///// <remarks></remarks>
    //private int _documentIdTobeClosed;
    ///// <summary>
    ///// 即将删除的Document文档中程序的 OpenedDocuments 集合中的下标位置，如果此Document文档不在OpenedDocuments集合中，则其值为-1。
    ///// </summary>
    //private int _documentIndexTobeClosed = -1;

    //private void App_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
    //{
    //    _documentIdTobeClosed = e.DocumentId;
    //    this.MatchOldWDocument(e.Document, matchedOldDocumentIndex: out _documentIndexTobeClosed);
    //}

    private void App_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
    {
        ValidateOpenedOldWDocuments();
        //if (e.Status == RevitAPIEventStatus.Succeeded)
        //{
        //    // This Id is only used to identify the document for the duration of this event and the DocumentClosing event which preceded it.
        //    //It serves as synchronization means for the pair of events.
        //    if (e.DocumentId == _documentIdTobeClosed && (_documentIndexTobeClosed >= 0))
        //    {
        //        // 如果这个文档确实是被删除了，而且这个文档确实是位于OldWDocument集合中，
        //        // 则就要要 从全局集合 OpenedDocuments 集合中删除对应的元素
        //        OpenedDocuments.RemoveAt(_documentIndexTobeClosed);
        //        _documentIndexTobeClosed = -1;
        //    }
        //    //
        //}
    }

    /// <summary> 对 _openedDocuments 中的元素进行验证，以剔除其中因为被关闭或者其他原因而成为非 IsValidObject 的文档。 </summary>
    private void ValidateOpenedOldWDocuments()
    {
        _openedDocuments = _openedDocuments.Where(d => d.Document.IsValidObject).ToList();
    }
        
    #endregion


}

