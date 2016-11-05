using System;
using System.Linq.Expressions;
using System.Net.Configuration;
using RedArrow.Jsorm.Core.Map;
using Xunit;

namespace RedArrow.Jsorm.Core.Tests.Map
{
	public class PropertyMapTests
	{
		[Fact]
		public void PropertyMap__Given_Expression__Then_Initialize()
		{
			var propName = nameof(Model.Name);

			var subject = new TestablePropertyMap(x => x.Name);
			
			Assert.Equal(propName, subject.PropertyName);
			Assert.Equal(propName, subject.AttributeName);
			Assert.Equal(typeof(string), subject.PropertyType);
		}

		[Fact]
		public void PropertyMap__Given_Expression__When_AttrName__Then_Initialize()
		{
			var attrName = "attr";
			var propName = nameof(Model.Name);

			var subject = new TestablePropertyMap(x => x.Name, attrName);
			
			Assert.Equal(propName, subject.PropertyName);
			Assert.Equal(attrName, subject.AttributeName);
			Assert.Equal(typeof(string), subject.PropertyType);
		}

		private class TestablePropertyMap : PropertyMap<Model, string>
		{
			public TestablePropertyMap(Expression<Func<Model, string>>  property, string attrName = null)
				: base(property, attrName)
			{
			}
		}

		private class Model
		{
			public string Name { get; set; }
		}
	}
}
