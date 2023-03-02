using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var nextJokeId = 0;
app.MapGet("/", () => "Hello World!");

var recipes = new ConcurrentDictionary<int, Recipe>();

app.MapGet("/recipes", () => recipes.Values);

app.MapPost("/recipes", (RecipeDto recipeDto) =>
{
    var newId = Interlocked.Increment(ref nextJokeId);

    var recipe = new Recipe
    {
        Id = newId,
        Title = recipeDto.Title,
        Ingredients = recipeDto.Ingredients,
        Description = recipeDto.Description,
        ImageUrl = recipeDto.ImageUrl
    };
    if (!recipes.TryAdd(newId, recipe))
    {
        //This should never happen!!!!
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
    // Return Created (201)

    return Results.Created($"/recipes/{newId}", recipe);
});


app.MapDelete("recipes/{id}", (int id) =>
{
    if (recipes.TryRemove(id, out var recipe))
    {
        return Results.Ok(recipe);
    }
    return Results.NotFound();
});


app.MapGet("/recipes/filterByTitle/{filter}", (string filter) =>
{
    var filteredRecipes = recipes.Values.Where(r => r.Title.Contains(filter, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(filteredRecipes);
});

app.MapGet("/recipes/filterByIngredient/{ingredient}", (string ingredient) =>
{
    var filteredRecipes = recipes.Values.Where(r => r.Ingredients.Any(i => i.Name.Contains(ingredient, StringComparison.OrdinalIgnoreCase)));
    return Results.Ok(filteredRecipes);
});

app.MapPut("/recipes/{id}", (int id, RecipeDto recipeDto) =>
{
    var recipe = new Recipe
    {
        Id = id,
        Title = recipeDto.Title,
        Ingredients = recipeDto.Ingredients,
        Description = recipeDto.Description,
        ImageUrl = recipeDto.ImageUrl
    };
    if (recipes.TryUpdate(id, recipe, recipes[id]))
    {
        return Results.Ok(recipe);
    }
    return Results.NotFound();
});

app.Run();





class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<Ingredients>? Ingredients { get; set; }
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
};

class Ingredients
{
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; }
};

record RecipeDto(int Id, string Title, List<Ingredients> Ingredients, string Description, string ImageUrl)
{

}