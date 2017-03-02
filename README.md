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

## Domain Language
Before going any further, it's important to establish some common language to describe these rather abstract concepts:
 - Model: The C# POCO representation of your domain entity that is used by your c# runtime
 - Resource: The Json.API representation of your domain entity that is used to communicate with a Json.API compatible web API
 - Session: A stateful instance used to invoke one or many transactions with the API.

## Why
Here is an example of a JSON API resource:
```javascript
{
    "type": "person",
    "id": "02e8ae9d-b9b5-40e9-9746-906f1fb26b64",
    "attributes": {
      "firstName": "Nick",
	  "lastname": "O'Tyme",
	  "age": 42
    },
    "relationships": {
      "bestFriend": {
        "data": { "type": "person", "id": "2dba2b39-a419-41c1-8d3c-46eb1250dfe8" }
      },
      "friends": {
        "data": [
          { "type": "person", "id": "d893ef57-7939-4b4e-9dfa-be4d1af27c5e" },
          { "type": "person", "id": "2dba2b39-a419-41c1-8d3c-46eb1250dfe8" },
          { "type": "person", "id": "c2ac33ab-e9fa-4fb4-9a63-24b168854023" }
        ]
      }
    }
}
```
This could be the Model of the above Resource:
```csharp
public class Person
{
	public Guid Id { get; set; }						// => $.id
	public string FirstName { get; set; }				// => $.attributes.firstName
	public string LastName { get; set; }				// => $.attributes.lastName
	public int Age { get; set; }						// => $.attributes.age
	public Person BestFriend { get; set; }				// => $.relationships.bestFriend.data.???
	public ICollection<Person> Friends { get; set; }	// => $.relationships.friends.data.???
}
```
Let's imagine we fetched the above Resource from our Json.API compatible web API and wanted to map it to our Model.  First, notice `bestFriend` and `friends` in the Resource do not have the information we need to fully hydrate these properties, but just `id` and `type` - the minimum information needed to fetch (read: lazy load) thier respective Resources from the web API.  We would need to make additional requests to the server to fetch the necessary Resources for our `BestFriend` and `Friends` properties.  As object models grow and become more complex, this becomes increasingly difficult and expensive to manage.  Also, notice the same Resource with id `2dba2b39-a419-41c1-8d3c-46eb1250dfe8` is not only our `bestFriend`, but is also included in our `friends` collection, of course.  We don't want to fetch this Resource from the server twice!  Ideally, we fetch it once and cache it somewhere to be referenced from either property as needed.

