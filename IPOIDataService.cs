using System;
using System.Collections.Generic;

namespace POIApp
{
	public interface IPOIDataService
	{
		public IReadOnlyList<PointOfInterest> POIs { get; }
		public void RefreshCache();
		public PointOfInterest GetPOI(int id);
		public void SavePOI(PointOfInterest poi);
		public void DeletePOI(PointOfInterest poi);

		string GetImageFilename(int? id);
	}
}

