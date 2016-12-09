# jsorm (json orm)
A JSON API C# ORM client to abstract a JSON API 1.0 spec web api with familiar, friendly POCO semantics

[![GitHub license](https://img.shields.io/github/license/redarrowlabs/jsorm.svg)](https://raw.githubusercontent.com/redarrowlabs/jsorm/development/LICENSE)

#### Weaver
| Branch | Nuget | Build |
| ------ | ----- | ----- |
| master | N/A | N/A |
| development | N/A | [![VSTS](https://img.shields.io/vso/build/redarrowlabs/23bc67da-86b0-4d8a-8b5b-8e999658a24f/117.svg)](https://redarrowlabs.visualstudio.com/Titan/Titan%20Team/_build/index?definitionId=117) |

#### Client
| Branch | Nuget | Build |
| ------ | ----- | ----- |
| master | N/A | N/A |
| development | N/A | [![VSTS](https://img.shields.io/vso/build/redarrowlabs/23bc67da-86b0-4d8a-8b5b-8e999658a24f/153.svg)](https://redarrowlabs.visualstudio.com/Titan/Titan%20Team/_build/index?definitionId=153) |


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
## Examples
Just show the code, right?  Below are some examples of how jsorm transforms your POCO into a session-aware ORM proxy.
```csharp
[Model]
public class Person
{
    [Id]
    public Guid Id { get; set; }
	[Property]
	public string FirstName { get; set; }
	[Property]
	public string LastName { get; set; }
	[Property]
	public int Age { get; set; }
	[HasOne]
	public Friend BestFriend { get; set; }
}
```
At a minimum, each model needs an `[Id]` defined.  Jsorm will weave this class into:
```csharp
[Model]
public class Person
{
    private IModelSession __jsorm_session;

    [Id]
	public Guid Id { get; set; }

    public Person(Guid id, IModelSession session)
    {
        this.Id = id;
        this.__jsorm_session = session;
		
		this.firstName = __jsorm_session.GetAttribute<Person, string>(this.Id, "firstName");
		this.lastName = __jsorm_session.GetAttribute<Person, string>(this.Id, "lastName");
    }
	
	// properties explained below
}
```
A ctor is added that will be only be used by the session.  Notice `BestFriend` marked as `[HasOne]` was not initialized.  Relationships are lazy-loaded by default.  (Configurable eager loading coming soon...)

A basic property:
```csharp
[Property]
public string FirstName { get; set; }
```
becomes...
```csharp
private string firstName;
[Property]
public string FirstName
{
	get
	{
		return this.firstName;
	}
	set
	{
		this.firstName = value;
		if (this.__jsorm_session != null && !string.Equals(this.firstName, value, StringComparison.Ordinal))
		{
			this.__jsorm_session.SetAttribute<Person, string>(this.Id, "firstName", this.firstName);
		}
	}
}
```
Notice the ordinal comparison string equality check.  All attributes are also overridable to allow you to chose your own naming convention to map to your json.api convention.
```csharp
[Property("first-name")]
public string FirstName { get; set; }
```
becomes...
```csharp
private string firstName;
[Property]
public string FirstName
{
	get
	{
		return this.firstName;
	}
	set
	{
		if (this.__jsorm_session != null && !string.Equals(this.firstName, value, StringComparison.Ordinal))
		{
			this.__jsorm_session.SetAttribute<Person, string>(this.Id, "first-name", this.firstName);
		}
		this.firstName = value;
	}
}
```
We can also relate models
```csharp
public Friend BestFriend { get; set; }
```
will be woven into
```csharp
private Friend bestFriend;
[HasOne]
public Friend BestFriend
{
    get
    {
		if(this.__jsorm_session != null)
        {
            this.bestFriend = this.__jsorm_session.GetReference<Person, Friend>(this.Id, "bestFriend");
        }
        return this.bestFriend;
    }
    set
    {
        this.bestFriend = value;
        if(this.__jsorm_session != null && !object.Equals(this.bestFriend, value))
        {
            this.__jsorm_session.SetReference<Person, Friend>(this.Id, "bestFriend", this.bestFriend);
        }
    }
}
```

With the model delegating to the session, the session can do a lot of cool stuff for us.
 - track property changes for building `PATCH` requests
 - manage lazy-loading relationships via session-managed collections in place of `IEnumerable<T>`
 - linq provider to allow session-managed sorting, paging, and filtering on collections
 - cache retrieved models for subsequent gets

## Configuring
jsorm gives you a pleasent, easy-to-understand configuration api.  If you've worked with [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate), this should look a little familiar.
```csharp
// the ISessionFactory is the long-lived object you would (ideally) register in your IoC container
var sessionFactory = Fluently.Configure("http://api.host.com")
	.Remote()
		// with Xamarin iOS apps, you may need to provide your own TLS-compatible HttpMessageHandler
		.Create(() => new HttpClient())
		// jsorm will run these configuration actions on the HttpClient
		// whenever an `ISession` is built by the `ISessionFactory`
		.Configure(httpClient => httpClient
		    .DefaultRequestHeaders
		    .Authorization = new AuthenticationHeaderValue("Bearer", token))
		// and/or...
		.ConfigureAsync(() => YourTokenManagerInstance.GetAccessTokenAsync())
	.Models()
		// tell jsorm where your models are
		.Configure(scan => scan.AssemblyOf<Person>())
		// and/or...
		.Configure(scan => scan.Assembly(Assembly.GetExecutingAssembly()))
	.BuildSessionFactory();
```
## In Action
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
	crossSessionPersonId = person.Id;
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
	person.BestFriend = new Friend
	{
	    FirstName = "Keri",
	    LastName = "Oki"
	};
	// 1. first send POST to create new Friend
	// 2. update Person.BestFriend relationship with new Friend.Id
	// 3. send PATCH to update Person FirstName and BestFriend relationship
	await session.Update(person);
}
// cleaning up...
using (var session = sessionFactory.CreateSession())
{
    // sends a DELETE to the server (no cascade options yet)
    await session.Delete<Person>(crossSessionPersonId);
}
```
## The Future
We're still evaluating the long-term roadmap for this project, but initial tentative ideas:
- Linq provider
  - sorting via OrderBy
  - paging via Take
  - filtering via Where
- Configurable eager loading
- Cache provider plugins with initial support for [Akavache](https://github.com/akavache/Akavache)
- server to client eventing/sync push via [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)
- your idea could go here...

