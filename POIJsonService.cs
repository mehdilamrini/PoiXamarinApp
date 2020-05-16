using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace POIApp
{

    public class POIJsonService : IPOIDataService
    {
        private List<PointOfInterest> _pois = new List<PointOfInterest>();

        public IReadOnlyList<PointOfInterest> POIs
        {
            get { return _pois; }
        }

  

        public void DeletePOI(PointOfInterest poi)
        {

            if (File.Exists(GetFilename(poi.Id))) 
                File.Delete(GetFilename(poi.Id));

            // delete POI image file 
            if (File.Exists(GetImageFilename(poi.Id)))
                File.Delete (GetImageFilename(poi.Id));

            _pois.Remove(poi);

        }

        public PointOfInterest GetPOI(int id)
        {
            PointOfInterest poi = _pois.Find(p => p.Id == id);
            return poi;
        }

        public void RefreshCache()
        {
            _pois.Clear();
            string[] filenames = Directory.GetFiles(_storagePath, "*.json");

            foreach (string filename in filenames)
            {
                string poiString = File.ReadAllText(filename);
                PointOfInterest poi = JsonConvert.DeserializeObject<PointOfInterest>(poiString); _pois.Add(poi);
            }
        }

        public void SavePOI(PointOfInterest poi)
        {
            Boolean newPOI = false; if (!poi.Id.HasValue) { poi.Id = GetNextId(); newPOI = true; }  // serialize POI  
            string poiString = JsonConvert.SerializeObject (poi);  // write new file or overwrite existing file
            File.WriteAllText (GetFilename (poi.Id.Value), poiString);  // update cache if file save was successful  
            if (newPOI)    _pois.Add (poi);}

        private string _storagePath;
        public POIJsonService(string storagePath)
        {
            Log.Info("POIPath", storagePath);
            _storagePath = storagePath;  // create the storage path if it does not exist 
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath); 
            RefreshCache();
        }

        private int GetNextId() { if (_pois.Count == 0) return 1; else return _pois.Max(p => p.Id.Value) + 1; }
        private string GetFilename(int? id)
        {
            return Path.Combine(_storagePath, "poi" + id.ToString() + ".json");

        }

        public string GetImageFilename(int? id) {
            return Path.Combine(_storagePath, "poiimage" + id.ToString() + ".jpg"); }
    }
}