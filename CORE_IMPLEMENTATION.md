# Core Module - DDD Base Classes

## 📦 O que foi implementado

O módulo **Core** contém as abstrações fundamentais da arquitetura DDD que são compartilhadas entre todos os Bounded Contexts.

### ✅ Arquivos Criados

#### 1. `Domain/ValueObject.cs`
- Classe base para Value Objects
- Implementação de igualdade por valor (não por referência)
- Migrado da estrutura antiga (Domain/Common/ValueObject.cs)

```csharp
public class IsbnNumber : ValueObject
{
    public string Value { get; }
    
    public IsbnNumber(string value) => Value = value;
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### 2. `Domain/IDomainEvent.cs`
- Interface para marcar eventos de domínio
- Classe base `DomainEvent` com `OccurredAt` e `EventId`
- Necessária para eventos que ocorrem nos agregados

```csharp
public class BookCreatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    
    public BookCreatedEvent(Guid bookId, string title)
    {
        BookId = bookId;
        Title = title;
    }
}
```

#### 3. `Domain/Entity.cs`
- Classe base para Entidades com identidade única
- Genérica em TId (ValuObject)
- Gerencia Domain Events
- Propriedades de auditoria: CreatedAt, UpdatedAt, IsActive

```csharp
public class User : Entity<UserId>
{
    public string Email { get; private set; }
    
    public User(UserId id, string email)
    {
        Id = id;
        Email = email;
        RaiseDomainEvent(new UserCreatedEvent(id.Value, email));
    }
}
```

#### 4. `Domain/AggregateRoot.cs`
- Classe base para Aggregate Roots
- Estende Entity<TId> com versionamento otimista
- Métodos `Deactivate()` e `Reactivate()` para soft delete
- Evento `AggregateDeactivatedEvent` automático

```csharp
public class Book : AggregateRoot<BookId>
{
    public string Title { get; private set; }
    
    public void Deactivate()
    {
        // Chama a base, que levanta AggregateDeactivatedEvent
        base.Deactivate();
    }
}
```

#### 5. `Domain/DomainException.cs`
- Exceção especializada para invariantes violadas
- Diferente de FluentResults (para erros esperados)
- Usa-se apenas quando algo realmente inesperado ocorre

```csharp
// ✅ Correto: erro esperado → FluentResults
if (user == null)
    return Result.Fail($"Usuário {userId} não encontrado");

// ✅ Correto: invariante violada → DomainException
if (book.AvailableCopies > book.TotalCopies)
    throw new DomainException(
        "Invariante violada",
        aggregateType: "Book",
        invariantDescription: "AvailableCopies não pode exceder TotalCopies");
```

#### 6. `Application/OperationResult.cs`
- Classe base simples para retornar resultados
- `OperationResult` (sem valor) e `OperationResult<T>`
- Alternativa light ao FluentResults (pode coexistir)

```csharp
public async Task<OperationResult<Book>> CreateBookAsync(CreateBookCommand command)
{
    var book = Book.Create(command.Title, ...);
    await repository.AddAsync(book);
    return OperationResult<Book>.Success(book);
}
```

---

## 📐 Estrutura de Pastas

```
backend/src/Core/
├── Core.csproj (net10.0)
├── Domain/
│   ├── ValueObject.cs
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   ├── IDomainEvent.cs
│   └── DomainException.cs
└── Application/
    └── OperationResult.cs
```

---

## 🎯 Como Usar no Catalog Context

### Passo 1: Criar ValueObjects

```csharp
// Catalog.Domain/ValueObjects/IsbnNumber.cs
using Core.Domain;

public class IsbnNumber : ValueObject
{
    public string Value { get; }
    
    public IsbnNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 13)
            throw new ArgumentException("ISBN deve ter 13 caracteres");
        Value = value;
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

// Catalog.Domain/Aggregates/Book/BookId.cs
public class BookId : ValueObject
{
    public Guid Value { get; }
    
    public BookId(Guid value) => Value = value;
    
    public static BookId New() => new(Guid.NewGuid());
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Passo 2: Criar Aggregates

```csharp
// Catalog.Domain/Aggregates/Book/Book.cs
using Core.Domain;

public class Book : AggregateRoot<BookId>
{
    public string Title { get; private set; }
    public IsbnNumber Isbn { get; private set; }
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }
    
    private Book() { }  // Construtor vazio para ORM
    
    public static Result<Book> Create(
        string title,
        IsbnNumber isbn,
        int totalCopies)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail("Título é obrigatório");
        
        if (totalCopies <= 0)
            return Result.Fail("Total de cópias deve ser > 0");
        
        var book = new Book
        {
            Id = BookId.New(),
            Title = title.Trim(),
            Isbn = isbn,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };
        
        book.RaiseDomainEvent(new BookCreatedEvent(book.Id.Value, title));
        return Result.Ok(book);
    }
    
    public Result UpdateAvailability(int quantity)
    {
        if (AvailableCopies + quantity > TotalCopies)
            return Result.Fail("Quantidade excede o total");
        
        AvailableCopies += quantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
}

// Catalog.Domain/DomainEvents/BookCreatedEvent.cs
public class BookCreatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    
    public BookCreatedEvent(Guid bookId, string title)
    {
        BookId = bookId;
        Title = title;
    }
}
```

### Passo 3: Herança e Compilação

Ao referenciar Core no Catalog:
```bash
dotnet add src\Catalog\Catalog.Domain\Catalog.Domain.csproj reference src\Core\Core.csproj
dotnet build  # Validar que compila
```

---

## 📋 Checklist de Implementação por Contexto

Ao implementar cada contexto, seguir este padrão:

- [ ] **Domain/Aggregates/** - Criar AggregateRoots herdando de `AggregateRoot<TId>`
- [ ] **Domain/ValueObjects/** - Criar ValueObjects herdando de `ValueObject`
- [ ] **Domain/DomainEvents/** - Criar eventos herdando de `DomainEvent`
- [ ] **Application/Write/** - CommandHandlers que orquestram agregados
- [ ] **Application/Read/** - QueryHandlers que retornam DTOs
- [ ] **Infrastructure/** - Implementar repositories e serviços

---

## 🔧 Adições Futuras ao Core (Conforme Necessário)

Se novos padrões emergirem:

- [ ] `Application/Behaviors/` - Behaviors genéricos do MediatR
- [ ] `Application/Mappers/` - Interfaces de mapeadores
- [ ] `Infrastructure/EntityConfigurations/` - Base para EF Core Fluent API

---

## ✅ Status

- ✅ Core compilando: **0 Erros, 4 Warnings** (warnings do código legado)
- ✅ Referências corretas em todos os contextos
- ✅ Pronto para começar implementação do Catalog

---

## 🚀 Próximo Passo

Implementar **Catalog.Domain** com:
1. Agregados: Book, Contributor, Publisher, Category
2. ValueObjects: IsbnNumber, etc
3. Repositories interfaces
4. Domain Events

Tempo estimado: **4-6 horas**
