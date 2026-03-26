# 🏦 Bank Transfer Quest

**Goal:** Implement `POST /v1/transfers` end-to-end.

The endpoint exists but most of the implementation is stubbed with `// TODO`. Complete it layer by layer, writing tests as you go.

---

## The Challenge

```
POST /v1/transfers
Body: { "fromAccountId": "...", "toAccountId": "...", "amount": 100.00 }

Success (201): Transfer created, balance moved from → to
Not Found (404): Source or destination account doesn't exist
Unprocessable (422): Insufficient funds
Bad Request (400): Invalid input (negative amount, same account)
```

---

## Layer by Layer

### 1. Domain — `src/Bank.Domain/Transfer.cs`

Verify `Transfer` has correct validation in the constructor:
- `amount` must be positive
- `fromAccountId` must differ from `toAccountId`

### 2. Repository — `src/Bank.Repository/`

Verify `IBankRepository` has:
```csharp
Task<Transfer> CreateTransferAsync(Transfer transfer, CancellationToken ct = default);
Task UpdateTransferAsync(Transfer transfer, CancellationToken ct = default);
```

Implement them in `PostgresBankRepository`.

### 3. Service — `src/Bank.Service/BankService.cs`

Implement `CreateTransferAsync`:
1. Fetch `from` and `to` accounts (throw `AccountNotFoundException` if missing)
2. Call `from.Withdraw(amount)` (throws `InsufficientFundsException` if needed)
3. Call `to.Deposit(amount)`
4. Set `transfer.Complete()` or `transfer.Fail(reason)`
5. Persist via `repository.CreateTransferAsync(transfer)`

### 4. API — `src/Bank.Api/Controllers/TransferController.cs`

Verify `POST /v1/transfers` handles:
- `AccountNotFoundException` → 404
- `InsufficientFundsException` → 422
- `ArgumentException` → 400
- Success → 201 with Location header

### 5. Tests

**Unit test** (`tests/Bank.Tests/Services/TransferServiceTests.cs`):
```csharp
[Fact] public async Task CreateTransfer_MovesBalance_BetweenAccounts()
[Fact] public async Task CreateTransfer_Throws_WhenFromAccountNotFound()
[Fact] public async Task CreateTransfer_Throws_WhenInsufficientFunds()
```

**Integration test** (`tests/Bank.Tests/Api/TransferApiTests.cs`):
```csharp
[Fact] public async Task PostTransfer_Returns201_WhenValid()
[Fact] public async Task PostTransfer_Returns404_WhenAccountNotFound()
[Fact] public async Task PostTransfer_Returns422_WhenInsufficientFunds()
```

---

## Running Tests

```bash
dotnet test tests/Bank.Tests --filter "FullyQualifiedName~Transfer"
```
