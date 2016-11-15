# jsorm (json orm)
A JSON API C# ORM client to abstract a JSON API 1.0 spec web api with familiar, friendly POCO semantics

[![GitHub license](https://img.shields.io/github/license/redarrowlabs/jsorm.svg)](https://raw.githubusercontent.com/redarrowlabs/jsorm/development/LICENSE)

| Branch | Nuget | Build | Test Coverage | Static Analysis |
| ------ | ----- | ----- | ------------- | --------------- |
| master | N/A | N/A | N/A | N/A |
| development | N/A | N/A | N/A | N/A |

## What
With the advent of NoSQL databases, we all need to decide how to relate these schema-less resources over web apis.  This often introduces new challenges, like how to model relationships bewteen resources.  These challenges compound when you want to introduce filtering or paging and sorting features to your api. [JSON API](http://jsonapi.org/) is a specification to define a common approach to these challenges.  More challenges arise when you realize you need to somehow map a JSON API document to a POCO.  The goal of jsorm is to solve all of these challenges (and then some!) for you.
## Why
Here is an example of a JSON API document:
```javascript
{
    "type": "article",
    "id": "02e8ae9d-b9b5-40e9-9746-906f1fb26b64",
    "attributes": {
      "title": "JSON API paints my bikeshed!"
    },
    "relationships": {
      "author": {
        "data": { "type": "person", "id": "2dba2b39-a419-41c1-8d3c-46eb1250dfe8" }
      },
      "comments": {
        "data": [
          { "type": "comment", "id": "d893ef57-7939-4b4e-9dfa-be4d1af27c5e" },
          { "type": "comment", "id": "c2ac33ab-e9fa-4fb4-9a63-24b168854023" }
        ]
      }
    }
}
```
This is what the poco version of this document:
```csharp
public class Article
{
    public Guid Id { get; set; }                        // => $.id
    public string Title { get; set; }                   // => $.attributes.title
    public Person Author { get; set; }                  // => $.relationships.author
    public IEnumerable<Comment> Comments { get; set; }  // => $.relationships.comments
}
```
Let's imagine we fetched the above json from our JSON API spec web api and tried to map it to our POCO.  First, notice `author` and `comments` on the json document are not the full object, but just `id` and `type` - the minimum information needed to fetch (read: lazy load) that related resource from the web api.  Pseudo code to fetch `Comments` might look something like
```csharp
//fetch article with "dehydrated" comments
var article = await JsonApiClient.GetResourceAsync<Article>(articleId);
var commentIds = article.Comments.Select(x => x.Id); // all we have are the Ids!
// use the commentIds to fetch the full comment resources from the web
var comments = (await Task.WhenAll(commentIds
    .Select(x => JsonApiClient.GetResourceAsync<Comment>(x))))
    .ToArray();
article.Comments = comments;
```
Yuck.
## How
jsorm takes advantage of [Fody](https://github.com/Fody/Fody) to weave code into your model POCO at compile time.  There are many advantages to this, namely expensive reflection-based mapping logic is executed at compile time, not at runtime.  Maybe the thought of a 3rd party library modifying your code is scary.  Let's see some examples.
### Compiling
```csharp
[Model] // denotes a jsorm model
public class Person
{
    [Id] // denotes the id property
    public PersonId
    [Property] // denotes properties to be mapped to the "attributes" section
    public string FirstName { get; set; }
    [Property("lName")] // you can override any default naming conventions
    public string LastName { get; set; }
    // ...I haven't implemented relationships yet
}
// a post jsorm woven Person, decompiled (with my added comments)
[Model]
public class Person
{
	[NonSerialized]
	private IModelSession _session; // a jsorm session field is added

    // the setter is made private, as this should be managed by the ORM
	// a compiler warning notifies you to remove your public Id setter, if you have one
	[Id]
	public Guid PersonId { get; private set; }

	[Property]
	public string FirstName
	{
		get
		{
		    // if this model is managed by the session, delegate
			if (this._session != null)
			{
			    // property 'FirstName' => attribute 'firstName' by convention
				this.<FirstName>k__BackingField = this._session.GetAttribute<Person, string>(this.Id, "firstName"); 
			}
			return this.<FirstName>k__BackingField;
		}
		set
		{
			this.<FirstName>k__BackingField = value;
			if (this._session != null)
			{
				this._session.SetAttribute<Person, string>(this.Id, "firstName", this.<FirstName>k__BackingField);
			}
		}
	}

	[Property]
	public string LastName
	{
		get
		{
			if (this._session != null)
			{
			    // property 'LastName' => attribute 'lName' per override
				this.<LastName>k__BackingField = this._session.GetAttribute<Person, string>(this.Id, "lName"); 
			}
			return this.<LastName>k__BackingField;
		}
		set
		{
			this.<LastName>k__BackingField = value;
			if (this._session != null)
			{
				this._session.SetAttribute<Person, string>(this.Id, "lName", this.<LastName>k__BackingField);
			}
		}
	}
	
	// additional ctor for session to use when creating managed models
	public Patient(Guid id, IModelSession session)
	{
		this.Id = id;
		this._session = session;
	}
}
```
With the model delegating to the session, the session can track property changes for building `PATCH` requests, manage lazy-loading via session-managed collections returned in place of `IEnumerable<TYourModel>` and ideally a linq provider can allow session-managed sorting, paging, and filtering on collections.  These are all familiar concepts if you're used to working with a modern ORM.
### Configuring
jsorm gives you a plesent, easy-to-discover configuration api.  If you've worked with [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate), this should look a little familiar.
```csharp
var sessionFactory = Fluently.Configure()
	.Remote()
		.Configure(x => x.BaseAddress = new Uri("http://json.host/api"))
		.Configure(x => x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token))
	.Models()
		.Configure(x => x.AddFromAssemblyOf<Person>())
	.BuildSessionFactory();
```
### Using
```csharp
Guid crossSessionPersonId;
using (var session = sessionFactory.CreateSession())
{
	var person = new Person
	{
		FirstName = "Justin",
		LastName = "Case"
	};
	
	// sends a POST to the server and returns session-managed Person
	person = await session.Create(person);
    // Id was created by the server and populated by the session
    crossSessionPersonId = person.Id;
    Assert.NotNull(crossSessionPersonId);
}
// later that day...
using (var session = sessionFactory.CreateSession())
{
	var person = await session.Get<Person>(crossSessionPersonId);
	Assert.Equal("Justin", person.FirstName);
	Assert.Equal("Case", person.LastName);
	
	// session receives this setter and builds a patch context for this model
	person.FirstName = "Charity";
	// sends a PATCH to the server
	await session.Update(person);
}
// and theeeenn.....
using (var session = sessionFactory.CreateSession())
{
    // sends a DELETE to the server
    await session.Delete<Person>(crossSessionPersonId);
}
```