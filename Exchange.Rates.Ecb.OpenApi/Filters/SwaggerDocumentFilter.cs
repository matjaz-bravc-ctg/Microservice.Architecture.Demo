using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exchange.Rates.Ecb.OpenApi.Filters;

/// <summary>
/// Corresponding to Controller's API document description information
/// </summary>
public class SwaggerDocumentFilter : IDocumentFilter
{
  private const string DOCS_URI = "https://frankfurter.dev/";

  public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
  {
    var tags = new List<OpenApiTag>
          {
              new()
              {
                  Name = "ExchangeRatesEcb",
                  Description = "ECB reference exchange rates (Frankfurter API)",
                  ExternalDocs = new OpenApiExternalDocs
                  {
                      Description = "Read more",
                      Url = new Uri(DOCS_URI)
                  }
              }
          };

    // Sort in ascending order by AssemblyName
    swaggerDoc.Tags = new HashSet<OpenApiTag>(tags.OrderBy(x => x.Name));
  }
}