// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
// End of VB project level imports


public class GrammerTest
{
	
	private delegate void event1EventHandler(string UserName);
	private event1EventHandler event1Event;
	
	private event event1EventHandler event1
	{
		add
		{
			event1Event = (event1EventHandler) System.Delegate.Combine(event1Event, value);
		}
		remove
		{
			event1Event = (event1EventHandler) System.Delegate.Remove(event1Event, value);
		}
	}
	
	private Action<string> event2Event;
	private event Action<string> event2
	{
		add
		{
			event2Event = (Action<string>) System.Delegate.Combine(event2Event, value);
		}
		remove
		{
			event2Event = (Action<string>) System.Delegate.Remove(event2Event, value);
		}
	}
	
	private EventHandler<string> event3Event;
	private event EventHandler<string> event3
	{
		add
		{
			event3Event = (EventHandler<string>) System.Delegate.Combine(event3Event, value);
		}
		remove
		{
			event3Event = (EventHandler<string>) System.Delegate.Remove(event3Event, value);
		}
	}
	
	
	// Declare the delegate (if using non-generic pattern).
	public delegate void SampleEventHandler();
	
	// Declare the event.
	private SampleEventHandler event4Event;
	public event SampleEventHandler event4
	{
		add
		{
			event4Event = (SampleEventHandler) System.Delegate.Combine(event4Event, value);
		}
		remove
		{
			event4Event = (SampleEventHandler) System.Delegate.Remove(event4Event, value);
		}
	}
	
	
	public GrammerTest()
	{
		if (event1Event != null)
			event1Event("");
		if (event2Event != null)
			event2Event("");
		if (event3Event != null)
			event3Event(this, "");
		if (event4Event != null)
			event4Event();
	}
	
}
