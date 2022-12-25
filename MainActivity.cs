using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using System;
using Android.Util;
using System.Collections.Generic;

namespace BarcodeEan128
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity, EMDKManager.IEMDKListener
    {
        private TextView statusView = null;
        private TextView dataView = null;

        EMDKManager emdkManager = null;
        BarcodeManager barcodeManager = null;
        Scanner scanner = null;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            statusView = FindViewById<TextView>(Resource.Id.statusView);
            dataView = FindViewById<TextView>(Resource.Id.DataView);

            EMDKResults results = EMDKManager.GetEMDKManager(Android.App.Application.Context, this);
            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                statusView.Text = "Status: EMDKManager object creation failed ...";
            }
            else
            {
                statusView.Text = "Status: EMDKManager object creation succeeded ...";
            }
        }

        void displayStatus(String status)
        {

            if (Looper.MainLooper.Thread == Java.Lang.Thread.CurrentThread())
            {
                statusView.Text = status;
            }
            else
            {
                RunOnUiThread(() => statusView.Text = status);
            }
        }

        void displaydata(string data)
        {

            if (Looper.MainLooper.Thread == Java.Lang.Thread.CurrentThread())
            {
                dataView.Text += (data + "\n");
            }
            else
            {
                RunOnUiThread(() => dataView.Text += data + "\n");
            }
        }

        void EMDKManager.IEMDKListener.OnClosed()
        {
            statusView.Text = "Status: EMDK Open failed unexpectedly. ";

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        void EMDKManager.IEMDKListener.OnOpened(EMDKManager emdkManager)
        {
            statusView.Text = "Status: EMDK Opened successfully ...";

            this.emdkManager = emdkManager;

            InitScanner();
        }

        void InitScanner()
        {
            if (emdkManager != null)
            {
                if (barcodeManager == null)
                {
                    try
                    {
                        //Get the feature object such as BarcodeManager object for accessing the feature.
                        barcodeManager = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);

                        scanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);

                        if (scanner != null)
                        {
                            //Attahch the Data Event handler to get the data callbacks.
                            scanner.Data += scanner_Data;

                            //Attach Scanner Status Event to get the status callbacks.
                            scanner.Status += scanner_Status;

                            scanner.Enable();

                            //EMDK: Configure the scanner settings
                            ScannerConfig config = scanner.GetConfig();
                            config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
                            config.ScanParams.DecodeLEDFeedback = true;
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                            config.ReaderParams.ReaderSpecific.ImagerSpecific.PickList = ScannerConfig.PickList.Enabled;
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                            config.DecoderParams.Code39.Enabled = true;
                            config.DecoderParams.Code128.Enabled = true;

                            scanner.SetConfig(config);
                        }
                        else
                        {
                            displayStatus("Failed to enable scanner.\n");
                        }
                    }
                    catch (ScannerException e)
                    {
                        displayStatus("Error: " + e.Message);
                    }
                    catch (Exception ex)
                    {
                        displayStatus("Error: " + ex.Message);
                    }
                }
            }
        }

        void DeinitScanner()
        {
            if (emdkManager != null)
            {
                if (scanner != null)
                {
                    try
                    {
                        scanner.Data -= scanner_Data;
                        scanner.Status -= scanner_Status;

                        scanner.Disable();
                    }
                    catch (ScannerException e)
                    {
                        Log.Debug(this.Class.SimpleName, "Exception:" + e.Result.Description);
                    }
                }

                if (barcodeManager != null)
                {
                    emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
                }
                barcodeManager = null;
                scanner = null;
            }
        }

        void scanner_Data(object sender, Scanner.DataEventArgs e)
        {
            ScanDataCollection scanDataCollection = e.P0;

            if ((scanDataCollection != null) && (scanDataCollection.Result == ScannerResults.Success))
            {
                IList<ScanDataCollection.ScanData> scanData = scanDataCollection.GetScanData();

                foreach (ScanDataCollection.ScanData data in scanData)
                {
                    displaydata(data.LabelType + " : " + data.Data);

                    if (data.LabelType.ToString() == "EAN128")
                    {
                        EAN128Parser.GroutSeperator = Convert.ToChar(29);
                        Dictionary<EAN128Parser.AII, string> dic = EAN128Parser.Parse(data.Data, false);

                        List<string> valores = new List<string>(dic.Values);
                        List<EAN128Parser.AII> claves = new List<EAN128Parser.AII>(dic.Keys);

                        for (int i = 0; i < claves.Count; i++)
                        {
                            displaydata("(" + claves[i] + ") \n" + valores[i]);
                        }
                    }
                }
            }
        }

        void scanner_Status(object sender, Scanner.StatusEventArgs e)
        {
            String statusStr = "";
            //EMDK: The status will be returned on multiple cases. Check the state and take the action.
            StatusData.ScannerStates state = e.P0.State;

            if (state == StatusData.ScannerStates.Idle)
            {
                statusStr = "Scanner is idle and ready to submit read.";
                try
                {
                    if (scanner.IsEnabled && !scanner.IsReadPending)
                    {
                        scanner.Read();
                    }
                }
                catch (ScannerException e1)
                {
                    statusStr = e1.Message;
                }
            }
            if (state == StatusData.ScannerStates.Waiting)
            {
                statusStr = "Waiting for Trigger Press to scan";
            }
            if (state == StatusData.ScannerStates.Scanning)
            {
                statusStr = "Scanning in progress...";
            }
            if (state == StatusData.ScannerStates.Disabled)
            {
                statusStr = "Scanner disabled";
            }
            if (state == StatusData.ScannerStates.Error)
            {
                statusStr = "Error occurred during scanning";
            }
            displayStatus(statusStr);
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitScanner();
        }
        
        protected override void OnPause()
        {
            base.OnPause();
            DeinitScanner();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //Clean up the emdkManager
            if (emdkManager != null)
            {
                //EMDK: Release the EMDK manager object
                emdkManager.Release();
                emdkManager = null;
            }
        }
    }
}

