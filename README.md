![Argo](argo.png?raw=true)
# Argo
A Json.API 1.0 C# ORM client to translate Json.API semantics into familiar, friendly POCOs.

[![GitHub license](https://img.shields.io/github/license/redarrowlabs/argo.svg)](https://raw.githubusercontent.com/redarrowlabs/argo/development/LICENSE)

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
With the advent of NoSQL databases, we all need to decide how to relate these schemaless resources over web APIs.  This often introduces new challenges, like how to model relationships bewteen resources.  These challenges compound when you introduce filtering or paging and sorting features to your API. [JSON API](http://jsonapi.org/) is a specification to define a common approach to overcoming these challenges.  However, additional challenges arise when you realize you need to somehow map a JSON API resource to a POCO.  The goal of Argo is to solve all of these challenges (and then some!) for you.
## Why
Here is an example of a JSON API resource:
```javascript
{
    "type": "article",
    "id": "02e8ae9d-b9b5-40e9-9746-906f1fb26b64",
    "attributes": {
      "title": "argo and JSON API: a coming of age story"
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
Let's imagine we fetched the above resource from our JSON API spec web API and wanted to map it to our POCO.  First, notice `author` and `comments` in the json resource are not fully fleshed out objects, but just `id` and `type` - the minimum information needed to fetch (read: lazy load) thier respective resources from the web API.  Pseudo code to fetch `Comments` might look something like
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
Argo takes advantage of [Fody](https://github.com/Fody/Fody) to weave code into your POCO at compile time in order to bridge the gap between the POCO semantics developers expect and the JSON API json structure.

Advantages:
 1. Cross Platform!
 2. Most expensive reflection-based mapping logic is executed at compile time and not at runtime.
 3. Simple.  Argo targets netstandard 1.3

Most all ORMs leverage proxies to abstract your POCO from your database.  Unfortunately, the most popular .net proxy implementation, [Castle DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md), will not play nice with Xamarin iOS projects due to its use of `Reflection.Emit` APIs, which are not permitted on iOS.  We get around this by modifying the POCO itself at compile time instead of proxying it at runtime.

Maybe the thought of a 3rd party library modifying your code is scary.  Let's see some examples.
## Examples
Just show the code, right?  Below are some examples of how Argo transforms your POCO into a session-aware ORM proxy.  But first, some helpful domain language definitions:
 - Model: The c# poco representation of your domain entity that is used by your c# runtime
 - Resource: The Json.API representation of your domain entity that is used to communicate with a json.api web api
 - Session: A stateful instance that may be used to invoke one or many transactions with the API.  Models are cached for the duration of a session, so session lifetime should be scoped accordingly.  The longer a session lives, the larger the risk of stale data, conflicts, and contention.

### Model Weaving
Below, you'll find an example Model.  Models must be marked with the `[Model]` attribute, as well as contain a `System.Guid` property marked with the `[Id]` attribute.
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
Argo will weave some magic into your Model:
```csharp
[Model]
public class Person
{
    private static readonly string __argo__generated_include = "";
    private IModelSession __argo__generated_session;
    public IResourceIdentifier __argo__generated_Patch { get; set; }
    public IResourceIdentifier __argo__generated_Resource { get; set; }
    public bool __argo__generated_SessionManaged => this.__argo__generated_session != null && !this.__argo__generated_session.Disposed;

    private Guid id;
    [Id]
	public Guid Id
	{
	    get
	    {
	        return this.id;
	    }
	    private set
	    {
	        if(!this.__argo__generated_SessionManaged)
	        {
	            this.id = value;
	        }
	    }
    }

    public Person(IResourceIdentifier resource, IModelSession session)
    {
        this.__argo__generated_Resource = resource;
        this.__argo__generated_session = session;
        this.id = this.__argo__generated_session.GetId<Person>(this);
		
		this.firstName = __argo__generated_session.GetAttribute<Person, string>(this, "firstName");
		this.lastName = __argo__generated_session.GetAttribute<Person, string>(this, "lastName");
    }
	
	// property weaving explained below
}
```
Take note of the fields added to your Model.  `__argo__generated_Resource` is the backing Json.API Resource for this Model.  This repsresents the transport object used to communicate with the API.  Additionally, `__argo__generated_Patch` is also a Resource, but only contains the changes made to its Model since the last server sync.  Both of these reside directly in your Model, so you can easily inspect them when debugging.  `__argo__generated_include` specifies the relationships Argo should load during a GET operation - more on this later.

A ctor is added that will be only be used by the Session.  Notice `BestFriend` marked as `[HasOne]` was not initialized.  Relationships are lazy-loaded by default.

You may also notice the `Id` was enhanced a bit.  Model Ids are settable as long as the Model is not currently bound to a Session.  This means you may create a new Model, set its Id, and persist it.  However, if you use the Session to fetch a Model by Id, you may not change that Id anymore.

### Property Weaving
This Model property...
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
		if (this.__argo__generated_session != null && !string.Equals(this.firstName, value, StringComparison.Ordinal))
		{
			this.__argo__generated_session.SetAttribute<Person, string>(this, "firstName", this.firstName);
		}
		this.firstName = value;
	}
}
```
Notice the string equality check.  Most property setters have type-specific equality checks to verify the value is actually being changed before delegating that change to the Session.

The default naming convention for Model property to Resource attribute names is camel-case.  However, all names are overridable to allow you to chose your own naming convention.
```csharp
[Model("person")]
public class Actor
{
    ...
}
```
or
```csharp
[Property("first-name")]
public string FirstName { get; set; }
```
### Relationship Weaving
We can also relate models
```csharp
[HasOne]
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
		if(this.__argo__generated_session != null)
        {
            this.bestFriend = this.__argo__generated_session.GetReference<Person, Friend>(this, "bestFriend");
        }
        return this.bestFriend;
    }
    set
    {
        this.bestFriend = value;
        if(this.__argo__generated_session != null && this.bestFriend != value)
        {
            this.__argo__generated_session.SetReference<Person, Friend>(this, "bestFriend", this.bestFriend);
        }
    }
}
```
With the Model delegating to the Session, the Session can do a lot of cool stuff for us.
 - track changes for building `PATCH` requests
 - manage lazy-loading relationships via Session-managed collections in place of `IEnumerable<T>`
 - linq provider to allow Session-managed sorting, paging, and filtering on collections
 - cache retrieved models for subsequent gets

### Eager Loading
By default, relationships are lazy loaded.  You may override this behavior by specifying the `LoadStrategy` of a relationship.
```csharp
[HasOne(LoadStrategy.Eager)]
public Friend BestFriend { get; set; }
```
This will update the value of `__argo__generated_include` in your Model.  The Session will use this value for all GET requests for the respective Model type.  In this case, Argo would weave:
```csharp
private static readonly string __argo__generated_include = "bestFriend";
```
The Session will use the value `"bestFriend"` when retrieving the `Person` Model, so the data for both the primary `Person` and `Person.BestFriend` is retrieved in a single request.  For more info on this behavior, read up on the behavior of fetching included relationships in the [Json.API spec](http://jsonapi.org/format/#fetching-includes).

## Configuring
Argo gives you a pleasent, easy-to-understand configuration API.  If you've worked with [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate), this should look a little familiar.  This configuration step performs a potentially significant amount of reflection and caching and should only be performed once throughout your application's lifecycle, typically at startup.
```csharp
// the ISessionFactory is the long-lived object you would (ideally) register in your IoC container
var sessionFactory = Fluently.Configure("http://api.host.com")
	.Remote()
		// with Xamarin iOS apps, you may need to provide your own TLS-compatible HttpMessageHandler
		.Create(() => new HttpClient())
		// Argo will run these configuration actions on the HttpClient
		// whenever an `ISession` is built by the `ISessionFactory`
		.Configure(httpClient => httpClient
		    .DefaultRequestHeaders
		    .Authorization = new AuthenticationHeaderValue("Bearer", token))
		// and/or...
		.ConfigureAsync(() => YourTokenManagerInstance.GetAccessTokenAsync())
		// you may add callbacks to asynchronously inspect a *copy* of the HttpResponseMessage
        .OnHttpResponse(response => Task.CompletedTask)
        .OnResourceCreated(response => Task.CompletedTask)
        .OnResourceRetreived(response => Task.CompletedTask)
        .OnResourceUpdated(response => Task.CompletedTask)
        .OnResourceDeleted(response => Task.CompletedTask)
	.Models()
		// tell Argo where your models are
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
You may have requirements to provide Session-specific connection configuration.  For example, if authenticating as a user, the access token will typically not be valid forever and will be refreshed throughout the lifetime of your application.  As a result, the current access token value will typically need to be provided for each Session instance.
```csharp
using (var session = sessionFactory.CreateSession(YourHttpClientConfigureFunction))
{
    // do some stuff
}

private void YourHttpClientConfigureFunction(HttpClient client)
{
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "access token value")
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

## Name
In Greek mythology, [Argo](https://en.wikipedia.org/wiki/Argo) was the ship Jason and the Argonauts sailed in search of the golden fleece.

## Logo
[Sail Boat](https://thenounproject.com/term/sail-boat/17570/) designed by [Celia Jaber](https://thenounproject.com/celiajaber/) from The Noun Project
