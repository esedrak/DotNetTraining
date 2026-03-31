# Challenges

This directory contains all student exercises for the .NET Training workshop.

```
Challenges/
├── Basics/       # Core C# language fundamentals
├── Bank/         # .NET Bank HTTP & Data layer quests
└── Temporal/     # Durable Workflow & Agentic .NET quest
```

## Basics/

The **[Basics Challenges: Detective Briefs](Basics/README.md)** are short exercises covering core C# building blocks, interfaces, concurrency, and testing.

Each challenge is a short mystery. You will encounter different types of quests:
- **FixMe**: Buggy code is provided. Your task is to identify the problem and fix it.
- **ImplMe**: You'll find `throw new NotImplementedException()` stubs. Your task is to implement the method to make the tests pass.

Run tests with: `dotnet test tests/Basics.Tests`

## Bank/

The **[.NET Bank Transfer Quest](Bank/README.md)** is your introduction to building production API handlers.

You'll implement the `POST /v1/transfers` API endpoint in a pre-scaffolded service, focusing on:
- Idiomatic HTTP handler patterns using ASP.NET Core.
- Entity Framework Core for data persistence.
- JWT authentication and scope-based authorisation.
- Table-driven unit testing for service and controller layers.

## Temporal/

The **[Durable Transfer Quest](Temporal/README.md)** is the final, high-stakes challenge!

You will transform the bank transfer into a robust **Distributed Transaction** using Temporal. This module focuses on:
- **Agentic Engineering:** Using specialized AI tools to build complex logic.
- **Workflow Orchestration:** Implementing the Compensation Pattern and Human-in-the-loop approvals.
- **Durable Reliability:** Ensuring idempotency and surviving worker restarts.
- **Full-Lifecycle Testing:** Unit, Integration, and the "Gold Standard" Replay tests.

Evaluate your work using the **[Competition Grading Rubric](Temporal/GRADING_PROMPT.md)**.

## Your Exploration Journey

Ready to put your skills to the test? We've prepared a series of challenges that increase in complexity as you progress through the training.

Start with the basics: **[Challenges: Detective Briefs](Basics/README.md)**.

---
[← Back to Main README](../../README.md)
