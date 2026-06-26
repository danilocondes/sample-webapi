using PostHog;
using PostHog.AspNetCore;
using PostHog.Config;
using PostHogSample.Api.FeatureManagement;
using PostHogSample.Api.Options;
using PostHogSample.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<PostHogAdminApiOptions>(options =>
{
    builder.Configuration.GetSection(PostHogAdminApiOptions.SectionName).Bind(options);

    if (string.IsNullOrWhiteSpace(options.PersonalApiKey))
    {
        options.PersonalApiKey = builder.Configuration["PostHog:PersonalApiKey"];
    }
});

builder.AddPostHog(options =>
{
    options.UseFeatureManagement<ApiFeatureFlagContextProvider>();
});

builder.Services.AddHttpClient<IPostHogAdminApiClient, PostHogAdminApiClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePostHogRequestContext();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation(
    "PostHog Sample API started. Configure PostHog:ProjectToken and optional PostHogAdminApi settings.");

app.Run();
