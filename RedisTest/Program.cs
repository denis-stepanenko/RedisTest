using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase();
var ft = db.FT();
var json = db.JSON();

// Values
db.StringSet("foo", "bar");

Console.WriteLine(db.StringGet("foo"));

// Hashes

db.HashSet("claims",
[
    new HashEntry("user_name", "dennis smith"),
    new HashEntry("user_role", "manager"),
    new HashEntry("age", "25"),
    new HashEntry("address", "baker street")
]);

Console.WriteLine(db.HashGet("claims", "user_name"));

var claims = db.HashGetAll("claims");
Console.WriteLine(string.Join('\n', claims));

// Use Redis as document datatable

// Create index
// var schema = new Schema()
//     .AddTextField(new FieldName("$.Brand", "Brand"))
//     .AddTextField(new FieldName("$.Model", "Model"))
//     .AddTextField(new FieldName("$.Description", "Desciption"))
//     .AddNumericField(new FieldName("$.Price", "Price"))
//     .AddTagField(new FieldName("$.Condition", "Condition"));

// ft.Create(
//     "idx:bicycle",
//     new FTCreateParams().On(IndexDataType.JSON).Prefix("bicycle"),
//     schema
// );

// Add documents

// var bicycles = new List<Bicycle>
// {
//     new Bicycle("Brand 1", "Model 1", "Anything", 100, ""),
//     new Bicycle("Brand 2", "Model 2", "Anything", 100, ""),
//     new Bicycle("Brand 3", "Model 3", "Anything", 100, ""),
// };

// for (int i = 0; i < bicycles.Count; i++)
// {
//     json.Set($"bicycle:{i}", "$", bicycles[i]);
// }

// Find documents
// var query = new Query("@brand:\"Velorim\"");
// var res = ft.Search("idx:bicycle", query).Documents;
// Console.WriteLine(string.Join("\n", res.Select(x => x["json"])));


record Bicycle(string Brand, string Model, string Description, int Price, string Condition);