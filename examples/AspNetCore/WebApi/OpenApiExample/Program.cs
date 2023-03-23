using ApiVersioning.Examples;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning( options =>
    {
        // (Optional) Set the default API version when a client does not specify one.
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion( 1, 0 );

        // Reporting API versions will return the headers
        // "api-supported-versions" and "api-deprecated-versions".
        options.ReportApiVersions = true;

        // (Optional) Support multiple options for specifying the API version.
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader( "api-version" ), // Default. Ex: ...?api-version=1.0
            new HeaderApiVersionReader( "X-Version" ), // Ex: X-Version: 1.0
            new MediaTypeApiVersionReader( "ver" ) ); // Ex: Accept: application/json; ver=1.0
        // See **Important Note** below.

        // (Optional) Example of how to create a Sunset Policy for API v0.9.
        options.Policies.Sunset( 0.9 )
            .Effective( DateTimeOffset.Now.AddDays( 60 ) )
            .Link( "policy.html" )
            .Title( "Versioning Policy" )
            .Type( "text/html" );
    } )
    //.AddMvc()
    .AddApiExplorer( options =>
    {
        // Add the versioned API explorer, which also adds IApiVersionDescriptionProvider service.
        // Note: The specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // Note: This option is only necessary when versioning by URL segment. The SubstitutionFormat
        // can also be used to control the format of the API version in route templates.
        // Ex: [Route("api/v{version:apiVersion}/[controller]")]
        options.SubstituteApiVersionInUrl = true;
        // **Important Note** This locks you into this format! You can't use the other options above.
        // As a dev team, decide if this is your preferred option for specifying the API version.
    } );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle.
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(
    options =>
    {
        // Add a custom operation filter which sets default values.
        options.OperationFilter<SwaggerDefaultValues>();

        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine( AppContext.BaseDirectory, fileName );

        // Integrate XML comments.
        options.IncludeXmlComments( filePath );
    } );

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        var descriptions = app.DescribeApiVersions();

        // Build a Swagger endpoint for each discovered API version.
        foreach ( var description in descriptions )
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint( url, name );
        }
    } );

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
///     Making Program public with Top-level statements.
/// </summary>
public partial class Program
{
}