// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
//using Autodesk.Revit.DB;
	using System.Xml.Linq;
//using Autodesk.Revit.UI;
	using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
// End of VB project level imports


public 
partial class ConstructionReview : System.Windows.Forms.Form
{
	
	//Form overrides dispose to clean up the component list.
	[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
	
	//Required by the Windows Form Designer
	private System.ComponentModel.Container components = null;
	
	//NOTE: The following procedure is required by the Windows Form Designer
	//It can be modified using the Windows Form Designer.
	//Do not modify it using the code editor.
	[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConstructionReview));
		this.SuspendLayout();
		//
		//ConstructionReview
		//
		this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (12.0F));
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ClientSize = new System.Drawing.Size(440, 332);
		this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
		this.Name = "ConstructionReview";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "工况查看";
		this.ResumeLayout(false);
		
	}
}

