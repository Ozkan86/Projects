using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions> //Swagger yapılandırmasını özelleştirmek için kullanılan bir sınıf
{
    private readonly IApiVersionDescriptionProvider _provider; //API'nin hangi sürümlerinin mevcut olduğunu ve hangilerinin kullanımdan kaldırıldığını belirlemek için kullanılır

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider; //kurucu enjeksiyonu

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions) //mevcut API sürümlerinin bir listesini döner
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description)); //API sürüm bilgilerini oluşturur ve Swagger belgelerine ekler
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description) //API sürüm bilgilerini belirli bir sürüm için oluşturur
    {
        var info = new OpenApiInfo() // Swagger belgesi için başlık, sürüm ve açıklama gibi bilgileri içerir.
        {
            Title = "Eventify API",
            Version = description.ApiVersion.ToString(),
            Description = "Sürümlemeyi destekleyen örnek bir API"
        };

        if (description.IsDeprecated) //API sürümünün kullanımdan kaldırılıp kaldırılmadığını kontrol eder
        {
            info.Description += " - Bu sürüm kullanımdan kaldırılmıştır.";
        }

        return info;
    }
}
