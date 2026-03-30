// 👋 Hello — DotNetTraining introduction app
//
// Run with: dotnet run --project src/Hello
//           make run-hello

Console.WriteLine("Hello, .NET World!");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
Console.WriteLine($"Time: {DateTime.UtcNow:O}");

