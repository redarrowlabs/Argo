using System;
using System.Collections.Generic;
using RedArrow.Jsorm.Core.Map;
using Xunit;

namespace RedArrow.Jsorm.Core.Tests.Map
{
	public class ResourceMapTests
	{
		[Fact]
		public void Id__Given_PropertyGuid__Then()
		{
			var subject = new SubjectWithGuidId();

			Assert.NotNull(subject.IdMap);
			Assert.Equal(nameof(IdModel<Guid>.ModelId), subject.IdMap.PropertyName);
			Assert.Equal(typeof (Guid), subject.IdMap.PropertyType);
			Assert.Equal(nameof(IdModel<Guid>.ModelId), subject.IdMap.AttributeName);
		}

		[Fact]
		public void Id__Given_PropertyString__Then()
		{
			var subject = new SubjectWithStringId();

			Assert.NotNull(subject.IdMap);
			Assert.Equal(nameof(IdModel<string>.ModelId), subject.IdMap.PropertyName);
			Assert.Equal(typeof (string), subject.IdMap.PropertyType);
			Assert.Equal(nameof(IdModel<string>.ModelId), subject.IdMap.AttributeName);
		}

		[Fact]
		public void Attribute__Given_PropertyStruct__Then()
		{
			var subject = new SubjectWithStructAttribute();

			var propName = nameof(AttributeModel<Guid>.ModelAttribute);

			var map = subject.AttributeMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (Guid), map.PropertyType);
			Assert.Equal(propName, map.AttributeName);
		}

		[Fact]
		public void Attribute__Given_PropertyStruct__When_AttrName__Then()
		{
			var expectedAttrname = "test";

			var subject = new SubjectWithStructAttribute(expectedAttrname);

			var propName = nameof(AttributeModel<Guid>.ModelAttribute);

			var map = subject.AttributeMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (Guid), map.PropertyType);
			Assert.Equal(expectedAttrname, map.AttributeName);
		}

		[Fact]
		public void Attribute__Given_PropertyModel__Then()
		{
			var subject = new SubjectWithModelAttribute();

			var propName = nameof(AttributeModel<HasOneModel>.ModelAttribute);

			var map = subject.AttributeMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (HasOneModel), map.PropertyType);
			Assert.Equal(propName, map.AttributeName);
		}

		[Fact]
		public void Attribute__Given_PropertyModel__When_AttrName__Then()
		{
			var expectedAttrname = "test";

			var subject = new SubjectWithModelAttribute(expectedAttrname);

			var propName = nameof(AttributeModel<HasOneModel>.ModelAttribute);

			var map = subject.AttributeMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (HasOneModel), map.PropertyType);
			Assert.Equal(expectedAttrname, map.AttributeName);
		}

		[Fact]
		public void HasOne__Given_Model__Then_PropMustBeClass()
		{
			var subject = new SubjectWithHasOne();

			var propName = nameof(HasOneModel.ModelHasOne);

			var map = subject.HasOneMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (IdModel<Guid>), map.PropertyType);
			Assert.Equal(propName, map.AttributeName);
		}

		[Fact]
		public void HasOne__Given_Model__When_AttrName__Then_PropMustBeClass()
		{
			var expectedAttrname = "test";

			var subject = new SubjectWithHasOne(expectedAttrname);

			var propName = nameof(HasOneModel.ModelHasOne);

			var map = subject.HasOneMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (IdModel<Guid>), map.PropertyType);
			Assert.Equal(expectedAttrname, map.AttributeName);
		}

		[Fact]
		public void HasMany__Given_Model__Then_PropMustBeIEnumerable()
		{
			var subject = new SubjectWithHasMany();

			var propName = nameof(HasManyModel.ModelHasManyEnumerable);

			var map = subject.HasManyMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (IEnumerable<HasOneModel>), map.PropertyType);
			Assert.Equal(propName, map.AttributeName);
		}

		[Fact]
		public void HasMany__Given_Model__When_AttrName__Then_PropMustBeIEnumerable()
		{
			var expectedAttrname = "test";

			var subject = new SubjectWithHasMany(expectedAttrname);

			var propName = nameof(HasManyModel.ModelHasManyEnumerable);

			var map = subject.HasManyMaps[propName];

			Assert.NotNull(map);
			Assert.Equal(propName, map.PropertyName);
			Assert.Equal(typeof (IEnumerable<HasOneModel>), map.PropertyType);
			Assert.Equal(expectedAttrname, map.AttributeName);
		}

		#region Subjects

		private class SubjectWithGuidId : ResourceMap<IdModel<Guid>>
		{
			public SubjectWithGuidId()
			{
				Id(x => x.ModelId);
			}
		}

		private class SubjectWithStringId : ResourceMap<IdModel<string>>
		{
			public SubjectWithStringId()
			{
				Id(x => x.ModelId);
			}
		}

		private class SubjectWithStructAttribute : ResourceMap<AttributeModel<Guid>>
		{
			public SubjectWithStructAttribute(string attrName = null)
			{
				Attribute(x => x.ModelAttribute, attrName);
			}
		}

		private class SubjectWithModelAttribute : ResourceMap<AttributeModel<HasOneModel>>
		{
			public SubjectWithModelAttribute(string attrName = null)
			{
				Attribute(x => x.ModelAttribute, attrName);
			}
		}

		private class SubjectWithHasOne : ResourceMap<HasOneModel>
		{
			public SubjectWithHasOne(string attrName = null)
			{
				HasOne(x => x.ModelHasOne, attrName);
			}
		}

		private class SubjectWithHasMany : ResourceMap<HasManyModel>
		{
			public SubjectWithHasMany(string attrName = null)
			{
				// this just tests that many common collection types compile
				HasMany(x => x.ModelHasManyEnumerable, attrName);
				HasMany(x => x.HasOneModelArray);
				HasMany(x => x.ModelHasManyList);
				HasMany(x => x.ModelHasManySet);
			}
		}

		#endregion Subjects

		#region Models

		private class IdModel<TId>
		{
			public TId ModelId { get; set; }
		}

		private class AttributeModel<TAttribute>
		{
			public TAttribute ModelAttribute { get; set; }
		}

		private class HasOneModel
		{
			public IdModel<Guid> ModelHasOne { get; set; }
		}

		private class HasManyModel
		{
			public HasOneModel[] HasOneModelArray { get; set; }
			public IList<HasOneModel> ModelHasManyList { get; set; }
			public IEnumerable<HasOneModel> ModelHasManyEnumerable { get; set; }
			public ISet<HasOneModel> ModelHasManySet { get; set; }
		}

		#endregion Models
	}
}
