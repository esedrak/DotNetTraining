# API Contracts (OpenAPI)

This directory contains OpenAPI specs for reference and comparison.

The Bank API uses **code-first** design — the live spec is generated automatically from C# annotations (`[ProducesResponseType]`, XML `<summary>` tags) by .NET's built-in OpenAPI support. You can always inspect it from a running API instance:

```bash
make run-bank-api
```

- **Visual UI (Scalar):** http://localhost:5069/scalar/v1
- **Raw JSON:** http://localhost:5069/openapi/v1.json

## What's inside?

- **[accounts.yaml](accounts.yaml)**: A hand-written reference spec for the Accounts API. Useful for understanding OpenAPI structure and comparing against the generated output.

## Further Reading

- [Microsoft.AspNetCore.OpenApi — built-in OpenAPI (.NET 9+)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview)
- [Scalar for ASP.NET Core](https://scalar.com/blog/scalar-for-aspnet-core)
- [Swashbuckle / Swagger in ASP.NET Core (.NET 8 and earlier)](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)
- [Contract-First vs Code-First](../../workshop/Fundamentals/ApiFundamentals/04-contract-first-vs-code-first.md)
