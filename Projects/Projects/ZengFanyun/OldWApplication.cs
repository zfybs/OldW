// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
// End of VB project level imports
using Autodesk.Revit.UI;

/// <summary>
/// OldW程序中的应用程序级的操作。通常用来存储或者处理Document或者Element之上的对象。比如程序中所有打开的OldWDocument对象。
/// </summary>
/// <remarks></remarks>
public class OldWApplication
{
	private UIApplication uip;
#region    ---   Properties
	
	/// <summary>
	/// 当前正在运行的Revit的Application程序对象
	/// </summary>
	/// <remarks></remarks>
	private Autodesk.Revit.ApplicationServices.Application F_Application;
	/// <summary>
	/// 当前正在运行的Revit的Application程序对象
	/// </summary>
	/// <value></value>
	/// <returns></returns>
	/// <remarks>在每一次通过IExternalCommand接口执行的外部命令中，都可以从中提取出一个Application对象，
	/// 从变量上来说，每次的这个Application之间都是 not equal的，但是，这些Application对象都是代表Revit当前正在运行的应用程序，即其本质上是相同的。</remarks>
public Autodesk.Revit.ApplicationServices.Application Application
	{
		get
		{
			return F_Application;
		}
	}
	
	/// <summary>
	/// 整个系统中，所有打开的 OldWDocument 文档
	/// </summary>
	/// <remarks></remarks>
	private List<OldWDocument> F_OpenedDocuments = new List<OldWDocument>();
	/// <summary>
	/// 整个系统中，所有打开的 OldWDocument 文档
	/// </summary>
	/// <remarks></remarks>
public List<OldWDocument> OpenedDocuments
	{
		get
		{
			return this.F_OpenedDocuments;
		}
	}
	
	private OldWDocument F_ActiveDocument;
public OldWDocument ActiveDocument
	{
		get
		{
			return F_ActiveDocument;
		}
	}
	
#endregion
	
#region    ---   Fields
	
	
#endregion
	
#region    ---   构造函数：构造唯一实例的经典思路。其实例可以通过OldWApplication.Create方法获得。
	
	/// <summary>
	/// 程序中已经加载进来的唯一的OldWApplication实例
	/// </summary>
	/// <remarks></remarks>
	private static OldWApplication LoadedApplication;
	/// <summary>
	/// 程序中是否已经有加载过OldWApplication对象。
	/// </summary>
	/// <remarks></remarks>
	private static bool IsLoaded;
	
	/// <summary>
	/// OldWApplication类在整个程序中只有一个实例，为了保证这一点，会在Create方法中进行判断，
	/// 看在程序中是否已经存在对应的OldWApplication实例，如果有，则直接返回，如果没有，则创建一个新的。
	/// </summary>
	/// <param name="App"></param>
	/// <returns>返回程序中唯一的那一个OldWApplication对象，如果不能正常返回，则抛出异常。</returns>
	/// <remarks>由于OldWApplication中，对Revit的Application对象进行了很多事件处理，
	/// 所以，如果程序中有n个OldWApplication实例，那么，每一次触发Revit的Application中的事件，
	/// 在OldWApplication中，每一个实例都会对此事件进行一次操作，这会极大地造成程序的混乱。</remarks>
	internal static OldWApplication Create(Autodesk.Revit.ApplicationServices.Application App)
	{
		if (!IsLoaded)
		{
			LoadedApplication = new OldWApplication(App);
			IsLoaded = true;
			return LoadedApplication;
		}
		else // 直接返回现有的
		{
			if (LoadedApplication.Application.IsValidObject)
			{
				return LoadedApplication;
			}
			else
			{
				MessageBox.Show("在程序测试中，出现上一个外部命令的Application对象的IsValidObject属性为False的情况，请及时调试并解决", "warnning");
				LoadedApplication = new OldWApplication(App);
				return LoadedApplication;
			}
		}
	}
	
	/// <summary>
	/// 为了确保程序中只有唯一的OldWApplication实例，应该将其New方法设置为Private，然后将Create设置为Shared。
	/// </summary>
	/// <param name="App"></param>
	/// <remarks></remarks>
	private OldWApplication(Autodesk.Revit.ApplicationServices.Application App)
	{
		if (App.IsValidObject)
		{
			this.F_Application = App;
		    this.F_Application.DocumentClosing += this.App_DocumentClosing;
			this.F_Application.DocumentClosed += new System.EventHandler<DocumentClosedEventArgs>(this.App_DocumentClosed);
            }
		else
		{
			throw (new ArgumentException("The specified application to construct an instance of OldWApplication is not a valid object"));
		}
	}
    
    #endregion

    #region    ---    整个程序中打开的 OldWDocument 文档的集合

    /// <summary> 在整个系统的集合中，搜索是否有与指定的Document相对应的OldWDocument对象，如果没有，则返回Nothing。 </summary>
    /// <param name="Doc"></param>
    /// <param name="ClosingDocumentIndex">即将删除的Document文档中程序的 OpenedDocuments 集合中的下标位置，如果此Document文档不在OpenedDocuments集合中，则其值为-1。</param>
    /// <returns>有相对应的对象，则返回之，否则返回Nothing。</returns>
    /// <remarks></remarks>
    public OldWDocument SearchOldWDocument(Document Doc, ref int ClosingDocumentIndex)
	{
		OldWDocument OldWD = null;
		// 搜索是否有对应的 OldWDocument 对象
		OldWDocument od = null;
		for ( ClosingDocumentIndex = 0; ClosingDocumentIndex <= OpenedDocuments.Count - 1; ClosingDocumentIndex++)
		{
			od = OpenedDocuments[ClosingDocumentIndex];
			if (od.Equals(Doc))
			{
				OldWD = od;
				break;
			}
		}
		if (OldWD == null)
		{
			ClosingDocumentIndex = -1;
		}
		return OldWD;
	}
	
	/// <summary>
	/// 在 Application.DocumentClosing 事件中，即将要关闭的那个Document的Id值。
	/// 对于同一个Revit文档，在其每次触发 Application.DocumentClosing事件时，其对应的DocumentId值都是不一样的。
	/// 这个DocumentId值只是为了与对应的Application.DocumentClosed事件中的DocumentId值进行匹配。
	/// </summary>
	/// <remarks></remarks>
	private int DocumentIdTobeClosed;
	/// <summary>
	/// 即将删除的Document文档中程序的 OpenedDocuments 集合中的下标位置，如果此Document文档不在OpenedDocuments集合中，则其值为-1。
	/// </summary>
	private int DocumentIndexTobeClosed = -1;
	
	private void App_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
	{
		DocumentIdTobeClosed = System.Convert.ToInt32(e.DocumentId);
		SearchOldWDocument(e.Document, ref DocumentIndexTobeClosed);
	}
	
	private void App_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
	{
		if (e.Status == Autodesk.Revit.DB.Events.RevitAPIEventStatus.Succeeded)
		{
			int ClosedDocumentId = System.Convert.ToInt32(e.DocumentId);
			// This Id is only used to identify the document for the duration of this event and the DocumentClosing event which preceded it.
			//It serves as synchronization means for the pair of events.
			if (e.DocumentId == DocumentIdTobeClosed && (DocumentIndexTobeClosed > 0))
			{
				// 如果这个文档确实是被删除了，而且这个文档确实是位于OldWDocument集合中，
				// 则就要要 从全局集合 OpenedDocuments 集合中删除对应的元素
				OpenedDocuments.RemoveAt(DocumentIndexTobeClosed);
			}
			//
		}
	}
	
#endregion
	
}

