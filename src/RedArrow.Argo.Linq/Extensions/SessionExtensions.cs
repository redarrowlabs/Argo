﻿using System.Linq;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq.Extensions
{
	internal static class SessionExtensions
	{
		public static IQueryable<TModel> CreateQuery<TModel>(this ISession session)
			where TModel : class
		{
			return new RemoteQueryable<TModel>();
		}
	}
}
