# JsonStorage

JsonStorage is a simple (and not thread safe, yet) document storage mechanisame for wp7 mango applications. JsonStrorage provides a simple way of storing and querying classes within your application.

JsonStorage is not a fully fledged Database, but a simple way of storing and retrieving classes from within a wp7 application. Its a prefect way of saving and query a small set of data, in the 100's of MB. If you need to store and retrive GB's of data you will need to look elsewhere.



## getting started

1. Download the JsonStorage project and Built it and then reference it within your application.
2. Register the classes you want to save with JsonStorage

	var storage = StorageManager.Instance;
	storage.RegisterDocument<User>();

	This allows json storage to create the document store and index files for your class, or if these have already been created it will load the index into memory ready for querying. You should do this within the App.xaml.cs InitializePhoneApplication() method.

3. Annotate the fields in your class you want to query and/or Order on on:

		public class User 
		{
			public string Id { get; set; }

			[Index("string")]
			public string EmailAddress { get; set; }

			public string Name { get; set; }

			[Index("DateTime")]
			public DateTime Created { get; set; }
		}

	So here we have only indexed the EmailAddress and Created fields and we will only be able to query and/or order on these fields. 

	NOTE: The Id field is required as JsonStorage will use this to uniquely identify individual instances of your documents. JsonStorage will not create the Id for you, its recommended that you use a Guid for the Id field.


4. Store a document

		storage.Store<User>(new User {
				Id = Guid.NewGuid().ToString(),
				EmailAddress = "andy@tyfu.co.uk",
				Name = "Andy Long",
				Created = DateTime.Now
		});


5. Finally run a Query for some users

		storage.Query<User>()
		  	   .Where(x => ((string)x.Fields["EmailAddress"]).Contains("tyfu.co.uk"))
	      	   .OrderBy<DateTime>(x => x.Fields["Created"] as DateTime)
		  	   .Execute();

	This query finds all users who have an email address containing tyfu.co.uk and then orders them by the date on which the account was created. The Where() and OrderBy() both methods both expose the underlying index for the documents which is a list of Dictionary<string, object> where the Key is the indexed property and the Value is the value of the field.

	NOTE: Execute() will return an IEnumerable, this will keep a lock on the db file which stores all the User documents, this will prevent any writes and/or deletes from occuring until this Enumerable is disposed. If you want to release this lock dispose of the IEnumarable or call .Execute().ToList(). The IEnumerable is exposed as it allows data to be bound to a view asyncronously, so you don't have to worry about paging long lists of data.



### Additional functionality

Get a count of all your documents

	storage.Count<User>();

Get a document by its Id

	storage.GetById<User>(id);

Delete a document

	storage.Delete<User>(id);

Page your result set

	storage.Query<User>()
	       .Skip(15)
	       .Take(15)
	       .Execute();

Get statistics on your query

	JsonQueryStatistics statistics = null;

	storage.Query<User>()
	       .Statistics(out statistics)
	       .Take(15)
	       .Execute();

	statistics.TotalDocuments   // returns the total amount of documents of this type stored
	statistics.QueryResults     // returns 15 as thats is the amount of documents we retrived with the Query

Query with Projections

	storage.Query<User>()
	       .Excute<ProjectedUser>();

NOTE: Projected user class has to be a subset of the fields contained with the user class, i.e.

	public class ProjectedUser 
	{
		public string EmailAddress { get; set; }
		public string Name { get; set; }
		public DateTime LastLoginDate { get; set; } // this field will not be populated as it doesn't exist in the User class
	}

OrderByDecending

	storage.Query<User>()
		   .Where(x => ((string)x.Fields["EmailAddress"]).Contains("tyfu.co.uk"))
	       .OrderByDecending<DateTime>(x => x.Fields["Created"] as DateTime)
		   .Execute();



## Future plans

### Improved Querying

Hide the underying index implementation and allow users to query on the class itself, i.e.
		
	.Where(x => x.EmailAddress.Contains("tyfu.co.uk"))
	.OrderBy(x => x.Created)

Just need to get around to writing a Lambda parser...


### Improve the creation of Indexes and multiple indexes

Remove the Annotations for Indexing and allow users to create indexes with a Lambda, i.e.

	var storage = StorageManager.Instance;
	storage.CreateIndex<User>(x => new { 
			EmailAddress = x.EmailAddress,
			Created = x.Created
	});

This would also allow users to create multiple indexes on the same document and then select which index you want to run your query against



### Improve Projections

Allow the users to define how a document projection works, at the moment the projection only works if the class we are projecting into is a subset of the original class, it would be nice to be able to create a mapping to do the projection so they don't have to match.

i.e. 

	storage.Query<User>()
	       .Excute<ProjectedUser>(x => new ProjectedUser {
	        	Email = x.EmailAddress,
	        	Name  = x.Name,
	        	DateCreated = x.Created.ToString("dd MMM yy")
	       });

or to be able to create this in the index, so we can create a Map function as part of te index.