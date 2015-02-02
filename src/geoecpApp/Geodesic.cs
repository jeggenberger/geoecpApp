using System;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Geometry;

namespace geoecpApp
{
	/// <summary>
	///     Extension methods for geodesic calculations.
	/// </summary>
	public static class Geodesic
	{
		private const double EarthRadius = 6378.137; //kilometers. Change to miles to return all values in miles instead 


		/// <summary>
		///     Gets the shortest path line between two points. THe line will be following the great
		///     circle described by the two points.
		/// </summary>
		/// <param name="start">The start point.</param>
		/// <param name="end">The end point.</param>
		/// <returns></returns>
		public static Polyline GetGeodesicLine(MapPoint startIn, MapPoint endIn)
		{
			Polyline line;

			var start = GeometryEngine.Project(startIn, SpatialReferences.Wgs84) as MapPoint;
			var end = GeometryEngine.Project(endIn, SpatialReferences.Wgs84) as MapPoint;

			if (Math.Abs(end.X - start.X) <= 180) // Doesn't cross dateline  
			{
				PointCollection pnts = GetGeodesicPoints(start, end);
				line = new Polyline(pnts);
			}
			else
			{
				double lon1 = start.X/180*Math.PI;
				double lon2 = end.X/180*Math.PI;
				double lat1 = start.Y/180*Math.PI;
				double lat2 = end.Y/180*Math.PI;
				double latA = LatitudeAtLongitude(lat1, lon1, lat2, lon2, Math.PI)/Math.PI*180;
				//double latB = LatitudeAtLongitude(lat1, lon1, lat2, lon2, -180) / Math.PI * 180; 

				IList<PointCollection> parts = new List<PointCollection>();

				parts.Add(GetGeodesicPoints(start, new MapPoint(start.X < 0 ? -180 : 180, latA, start.SpatialReference)));
				parts.Add(GetGeodesicPoints(new MapPoint(start.X < 0 ? 180 : -180, latA, start.SpatialReference), end));

				line = new Polyline(parts);
			}
			return GeometryEngine.Project(line, SpatialReferences.WebMercator) as Polyline;
		}

		private static PointCollection GetGeodesicPoints(MapPoint start, MapPoint end)
		{
			double lon1 = start.X/180*Math.PI;
			double lon2 = end.X/180*Math.PI;
			double lat1 = start.Y/180*Math.PI;
			double lat2 = end.Y/180*Math.PI;
			double dX = end.X - start.X;
			var points = (int) Math.Floor(Math.Abs(dX));
			dX = lon2 - lon1;
			var pnts = new PointCollection(start.SpatialReference);
			pnts.Add(start);
			for (int i = 1; i < points; i++)
			{
				double lon = lon1 + dX/points*i;
				double lat = LatitudeAtLongitude(lat1, lon1, lat2, lon2, lon);
				pnts.Add(new MapPoint(lon/Math.PI*180, lat/Math.PI*180, start.SpatialReference));
			}
			pnts.Add(end);
			return pnts;
		}

		/// <summary>
		///     Gets the latitude at a specific longitude for a great circle defined by p1 and p2.
		/// </summary>
		/// <param name="p1">The p1.</param>
		/// <param name="p2">The p2.</param>
		/// <param name="lon">The longitude in degrees.</param>
		/// <returns></returns>
		private static double LatitudeAtLongitude(MapPoint p1, MapPoint p2, double lon)
		{
			double lon1 = p1.X/180*Math.PI;
			double lon2 = p2.X/180*Math.PI;
			double lat1 = p1.Y/180*Math.PI;
			double lat2 = p2.Y/180*Math.PI;
			lon = lon/180*Math.PI;
			return LatitudeAtLongitude(lat1, lon1, lat2, lon2, lon)/Math.PI*180;
		}

		/// <summary>
		///     Gets the latitude at a specific longitude for a great circle defined by lat1,lon1 and lat2,lon2.
		/// </summary>
		/// <param name="lat1">The start latitude in radians.</param>
		/// <param name="lon1">The start longitude in radians.</param>
		/// <param name="lat2">The end latitude in radians.</param>
		/// <param name="lon2">The end longitude in radians.</param>
		/// <param name="lon">The longitude in radians for where the latitude is.</param>
		/// <returns></returns>
		private static double LatitudeAtLongitude(double lat1, double lon1, double lat2, double lon2, double lon)
		{
			return Math.Atan((Math.Sin(lat1)*Math.Cos(lat2)*Math.Sin(lon - lon2)
			                  - Math.Sin(lat2)*Math.Cos(lat1)*Math.Sin(lon - lon1))/
			                 (Math.Cos(lat1)*Math.Cos(lat2)*Math.Sin(lon1 - lon2)));
		}

		/// <summary>
		///     Gets the true bearing at a distance from the start point towards the new point.
		/// </summary>
		/// <param name="start">The start point.</param>
		/// <param name="end">The point to get the bearing towards.</param>
		/// <param name="distanceKM">The distance in kilometers travelled between start and end.</param>
		/// <returns></returns>
		public static double GetTrueBearing(MapPoint start, MapPoint end, double distanceKM)
		{
			double d = distanceKM/EarthRadius; //Angular distance in radians 
			double lon1 = start.X/180*Math.PI;
			double lat1 = start.Y/180*Math.PI;
			double lon2 = end.X/180*Math.PI;
			double lat2 = end.Y/180*Math.PI;
			double tc1;
			if (Math.Sin(lon2 - lon1) < 0)
				tc1 = Math.Acos((Math.Sin(lat2) - Math.Sin(lat1)*Math.Cos(d))/(Math.Sin(d)*Math.Cos(lat1)));
			else
				tc1 = 2*Math.PI - Math.Acos((Math.Sin(lat2) - Math.Sin(lat1)*Math.Cos(d))/(Math.Sin(d)*Math.Cos(lat1)));
			return tc1/Math.PI*180;
		}

		/// <summary>
		///     Gets the point based on a start point, a heading and a distance.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="distanceKM">The distance KM.</param>
		/// <param name="heading">The heading.</param>
		/// <returns></returns>
		public static MapPoint GetPointFromHeading(MapPoint start, double distanceKM, double heading)
		{
			double brng = heading/180*Math.PI;
			double lon1 = start.X/180*Math.PI;
			double lat1 = start.Y/180*Math.PI;
			double dR = distanceKM/6378.137; //Angular distance in radians 
			double lat2 = Math.Asin(Math.Sin(lat1)*Math.Cos(dR) + Math.Cos(lat1)*Math.Sin(dR)*Math.Cos(brng));
			double lon2 = lon1 +
			              Math.Atan2(Math.Sin(brng)*Math.Sin(dR)*Math.Cos(lat1), Math.Cos(dR) - Math.Sin(lat1)*Math.Sin(lat2));
			double lon = lon2/Math.PI*180;
			double lat = lat2/Math.PI*180;
			while (lon < -180) lon += 360;
			while (lat < -90) lat += 180;
			while (lon > 180) lon -= 360;
			while (lat > 90) lat -= 180;
			return new MapPoint(lon, lat, start.SpatialReference);
		}
	}
}