## How
Argo takes advantage of [Fody](https://github.com/Fody/Fody) to weave code into your POCO Model at compile time in order to bridge the gap between the POCO semantics developers expect and the Json.API json structure.

Advantages:
 1. Cross Platform! Argo targets netstandard 1.4
 2. Most expensive reflection-based mapping is executed at compile time..  This allows post-startup runtime opterations to have minimal overhead.
 3. Simple.  Argo does all the heavy lifting so you don't have to!

Most ORMs leverage proxies to abstract your POCO from a database, for example.  Unfortunately, .net proxy implementations will not play nice with Xamarin iOS projects due to their use of `Reflection.Emit` APIs, which are not permitted on iOS.  Argo aims to be a cross-platform library, so we get around this by modifying the POCO itself at compile time instead of proxying it at runtime.

## Examples
Maybe the thought of a 3rd party library modifying your code is scary.  It may be scary at first, but you'll soon realize it's awesome.  Bring on the examples!

### Model Weaving
Take a look at this example Model.  Models must be marked with a `[Model]` attribute, as well as contain a `System.Guid` property marked with the `[Id]` attribute.
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
	[HasMany]
	public ICollection<Person> Friends { get; set; }
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
That may be a lot for you to process.  Let's break it down:
 - `__argo__generated_session` the Session instance for this Model.  Mutative operations are delegated to the Session.
 - `__argo__generated_include` specifies the relationships Argo should eagerly load during a GET operation - more on this later.
 - `__argo__generated_Resource` is the backing Json.API Resource for this Model.  This repsresents the transport object used to communicate with the API.
 - `__argo__generated_Patch` is also a Resource, but only contains the changes made to its Model since the last communication with the server.
Having the Resource and the Patch directly in the Model is hugely powerful for developers.  While debugging, you can see the base Resource that was retrieved from the server, as well as any pending deltas not yet flushed to the server in the Patch.
 
A ctor is added that should only be invoked by the Session.  Notice `BestFriend` marked as `[HasOne]` was not initialized.  Relationships are lazy-loaded by default.

You may also notice the `Id` was enhanced a bit.  Model Ids are settable as long as the Model is not currently bound to a Session.  This means you may create a new Model, set its Id, and persist it.  However, if you use the Session to fetch a Model by Id, that Model is now managed by a Session and you may not change that Id anymore for data consistency reasons.

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

The default naming convention from Model property to Resource attribute names is camel-case.  As shown above, a Model property `FirstName` will be mapped to a Resource attribute `firstName`.  However, all names are overridable to allow you to chose your own naming convention destiny.
```csharp
[Model("person")]
public class Actor
{
	[Property("first-name")]
	public string FirstName { get; set; }
}
```
### Relationship Weaving
We can also relate models
```csharp
[HasOne]
public Person BestFriend { get; set; }
```
will be woven into
```csharp
private Person bestFriend;
[HasOne]
public Person BestFriend
{
    get
    {
		if(this.__argo__generated_session != null)
        {
            this.bestFriend = this.__argo__generated_session.GetReference<Person, Person>(this, "bestFriend");
        }
        return this.bestFriend;
    }
    set
    {
        this.bestFriend = value;
        if(this.__argo__generated_session != null && this.bestFriend != value)
        {
            this.__argo__generated_session.SetReference<Person, Person>(this, "bestFriend", this.bestFriend);
        }
    }
}
```
With the Model delegating to the Session, the Session can do a lot of great stuff for us.
 - cache retrieved models for subsequent gets
 - track changes for building `PATCH` requests
 - manage lazy-loading relationships via Session-managed collections in place of `IEnumerable<T>` or `ICollection<T>`
 - linq provider to allow Session-managed sorting, paging, and filtering of collections

### Eager Loading
By default, relationships are lazy-loaded.  You may override this behavior by specifying the `LoadStrategy` of a relationship.
```csharp
[HasOne(LoadStrategy.Eager)]
public Person BestFriend { get; set; }
```
This will update the value of `__argo__generated_include` in your Model.  The Session will use this value for all GET requests for the respective Model type.  In this case, Argo would weave:
```csharp
private static readonly string __argo__generated_include = "bestFriend";
```
The Session will use the value `"bestFriend"` when retrieving the a Resource of type `person`, so the data for both the primary `Person` Model and `Person.BestFriend` Model is retrieved in a single request.  For more info on this behavior, read up on fetching included relationships in the [Json.API spec](http://jsonapi.org/format/#fetching-includes).

## Configuring
Argo gives you a pleasent, easy-to-understand configuration API.  If you've worked with [Fluent NHibernate](https://github.com/jagregory/fluent-nhibernate), this should look a little familiar.  This configuration step performs a potentially significant amount of reflection and caching and should only be performed once throughout your application's lifecycle, typically at startup.  Aside from telling Argo where to scan for your `[Model]`s, all of this is optional.  The idea is to offer a large amount of options and flexibility to meet your application's unique needs.
```csharp
// the ISessionFactory is the long-lived object you would (ideally) register in your IoC container
var sessionFactory = Fluently.Configure("http://api.host.com")
	.Remote()
		.Create(() => new HttpClient()) // not recommended
		.UseMessageHandler(() => new TlsCompatibleMessageHandler())
		.Configure(httpClient => httpClient
		    .DefaultRequestHeaders
		    .Authorization = new AuthenticationHeaderValue("Bearer", token))
		.ConfigureAsync(() => YourTokenManagerInstance.GetAccessTokenAsync())
        .OnHttpResponse(async response => await SomethingAsync())
        .OnResourceCreated(async response => await SomethingAsync())
        .OnResourceRetreived(async response => await SomethingAsync())
        .OnResourceUpdated(async response => await SomethingAsync())
        .OnResourceDeleted(async response => await SomethingAsync())
	.Models()
		.Configure(scan => scan.AssemblyOf<Person>())
		.Configure(scan => scan.Assembly(Assembly.GetExecutingAssembly()))
	.BuildSessionFactory();
```
If that's not enough configurability, you have full access to the HttpClient when a new Session is created to do any last-minute configuration.
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
	// fetch our person from the server
	var person = await session.Get<Person>(crossSessionPersonId);
	Assert.Equal("Justin", person.FirstName);
	Assert.Equal("Case", person.LastName);
	
	// session receives this setter value and creates a patch for this model
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
- ~~Configurable eager loading~~
- Cache provider plugins with initial support for [Akavache](https://github.com/akavache/Akavache)
- server to client eventing/sync push via [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)
- your idea could go here...

## Name
In Greek mythology, [Argo](https://en.wikipedia.org/wiki/Argo) was the ship Jason and the Argonauts sailed in search of the golden fleece.

## Logo
[Sail Boat](https://thenounproject.com/term/sail-boat/17570/) designed by [Celia Jaber](https://thenounproject.com/celiajaber/) from The Noun Project
