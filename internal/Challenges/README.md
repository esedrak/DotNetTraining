# 🎯 Challenges

Practice exercises to reinforce the workshop concepts.

---

## Basics/FixMe — Debug the Bugs

Intentionally buggy C# code. Your job: make the tests pass.

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~FixMe"
```

Bugs to find and fix:
- Race conditions with shared mutable state
- Deadlock from blocking async code synchronously
- Null reference exceptions
- Off-by-one errors
- Improper resource disposal

---

## Basics/ImplMe — Implement the Stubs

Method stubs with `throw new NotImplementedException()`. Your job: implement them.

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ImplMe"
```

Things to implement:
- Generic collection methods
- Async pipeline
- Custom middleware

---

## Bank/Transfer Quest

The main challenge: implement `POST /v1/transfers` end-to-end.

The transfer endpoint is partially stubbed. Complete the implementation:

1. Add `Transfer` entity validation to `Bank.Domain`
2. Add `CreateTransferAsync` to `IBankRepository` and `PostgresBankRepository`
3. Add `CreateTransferAsync` to `IBankService` and `BankService`
4. Wire up `POST /v1/transfers` in `TransferController`
5. Write unit tests for the service layer
6. Write an integration test for the endpoint

```bash
dotnet test tests/Bank.Tests --filter "FullyQualifiedName~Transfer"
```

---

## Tips

- Start with `FixMe` to warm up debugging skills
- `ImplMe` covers generics, async, and middleware
- The Bank Transfer Quest is the capstone — it touches all layers
