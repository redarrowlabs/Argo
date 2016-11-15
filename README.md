# jsorm (json orm)
A JSON API C# ORM client to abstract a JSON API 1.0 spec web api with familiar, friendly POCO semantics

[![GitHub license](https://img.shields.io/github/license/redarrowlabs/jsorm.svg)](https://raw.githubusercontent.com/redarrowlabs/jsorm/development/LICENSE)

| Branch | Nuget | Build |
| ------ | ----- | ----- |
| master | N/A | N/A |
| development | N/A | [![VSTS](https://img.shields.io/vso/build/redarrowlabs/23bc67da-86b0-4d8a-8b5b-8e999658a24f/117.svg)](https://redarrowlabs.visualstudio.com/Titan/Titan%20Team/_build/index?definitionId=117) |

## What
With the advent of NoSQL databases, we all need to decide how to relate these schemaless resources over web apis.  This often introduces new challenges, like how to model relationships bewteen resources.  These challenges compound when you introduce filtering or paging and sorting features to your api. [JSON API](http://jsonapi.org/) is a specification to define a common approach to overcoming these challenges.  However, additional challenges arise when you realize you need to somehow map a JSON API resource to a POCO.  The goal of jsorm is to solve all of these challenges (and then some!) for you.
## Why
Here is an example of a JSON API resource:
```javascript
{
    "type": "article",
    "id": "02e8ae9d-b9b5-40e9-9746-906f1fb26b64",
    "attributes": {
      "title": "jsorm and JSON API: a coming of age story"
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
This could be the poco version of the above resource:
```csharp
public class Article
{
    public Guid Id { get; set; }                        // => $.id
    public string Title { get; set; }                   // => $.attributes.title
    public Person Author { get; set; }                  // => $.relationships.author
    public IEnumerable<Comment> Comments { get; set; }  // => $.relationships.comments
}
```
Let's imagine we fetched the above resource from our JSON API spec web api and wanted to map it to our POCO.  First, notice `author` and `comments` in the json resource are not fully fleshed out objects, but just `id` and `type` - the minimum information needed to fetch (read: lazy load) thier respective resources from the web api.  Pseudo code to fetch `Comments` might look something like
```csharp
//fetch article with "dehydrated" comments
var article = await JsonApiClient.GetResourceAsync<Article>(articleId);
var commentIds = article.Comments.Select(x => x.Id); // all we have are the Ids!
// use commentIds to fetch the full comment resources from the web and hydrate our article comments
var comments = (await Task.WhenAll(commentIds
    .Select(x => JsonApiClient.GetResourceAsync<Comment>(x))))
    .ToArray();
article.Comments = comments;
```
Yuck.
## How
jsorm takes advantage of [Fody](https://github.com/Fody/Fody) to weave code into your POCO at compile time in order to bridge the gap between the POCO semantics developers expect and the JSON API json structure.

Advantages:
 1. Cross Platform!
 2. Most expensive reflection-based mapping logic is executed at compile time and not at runtime.
 3. Simple.  jsorm targets netstandard 1.3

Most all ORMs leverage proxies to abstract your POCO from your database.  Unfortunately, the most popular .net proxy implementation, [Castle DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md), will not play nice with Xamarin iOS projects due to its use of `Reflection.Emit` apis, which are not permitted on iOS.  We get around this by modifying the POCO itself at compile time instead of proxying it at runtime.

Maybe the thought of a 3rd party library modifying your code is scary.  Let's see some examples.
### Compiling
Your POCO might look something like this.  We'll need to add some attributes to tell jsorm how to map this model at compile time.
```csharp
// identifies a jsorm model
// all attributed classes/properties/etc are mapped to camel case json attributes to follow json conventions
// Person => "type": "person"
// FirstName => "attributes": { "firstName" : <value> }, etc.
// however, you may override these conventions - ex: [Model("your-override-type-name")]
[Model]
public class Person
{
	// the id property. jsorm requires Ids to be Guids.  it's for your own good!
	// id properties shouldn't have a public setter.  the id should be managed by jsorm
	// however, let's see what happens if we forget this, and add one...
	[Id]
	public Guid PersonId { get; set; }

	// identifies properties to be mapped to the "attributes" section in JSON API resource.
	// note: I wish I could call this [Attribute] but that has unfortunate .net api type/namespace collisions
	[Property]
	public string FirstName { get; set; }

	// you may override default naming conventions on anything
	[Property("lName")]
	public string LastName { get; set; }

	// I haven't implemented relationships yet...
}
```
jsorm runs as a Fody weaver during the compile process.  You'll see MSBUILD output informing you what models jsorm found and what properties it mapped.  To those new to concepts like code weaving, your assembly is built with your code and then modified/woven during a later step in the build pipeline.  Anywhere you reference that compiled assembly, it will contain the additional code woven in by jsorm and thus the additional behavior.  This saves you from writing that code yourself or executing expensive reflection operations at runtime to map POCO properties to JSON API attributes!
```
3>    Fody: Fody (version 1.29.4.0) Executing
3>      Fody/Weavers:   Jsorm scanner discovered model types:
3>      Fody/Weavers:   	AssemblyToWeave.Person
3>      Fody/Weavers:   Weaving type AssemblyToWeave.Person...
3>MSBUILD : warning : Fody/Weavers: AssemblyToWeave.Person either has no setter, or a public setter.  Attempting to resolve...
3>      Fody/Weavers:   Successfully added private setter to System.Guid AssemblyToWeave.Person::PersonId()
3>      Fody/Weavers:   	Weaving System.String AssemblyToWeave.Person::FirstName() => firstName
3>      Fody/Weavers:   	Weaving System.String AssemblyToWeave.Person::LastName() => lName
3>    Fody:   Finished Fody 747ms.
```
You'll see above that jsorm found our `Person` model and made some changes to it.  The `PersonId` property marked as the `Id` had a public setter.  For now, jsorm should be managing ids like any other traditional ORM, so some liberties are taken here to enforce this.  As an MSBUILD warning, this will show up in your Visual Studio errors list to hopefully draw attention to this issue.  If we were to now decompile the assembly built above, we would find our Person class modified as shown below:
```csharp
// a post jsorm woven Person, decompiled (with my added comments)
[Model]
public class Person
{
	// a jsorm session field is added to allow this POCO to act as a facade over the session
	[NonSerialized]
	private IModelSession _session;

	// the setter is made private, as ids should be managed by the ORM
	// a compiler warning notifies you to remove your public setter, if you have one
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
				// property 'FirstName' => attribute 'firstName' per convention
				this.<FirstName>k__BackingField = this._session.GetAttribute<Person, string>(this.PersonId, "firstName"); 
			}
			return this.<FirstName>k__BackingField;
		}
		set
		{
			this.<FirstName>k__BackingField = value;
			// update the session with the new value.  this allows the session to track and manage changes
			if (this._session != null)
			{
				this._session.SetAttribute<Person, string>(this.PersonId, "firstName", this.<FirstName>k__BackingField);
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
				this.<LastName>k__BackingField = this._session.GetAttribute<Person, string>(this.PersonId, "lName"); 
			}
			return this.<LastName>k__BackingField;
		}
		set
		{
			this.<LastName>k__BackingField = value;
			if (this._session != null)
			{
				this._session.SetAttribute<Person, string>(this.PersonId, "lName", this.<LastName>k__BackingField);
			}
		}
	}
	
	public Person()
	{
	}
	
	// additional ctor for session to use when activating managed models
	public Person(Guid id, IModelSession session)
	{
		this.Id = id;
		this._session = session;
	}
}
```
With the model delegating to the session, the session can do a lot of cool stuff for us.
 - track property changes for building `PATCH` requests
 - manage lazy-loading relationships via session-managed collections in place of `IEnumerable<T>`
 - linq provider to allow session-managed sorting, paging, and filtering on collections

### Configuring
jsorm gives you a pleasent, easy-to-understand configuration api.  If you've worked with [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate), this should look a little familiar.
```csharp
// the ISessionFactory is the long-lived object you would (ideally) register in your IoC container
var sessionFactory = Fluently.Configure()
	.Remote()
		// optionally, if you really want to control how the HttpClient is created each session, you can
		.Create(() => new HttpClient())
		// jsorm will run these configuration actions on the HttpClient for each session
		// don't forget to add a trailing slash!
		.Configure(x => x.BaseAddress = new Uri("http://json.host/api/"))
		// it's up to you to decide how you want to manage authentication
		.Configure(x => x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token))
	.Models()
		// tell jsorm where your models are
		.Configure(x => x.AddFromAssemblyOf<Person>())
		// and/or
		.Configure(x => x.AddFromAssembly(Assembly.GetExecutingAssembly()))
		// and/or
		.Configure(x => x.Add<Person>())
	.BuildSessionFactory();
```
### Using
```csharp
Guid crossSessionPersonId;
// ISession is a short-lived state/cache of request/responses
using (var session = sessionFactory.CreateSession())
{
	var person = new Person
	{
		FirstName = "Justin",
		LastName = "Case"
	};

	// sends a POST to the server and returns a session-managed Person
	person = await session.Create(person);
	// Id was created by the server and populated by the session
	crossSessionPersonId = person.PersonId;
	Assert.NotNull(crossSessionPersonId);
	Assert.NotEqual(Guid.Empty, crossSessionPersonId);
}
// later that day...
using (var session = sessionFactory.CreateSession())
{
	var person = await session.Get<Person>(crossSessionPersonId);
	Assert.Equal("Justin", person.FirstName);
	Assert.Equal("Case", person.LastName);
	
	// session receives this setter value and builds a patch context for this model
	person.FirstName = "Charity";
	// sends a PATCH to the server
	await session.Update(person);
}
// cleaning up...
using (var session = sessionFactory.CreateSession())
{
    // sends a DELETE to the server
    await session.Delete<Person>(crossSessionPersonId);
}
```
### The Future
We're still evaluating the long-term roadmap for this project, but initial, tentative ideas:
- Linq provider
  - sorting via OrderBy
  - paging via Take
  - filtering via Where
- Cache provider plugins with initial support for [Akavache](https://github.com/akavache/Akavache)
- server to client eventing/sync push via [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)
- your idea could go here...
