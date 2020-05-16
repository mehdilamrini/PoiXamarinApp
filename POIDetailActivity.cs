using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POIApp
{
    [Activity(Label = "POIDetailActivity", MainLauncher = false, Theme = "@android:style/Theme.Material.Light.DarkActionBar")]
    class POIDetailActivity : Activity, ILocationListener
    {
        PointOfInterest _poi;
        LocationManager _locMgr;
        ProgressDialog _progressDialog;

        bool errors =false;
        EditText _nameEditText; 
        EditText _descrEditText;
        EditText _addrEditText; 
        EditText _latEditText;
        EditText _longEditText; 
        ImageView _poiImageView;
        ImageButton _locationImageButton ;
        ImageButton _mapImageButton;
        ImageButton _photoImageButton;
        const int CAPTURE_PHOTO = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.POIDetail);

            _nameEditText = FindViewById<EditText>(Resource.Id.nameEditText);
            _descrEditText = FindViewById<EditText>(Resource.Id.descEditText);
            _addrEditText = FindViewById<EditText>(Resource.Id.addeditText);
            _latEditText = FindViewById<EditText>(Resource.Id.latEditText);
            _longEditText = FindViewById<EditText>(Resource.Id.longEditText);
            _poiImageView = FindViewById<ImageView>(Resource.Id.poiImageView);
            _photoImageButton = FindViewById<ImageButton>(Resource.Id.newPictureButton);
            _locMgr = GetSystemService(Context.LocationService) as LocationManager;

            _locationImageButton = FindViewById<ImageButton>(Resource.Id.locationImageButton);
            _locationImageButton.Click += GetLocationClicked;

            _mapImageButton = FindViewById<ImageButton>(Resource.Id.mapImageButton);
            _mapImageButton.Click += MapImageClicked;

            _photoImageButton.Click += NewPhotoClicked;

            if (Intent.HasExtra("poiId"))
            {
                int poiId = Intent.GetIntExtra("poiId", -1);
                _poi = POIData.Service.GetPOI(poiId);
                UpdateUI();
            }
            else _poi = new PointOfInterest();

            if (Intent.HasExtra("poiId")) { 
                int poiId = Intent.GetIntExtra("poiId", -1); 
                _poi = POIData.Service.GetPOI(poiId);
                Android.Graphics.Bitmap poiImage = POIData.GetImageFile(_poi.Id.Value);
                _poiImageView.SetImageBitmap(poiImage); 
                if (poiImage != null) poiImage.Dispose(); }
            else _poi = new PointOfInterest();


        }

        public void NewPhotoClicked(object sender, EventArgs e) 
        { if (!_poi.Id.HasValue) 
            { AlertDialog.Builder 
                    alertConfirm = new AlertDialog.Builder(this);
                alertConfirm.SetCancelable(false); 
                alertConfirm.SetPositiveButton("OK", delegate { }); 
                alertConfirm.SetMessage("You must save the POI prior to attaching a photo"); 
                alertConfirm.Show(); } else { 
                Intent cameraIntent = new Intent(MediaStore.ActionImageCapture);
                PackageManager packageManager = PackageManager; 
                IList<ResolveInfo> activities = packageManager.QueryIntentActivities(cameraIntent, 0); 
                if (activities.Count == 0) { AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this); 
                    alertConfirm.SetCancelable(false); 
                    alertConfirm.SetPositiveButton("OK", delegate { }); 
                    alertConfirm.SetMessage("No camera app available to capture photos.");
                    alertConfirm.Show(); } else { Java.IO.File imageFile = new Java.IO.File(POIData.Service.GetImageFilename(_poi.Id.Value)); 
                    Android.Net.Uri imageUri = Android.Net.Uri.FromFile(imageFile);
                    cameraIntent.PutExtra(MediaStore.ExtraOutput, imageUri); 
                    cameraIntent.PutExtra(MediaStore.ExtraSizeLimit, 1.5 * 1024);

                    StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
                    StrictMode.SetVmPolicy(builder.Build());
                    StartActivityForResult(cameraIntent, CAPTURE_PHOTO); }
            
            
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == CAPTURE_PHOTO)
            {
                if (resultCode == Result.Ok)
                {    // display saved image    
                    Android.Graphics.Bitmap poiImage = POIData.GetImageFile (_poi.Id.Value);   
                    _poiImageView.SetImageBitmap (poiImage);   
                    if (poiImage != null)      
                        poiImage.Dispose ();    }   
                else {      // let the user know the photo was cancelled      
                    Toast toast = Toast.MakeText (this, "No picture captured.",ToastLength.Short);   
                    toast.Show();    }  }  
            else    base.OnActivityResult (requestCode, resultCode, data);
        }

    
        protected void MapImageClicked(object sender, EventArgs e)
        {
            Android.Net.Uri geoUri;
            if (String.IsNullOrEmpty(_addrEditText.Text))
            {
                geoUri = Android.Net.Uri.Parse(String.Format("geo:{0},{1}", _poi.Latitude, _poi.Longitude));
            }
            else
            {
                geoUri = Android.Net.Uri.Parse(String.Format("geo:0,0?q={0}", _addrEditText.Text));
            }

            Intent mapIntent = new Intent(Intent.ActionView, geoUri);

            PackageManager packageManager = PackageManager; 
            IList<ResolveInfo> activities = packageManager.QueryIntentActivities(mapIntent, 0);
            if (activities.Count == 0) { 
                AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this); 
                alertConfirm.SetCancelable(false); 
                alertConfirm.SetPositiveButton("OK", delegate { }); 
                alertConfirm.SetMessage("No map app available.");
                alertConfirm.Show(); }
            else StartActivity(mapIntent);
        }

        protected void GetLocationClicked(object sender, EventArgs e)
        {
            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.NoRequirement;
            criteria.PowerRequirement = Power.NoRequirement;

            _locMgr.RequestSingleUpdate(criteria, this, null);

            _progressDialog = ProgressDialog.Show(this, "", "Obtaining location...");
        }

        protected void UpdateUI()
        {
           
                _nameEditText.Text = _poi.Name;
                _descrEditText.Text = _poi.Description;
                _addrEditText.Text = _poi.Address;
                _latEditText.Text = _poi.Latitude.ToString();
                _longEditText.Text = _poi.Longitude.ToString();
            
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.POIDetailMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {

                case Resource.Id.actionSave:
                    SavePOI();
                    return true;

                case Resource.Id.actionDelete:
                    DeletePOI(); return true;

                default: return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            base.OnPrepareOptionsMenu(menu);

            // disable delete for a new POI
            if (!_poi.Id.HasValue)
            {
                IMenuItem item =
              menu.FindItem(Resource.Id.actionDelete);
                item.SetEnabled(false);
            }

            return true;
        }

        protected void checkErrors()
        {
            if (String.IsNullOrEmpty(_nameEditText.Text))
            {
                _nameEditText.Error = "Name cannot be empty";
                errors = true;
            }
            else
                _nameEditText.Error = null;


            double? tempLatitude = null;
            if (!String.IsNullOrEmpty(_latEditText.Text))
            {
                try
                {
                    tempLatitude = Double.Parse(_latEditText.Text);
                    if ((tempLatitude > 90) | (tempLatitude < -90))
                    {
                        _latEditText.Error = "Latitude must be a decimal valuebetween -90 and 90";
                        errors = true;
                    }
                    else
                        _latEditText.Error = null;
                }
                catch
                {
                    _latEditText.Error = "Latitude must be valid decimal number";
                    errors = true;
                }
            }
        }

        protected void SavePOI()
        {
            checkErrors();
            if (!errors)
            {

            _poi.Name = _nameEditText.Text;
            _poi.Description = _descrEditText.Text;
            _poi.Address = _addrEditText.Text;
            _poi.Latitude = Double.Parse(_latEditText.Text);
            _poi.Longitude = Double.Parse(_longEditText.Text);

            POIData.Service.SavePOI(_poi);
            Finish();
                Toast toast = Toast.MakeText(this, String.Format("{0} Updated.", _poi.Name), ToastLength.Short); toast.Show();
            }


        }

        protected void ConfirmDelete(object sender, EventArgs e)
        {
            POIData.Service.DeletePOI(_poi);
            Finish();
            Toast toast = Toast.MakeText(this, String.Format("{0} deleted.", _poi.Name), 
                ToastLength.Short); toast.Show();
        }
        protected void DeletePOI() { 
            AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this); 
            alertConfirm.SetCancelable(false); 
            alertConfirm.SetPositiveButton("OK", ConfirmDelete); 
            alertConfirm.SetNegativeButton("Cancel", delegate { }); 
            alertConfirm.SetMessage(String.Format("Are you sure youwant to delete {0}?", _poi.Name)); 
            alertConfirm.Show(); }

        public void OnLocationChanged(Location location)
        {
            _latEditText.Text = location.Latitude.ToString();
            _longEditText.Text = location.Longitude.ToString();

            Console.WriteLine("location changed {0} {1} ", location.Latitude.ToString(), location.Longitude.ToString());

            Geocoder geocdr = new Geocoder(this);
            IList<Address> addresses = geocdr.GetFromLocation(location.Latitude, location.Longitude, 5);

            if (addresses.Any())
            {
                UpdateAddressFields(addresses.First());
            }

            _progressDialog.Cancel();
        }

        protected void UpdateAddressFields(Address addr)
        {
            if (String.IsNullOrEmpty(_nameEditText.Text))
                _nameEditText.Text = addr.FeatureName;

            if(String.IsNullOrEmpty(_addrEditText.Text)) {
                for (int i = 0; i < addr.MaxAddressLineIndex; i++)
                {
                    if (!String.IsNullOrEmpty(_addrEditText.Text))

                        _addrEditText.Text += System.Environment.NewLine;
                    _addrEditText.Text += addr.GetAddressLine(i);
                }

            }
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