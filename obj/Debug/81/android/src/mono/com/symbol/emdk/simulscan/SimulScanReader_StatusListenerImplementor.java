package mono.com.symbol.emdk.simulscan;


public class SimulScanReader_StatusListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.symbol.emdk.simulscan.SimulScanReader.StatusListerner
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onStatus:(Lcom/symbol/emdk/simulscan/SimulScanStatusData;)V:GetOnStatus_Lcom_symbol_emdk_simulscan_SimulScanStatusData_Handler:Symbol.XamarinEMDK.SimulScan.SimulScanReader/IStatusListernerInvoker, Symbol.XamarinEMDK\n" +
			"";
		mono.android.Runtime.register ("Symbol.XamarinEMDK.SimulScan.SimulScanReader+IStatusListenerImplementor, Symbol.XamarinEMDK", SimulScanReader_StatusListenerImplementor.class, __md_methods);
	}


	public SimulScanReader_StatusListenerImplementor ()
	{
		super ();
		if (getClass () == SimulScanReader_StatusListenerImplementor.class)
			mono.android.TypeManager.Activate ("Symbol.XamarinEMDK.SimulScan.SimulScanReader+IStatusListenerImplementor, Symbol.XamarinEMDK", "", this, new java.lang.Object[] {  });
	}


	public void onStatus (com.symbol.emdk.simulscan.SimulScanStatusData p0)
	{
		n_onStatus (p0);
	}

	private native void n_onStatus (com.symbol.emdk.simulscan.SimulScanStatusData p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
