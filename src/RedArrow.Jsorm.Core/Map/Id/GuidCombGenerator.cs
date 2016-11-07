﻿using System;

namespace RedArrow.Jsorm.Core.Map.Id
{
	public class GuidCombGenerator : IIdentifierGenerator
	{
		private static volatile IIdentifierGenerator _instance;
		private static readonly object SyncRoot = new object();

		public static IIdentifierGenerator Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (SyncRoot)
					{
						if (_instance == null)
						{
							_instance = new GuidCombGenerator();
						}
					}
				}
				return _instance;
			}
		}

		private static readonly long BaseDateTicks = new DateTime(1900, 1, 1).Ticks;

		protected GuidCombGenerator()
		{
		}

		public string Generate()
		{
			var guidArray = Guid.NewGuid().ToByteArray();

			var now = DateTime.UtcNow;

			// Get the days and milliseconds which will be used to build the byte string 
			var days = new TimeSpan(now.Ticks - BaseDateTicks);
			var msecs = now.TimeOfDay;

			// Convert to a byte array 
			// Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
			var daysArray = BitConverter.GetBytes(days.Days);
			var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

			// Reverse the bytes to match SQL Servers ordering 
			Array.Reverse(daysArray);
			Array.Reverse(msecsArray);

			// Copy the bytes into the guid 
			Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
			Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

			return new Guid(guidArray).ToString();
		}
	}
}
