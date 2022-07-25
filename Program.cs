using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

//retrieve port from environment variables
var port = builder.Configuration["PORT"];

//set listening urls
builder.WebHost.UseUrls($"http://*:{port};http://localhost:3000");

//build application
builder.Services.AddSingleton<IMongoCollection<Person>>(s =>
{
    //retrieve connection string from environment variables
    var client = new MongoClient(s.GetService<IConfiguration>()!["MONGO_URL"]);
    var database = client.GetDatabase("test");
      return database.GetCollection<Person>("people");
});

var app = builder.Build();

app.MapGet("/",()=>"Hello world 🥂");

app.MapGet("/{name}", async (string name,IMongoCollection<Person> collection) =>
{
    // find users bearing same name
   var result = await collection
   .Find(Builders<Person>.Filter.Eq(x=>x.name,name))
   .Project(Builders<Person>.Projection.Expression(x=>new Person(x.name,x.age)))
   .ToListAsync();

    // return 404 if not found
   if(result == null) return Results.NotFound();
   
   //return result
   return Results.Ok(result);
});
app.MapPost("/create", async (Person person, IMongoCollection<Person> collection) =>
{
    //crete user
    await collection.InsertOneAsync(new Person(person.name,person.age));

    // return 202
    return Results.Accepted();
});

app.Run();
internal record Person(string name, int age);

