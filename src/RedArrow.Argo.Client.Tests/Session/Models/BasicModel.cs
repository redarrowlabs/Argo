using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Models
{
	[Model]
    public class BasicModel
	{
		[Id]
		public Guid Id { get; set; }

		[Property]
		public string PropA { get; set; }
		[Property]
		public string PropB { get; set; }
	}
}
