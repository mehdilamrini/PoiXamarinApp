using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POIApp
{
    class POIListViewAdapter : BaseAdapter<PointOfInterest>


    {
        private readonly Activity _context;
        public Location CurrentLocation { get; set; }


        public POIListViewAdapter(Activity context)
        {
            _context = context;
        }
        public override PointOfInterest this[int position]
        {
            get { return POIData.Service.POIs[position]; }
        }
        public override int Count {
            get { return POIData.Service.POIs.Count; }
        }

        public override long GetItemId(int position)
        {
            return POIData.Service.POIs[position].Id.Value;

        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View view = convertView; if (view == null)
            view = _context.LayoutInflater.Inflate(Resource.Layout.POIListItem, null);

            PointOfInterest poi = POIData.Service.POIs[position];

            view.FindViewById<TextView>(Resource.Id.nameTextView).Text = poi.Name;

            if (String.IsNullOrEmpty(poi.Address))
                view.FindViewById<TextView>(Resource.Id.addrTextView).Visibility = ViewStates.Gone;
            else
                view.FindViewById<TextView>(Resource.Id.addrTextView).Text = poi.Address;

            if ((CurrentLocation != null) && (poi.Latitude.HasValue) && (poi.Longitude.HasValue))
            {
                Location poiLocation = new Location("");
                poiLocation.Latitude = poi.Latitude.Value;
                poiLocation.Longitude = poi.Longitude.Value;
                float distance = CurrentLocation.DistanceTo(poiLocation) * 0.000621371F;
                view.FindViewById<TextView>(Resource.Id.distanceTextView).Text = String.Format("{0:0,0.00} miles", distance);
            }
            else
            {
                view.FindViewById<TextView>(Resource.Id.distanceTextView).Text = "??";
            }

            //load image into image View
            Bitmap poiImage = POIData.GetImageFile (poi.Id.Value);
            view.FindViewById<ImageView> (Resource.Id.poiImageView).SetImageBitmap (poiImage);
            if (poiImage != null)  
                poiImage.Dispose ();

            return view;
        }



    }
}