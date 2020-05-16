using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Locations;

namespace POIApp
{
    [Activity(Label = "POIs", MainLauncher = true, Theme = "@android:style/Theme.Material.Light.DarkActionBar")]
    class POIListActivity :Activity, ILocationListener
    {
        ListView _poiListView;
        LocationManager _locMgr;

        POIListViewAdapter _adapter;
        int REQUEST_PERMISSIONS = 1000;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.POIList);

            CheckAppPermissions();

            _poiListView = FindViewById<ListView>(Resource.Id.poiListView);

            try
            {
                _adapter = new POIListViewAdapter(this);
                _poiListView.Adapter = _adapter;
            } catch (Exception e)
            {
                Console.WriteLine("Adapter exception",e.ToString());
            }
         
            _poiListView.ItemClick += POIClicked;

            _locMgr = GetSystemService(Context.LocationService) as LocationManager;

        }



        protected override void OnResume()
        {
            base.OnResume();

            _adapter.NotifyDataSetChanged();

            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.NoRequirement;
            criteria.PowerRequirement = Power.NoRequirement;

            string provider = _locMgr.GetBestProvider(criteria, true);
            _locMgr.RequestLocationUpdates(provider, 20000, 100, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locMgr.RemoveUpdates(this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu) 
        { 
            MenuInflater.Inflate(Resource.Menu.POIListViewMenu, menu);
            return true; }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionNew: 
                    StartActivity(typeof(POIDetailActivity)); 
                    return true;

                case Resource.Id.actionRefresh:
                    POIData.Service.RefreshCache();
                    _adapter.NotifyDataSetChanged();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }


        public bool CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return true;
            }

            if (!(ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted) 
                && !(ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted
                && !(ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == (int)Permission.Granted))
                && !(ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == (int)Permission.Granted))
            {
                var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage ,Manifest.Permission.AccessFineLocation,Manifest.Permission.AccessCoarseLocation};
                ActivityCompat.RequestPermissions(this, permissions, REQUEST_PERMISSIONS);
                return false;
            }
            return true;

        }



        protected void POIClicked(object sender, ListView.ItemClickEventArgs e) { 
            PointOfInterest poi = POIData.Service.GetPOI((int)e.Id); 
            Console.WriteLine("POIClicked: Name is {0}", poi.Name);

            Intent poiDetailIntent = new Intent(this, typeof(POIDetailActivity));
            poiDetailIntent.PutExtra("poiId",(int)poi.Id);
            Console.WriteLine("intent id is {0}", poi.Id);

            StartActivity(poiDetailIntent);

        }

        public void OnLocationChanged(Location location)
        {
            _adapter.CurrentLocation = location;
            _adapter.NotifyDataSetChanged();
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }
    }
}