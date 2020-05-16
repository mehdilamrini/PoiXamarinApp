using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace POIApp
{
    public class POIData
    {
        public static readonly IPOIDataService Service = new POIJsonService(System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "POIApp"));
        public static Bitmap GetImageFile(int poiId)
        {
            string filename = Service.GetImageFilename(poiId);
            if (File.Exists(filename))
            {
                Java.IO.File imageFile = new Java.IO.File(filename);
                return BitmapFactory.DecodeFile(imageFile.Path);
            }
            else return null;
        }

    }
}