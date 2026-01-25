# Análise de Arquitetura: DDD + CQRS Modularizado por Bounded Contexts

## 1. ESTADO ATUAL DO PROJETO

### 1.1 Estrutura Existente
```
backend/
├── API/              (Presentation Layer)
├── Application/      (Application Layer - MediatR com CQRS parcial)
├── Domain/          (Domain Layer - Bounded Contexts)
└── Infrastructure/  (Infrastructure Layer - Persistência)
```

### 1.2 Bounded Contexts Identificados
1. **Catalog** (Catálogo de Livros)
   - Entidades: Book, Contributor, Publisher, Category, BookContributor
   - BD: SQL (RelationalDbModel)

2. **Auth** (Identity & Access Management)
   - Entidades: User, Role, Permission, RefreshToken
   - BD: NoSQL/MongoDB (DocumentDbModel)

3. **Loan** (Empréstimos de Livros)
   - Entidades: BookCheckout
   - BD: SQL (RelationalDbModel)
   - **Problema**: Referencia direto para User (contexto Auth) e Book (contexto Catalog)

4. **Report** (Auditoria & Relatórios)
   - Entidades: AuditEntry
   - BD: NoSQL/MongoDB (DocumentDbModel)
   - **Função**: Apenas leitura para auditoria

### 1.3 Padrões Já Presentes
✅ MediatR (Command/Query Pattern - fundação para CQRS)
✅ FluentValidation (Validações)
✅ Separação em camadas (API, Application, Domain, Infrastructure)
✅ Multiple Database Contexts (SQL + NoSQL)
✅ Dependency Injection Pattern

### 1.4 Problemas Identificados

#### 🔴 **Crítico: Violação de Bounded Context**
```
BookCheckout (Loan Context)
└── Referencia direta: User (Auth Context)
└── Referencia direta: Book (Catalog Context)
```
**Impacto**: Acoplamento direto entre contextos, dificulta separação futura em serviços.

#### 🟡 **Auth no NoSQL**
- Contexto de escrita em NoSQL é subótimo
- Bancos de dados de identidade tipicamente usam SQL para ACID
- Faz sentido apenas se houver grande volume de leitura

#### 🟡 **CQRS Incompleto**
- MediatR está presente mas não há separação de Commands e Queries
- Não há otimização de leitura (sem read models específicas)
- Sem sincronização entre SQL e NoSQL para o contexto Auth

#### 🟡 **Acoplamento na Infrastructure**
```csharp
// Infrastructure/DependencyInjections.cs
// Registra services de TODOS os contextos em um único lugar
AddCatalogManagementServices()
AddLoanManagementServices()
AddReportManagement()
AddUserManagementServices()
```

---

## 2. RECOMENDAÇÕES: ESTRUTURA PROPOSTA (SIMPLIFICADA)

### 2.1 Princípios Fundamentais da Arquitetura

A estrutura proposta se baseia em:
- **Clean Architecture** (Robert Martin): Camadas independentes e testáveis
- **Domain-Driven Design** (Eric Evans): Bounded Contexts como unidades independentes
- **CQRS** (Greg Young): Separação explícita de Commands (escrita) e Queries (leitura)
- **Modular Monolith**: Múltiplos módulos acoplados apenas via interfaces/adapters

**Princípio chave**: Cada Bounded Context é **auto-contido** com suas próprias camadas (Domain → Application → Infrastructure), mas compartilha uma camada de **Core** para conceitos fundamentais.

### 2.2 Nova Estrutura por Bounded Contexts (Enxuta e Escalável)

```
backend/
├── src/
│   │
│   ├── API/                                (Presentation Layer - único ponto de entrada)
│   │   ├── API.csproj
│   │   ├── Program.cs
│   │   ├── Extensions/
│   │   ├── Middlewares/
│   │   └── Endpoints/
│   │       ├── Catalog/
│   │       ├── Auth/
│   │       ├── Loan/
│   │       └── Report/
│   │
│   ├── Core/                               (Código compartilhado - APENAS essencial)
│   │   ├── Core.csproj
│   │   ├── Domain/
│   │   │   ├── AggregateRoot.cs (classe base)
│   │   │   ├── Entity.cs (classe base)
│   │   │   ├── ValueObject.cs (classe base)
│   │   │   ├── DomainEvent.cs (interface)
│   │   │   └── IDomainEventHandler.cs (interface)
│   │   └── Application/
│   │       ├── Common/
│   │       │   ├── Behaviors/
│   │       │   │   └── ValidationBehavior.cs
│   │       │   ├── Mappers/
│   │       │   └── Exceptions/
│   │       │       ├── DomainException.cs
│   │       │       └── ValidationException.cs
│   │       └── Result.cs ou FluentResults wrapper
│   │
│   ├── Catalog/                            (Bounded Context 1)
│   │   ├── Catalog.Domain.csproj
│   │   │   ├── Aggregates/
│   │   │   │   ├── Book/
│   │   │   │   │   ├── Book.cs (AggregateRoot)
│   │   │   │   │   ├── BookId.cs (ValueObject)
│   │   │   │   │   └── BookStatus.cs (Enum)
│   │   │   │   ├── Contributor/
│   │   │   │   ├── Publisher/
│   │   │   │   └── Category/
│   │   │   ├── ValueObjects/
│   │   │   ├── DomainEvents/
│   │   │   └── Repositories/
│   │   │       └── IBookRepository.cs (interface)
│   │   │
│   │   ├── Catalog.Application.csproj
│   │   │   ├── Write/                     (COMMANDS - Fluxo de Escrita)
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateBook/
│   │   │   │   │   │   ├── CreateBookCommand.cs
│   │   │   │   │   │   └── CreateBookCommandHandler.cs
│   │   │   │   │   ├── UpdateBook/
│   │   │   │   │   └── DeleteBook/
│   │   │   │   ├── Services/
│   │   │   │   │   └── BookDomainService.cs (orquestração de lógica complexa)
│   │   │   │   └── DTOs/
│   │   │   │       └── CreateBookDto.cs
│   │   │   │
│   │   │   └── Read/                      (QUERIES - Fluxo de Leitura)
│   │   │       ├── Queries/
│   │   │       │   ├── GetBook/
│   │   │       │   │   ├── GetBookQuery.cs
│   │   │       │   │   └── GetBookQueryHandler.cs
│   │   │       │   ├── ListBooks/
│   │   │       │   └── SearchBooks/
│   │   │       └── DTOs/
│   │   │           └── BookReadDto.cs
│   │   │
│   │   └── Catalog.Infrastructure.csproj
│   │       ├── Persistence/
│   │       │   ├── Write/
│   │       │   │   ├── CatalogWriteContext.cs (DbContext para SQL)
│   │       │   │   ├── Configurations/
│   │       │   │   │   └── BookConfiguration.cs
│   │       │   │   └── Repositories/
│   │       │   │       └── BookRepository.cs (IBookRepository)
│   │       │   └── Read/                  (OPCIONAL: para CQRS com read models)
│   │       │       └── ReadModelContext.cs (MongoDB para queries complexas)
│   │       ├── Services/
│   │       └── DependencyInjection.cs (registra serviços do contexto)
│   │
│   ├── Auth/                                (Bounded Context 2)
│   │   ├── Auth.Domain.csproj
│   │   │   ├── Aggregates/
│   │   │   │   ├── User/
│   │   │   │   │   ├── User.cs (AggregateRoot)
│   │   │   │   │   └── UserId.cs (ValueObject)
│   │   │   │   ├── Role/
│   │   │   │   └── Permission/
│   │   │   ├── ValueObjects/
│   │   │   ├── DomainEvents/
│   │   │   └── Repositories/
│   │   │       ├── IUserRepository.cs
│   │   │       ├── IRoleRepository.cs
│   │   │       └── IPermissionRepository.cs
│   │   │
│   │   ├── Auth.Application.csproj
│   │   │   ├── Write/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateUser/
│   │   │   │   │   ├── AssignRole/
│   │   │   │   │   └── ...
│   │   │   │   └── Services/
│   │   │   └── Read/
│   │   │       ├── Queries/
│   │   │       │   ├── GetUser/
│   │   │       │   ├── HasPermission/
│   │   │       │   └── ...
│   │   │       └── DTOs/
│   │   │
│   │   └── Auth.Infrastructure.csproj
│   │       ├── Persistence/
│   │       │   ├── Write/
│   │       │   │   ├── AuthWriteContext.cs
│   │       │   │   ├── Configurations/
│   │       │   │   └── Repositories/
│   │       │   └── Read/
│   │       │       └── AuthReadModelContext.cs (MongoDB)
│   │       ├── Services/
│   │       │   ├── AuthenticationService.cs
│   │       │   └── PermissionService.cs
│   │       └── DependencyInjection.cs
│   │
│   ├── Loan/                               (Bounded Context 3)
│   │   ├── Loan.Domain.csproj
│   │   │   ├── Aggregates/
│   │   │   │   └── BookCheckout/
│   │   │   │       └── BookCheckout.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── UserId.cs    (Referência para outro contexto - apenas ID)
│   │   │   │   └── BookId.cs    (Referência para outro contexto - apenas ID)
│   │   │   ├── DomainEvents/
│   │   │   │   ├── BookCheckedOutEvent.cs
│   │   │   │   └── BookReturnedEvent.cs
│   │   │   └── Repositories/
│   │   │       └── IBookCheckoutRepository.cs
│   │   │
│   │   ├── Loan.Application.csproj
│   │   │   ├── Write/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CheckoutBook/
│   │   │   │   │   └── ReturnBook/
│   │   │   │   └── Services/
│   │   │   └── Read/
│   │   │       ├── Queries/
│   │   │       │   ├── GetActiveCheckouts/
│   │   │       │   └── GetUserCheckoutHistory/
│   │   │       └── DTOs/
│   │   │
│   │   └── Loan.Infrastructure.csproj
│   │       ├── Persistence/
│   │       │   ├── Repositories/
│   │       │   └── Adapters/
│   │       │       ├── ICatalogServiceAdapter.cs (contrato)
│   │       │       ├── IAuthServiceAdapter.cs (contrato)
│   │       │       └── Implementations/
│   │       └── DependencyInjection.cs
│   │
│   └── Report/                             (Bounded Context 4 - Apenas Leitura)
│       ├── Report.Domain.csproj
│       ├── Report.Application.csproj
│       │   └── Read/
│       │       ├── Queries/
│       │       └── DTOs/
│       └── Report.Infrastructure.csproj
│           └── Persistence/
│               └── Read/
│
└── LibraryApp.sln
```

### 2.3 Justificativa da Estrutura Proposta

| Aspecto | Solução | Benefício |
|---------|---------|-----------|
| **Muitos projetos?** | 3 projetos por contexto (não 4): Domain, Application, Infrastructure | Simples e escalável. Cada contexto é isolado. |
| **Quebra Clean Arch?** | Não. Cada contexto mantém camadas limpas: Domain (lógica) → Application (casos de uso) → Infra (persistência) | Decisões de negócio não dependem de frameworks |
| **CQRS primeiro?** | Separar explicitamente `Write/` (Commands) e `Read/` (Queries) na Application | Deixa claro intent: escrita vs leitura. Facilita otimizações |
| **Módulos "inchados"?** | Cada Bounded Context em pasta separada com apenas seus 3 projetos | Cada pasta contém tudo que precisa para seu domínio |
| **Compartilhamento?** | `Core/` com APENAS abstrações base (AggregateRoot, Entity, ValueObject, DomainEvent) | Evita referências cíclicas, mantém contextos independentes |

---

## 2.4 Nomenclatura de Código Compartilhado: "Core" vs "Shared" vs "Common"

| Nome | Usado Para | Quando Usar |
|------|-----------|-----------|
| **Core** | Abstrações fundamentais do domínio | ✅ **Recomendado para DDD**. Contém a "essência" arquitetural |
| **Shared** | Código compartilhado entre contextos | ✅ Bom para DTOs, Behaviors, Mappers genéricos |
| **Common** | Utilitários genéricos (Extensions, Helpers, Utils) | ✅ Bom para converter, validar, etc |

**Proposta**: Use **Core** como padrão (pois é a raiz da arquitetura), e divida conforme necessário:
- `Core/Domain/` - Abstrações de DDD
- `Shared/DTOs/` - DTOs compartilhados
- `Shared/Common/` - Utilitários

---

## 2.5 Exceções Específicas: Necessárias ou Overhead?

### Abordagem Minimalista (Recomendada)
Use **FluentResults** (já no projeto) em vez de exceções específicas:

```csharp
// ❌ Evitar
catch (UserNotFoundException ex) { }
catch (InsufficientPermissionException ex) { }

// ✅ Preferir
var result = await userRepository.GetAsync(userId);
if (result.IsFailed)
    return Result.Fail($"Usuário {userId} não encontrado");
```

**Vantagens**:
- Fluxo controlado (sem exceções)
- Fácil de testar
- Erros esperados como valores, não exceções

### Quando Usar Exceções Específicas
**Apenas** para erros inesperados:

```csharp
// Core/Application/Exceptions/DomainException.cs
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Usar para algo realmente inesperado
if (user == null)
    throw new DomainException("Invariante violada: usuário nulo");
```

**Resumo**: 
- ✅ Resultados esperados → FluentResults
- ⚠️ Invariantes violadas → DomainException
- ❌ Não criar exceção por erro (UserNotFoundException, RoleNotFoundException, etc)

---

## 3. EXEMPLO DETALHADO: CATALOG CONTEXT

---

### 3.1 Padrão Esperado: Book Aggregate (AggregateRoot)

#### Arquitetura do Agregado
```csharp
// Catalog.Domain/Aggregates/Book/Book.cs
public class Book : AggregateRoot<BookId>
{
    // Propriedades do agregado
    public string Title { get; private set; }
    public string Description { get; private set; }
    public IsbnNumber Isbn { get; private set; }        // ValueObject
    public ContributorId MainAuthorId { get; private set; }
    public IReadOnlyCollection<ContributorId> ContributorIds { get; private set; }
    public PublisherId PublisherId { get; private set; }
    public IReadOnlyCollection<CategoryId> CategoryIds { get; private set; }
    public DateTime PublishedDate { get; private set; }
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }
    public BookStatus Status { get; private set; }     // Enum
    
    // Métodos de negócio (Ubiquitous Language)
    public static Result<Book> Create(
        string title,
        string description,
        IsbnNumber isbn,
        ContributorId mainAuthorId,
        PublisherId publisherId,
        DateTime publishedDate,
        int totalCopies)
    {
        // Validações de negócio
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail("Título é obrigatório");
        if (totalCopies <= 0)
            return Result.Fail("Total de cópias deve ser maior que 0");
        
        var book = new Book
        {
            Id = BookId.New(),
            Title = title.Trim(),
            Description = description,
            Isbn = isbn,
            MainAuthorId = mainAuthorId,
            PublisherId = publisherId,
            PublishedDate = publishedDate,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies,
            Status = BookStatus.Active,
            ContributorIds = [],
            CategoryIds = []
        };
        
        // Publicar evento de domínio
        book.RaiseDomainEvent(new BookCreatedEvent(book.Id, book.Title));
        
        return Result.Ok(book);
    }
    
    public Result AssignContributor(ContributorId contributorId)
    {
        if (ContributorIds.Contains(contributorId))
            return Result.Fail("Contribuidor já associado");
        
        (ContributorIds as List<ContributorId>)?.Add(contributorId);
        RaiseDomainEvent(new ContributorAssignedToBookEvent(Id, contributorId));
        return Result.Ok();
    }
    
    public Result AssignCategory(CategoryId categoryId)
    {
        if (CategoryIds.Contains(categoryId))
            return Result.Fail("Categoria já associada");
        
        (CategoryIds as List<CategoryId>)?.Add(categoryId);
        return Result.Ok();
    }
    
    public Result UpdateAvailability(int quantity)
    {
        if (AvailableCopies + quantity > TotalCopies)
            return Result.Fail("Quantidade de cópias disponível não pode exceder o total");
        
        AvailableCopies += quantity;
        return Result.Ok();
    }
    
    public Result Deactivate()
    {
        Status = BookStatus.Inactive;
        RaiseDomainEvent(new BookDeactivatedEvent(Id));
        return Result.Ok();
    }
}

// Catalog.Domain/Aggregates/Book/BookId.cs
public class BookId : ValueObject
{
    public Guid Value { get; }
    
    public BookId(Guid value) => Value = value;
    
    public static BookId New() => new(Guid.NewGuid());
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}

// Catalog.Domain/ValueObjects/IsbnNumber.cs
public class IsbnNumber : ValueObject
{
    public string Value { get; }
    
    public IsbnNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 13)
            throw new ArgumentException("ISBN deve ter 13 caracteres");
        Value = value;
    }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}

// Catalog.Domain/Aggregates/Book/BookStatus.cs
public enum BookStatus
{
    Active = 1,
    Inactive = 2,
    OutOfStock = 3
}
```

### 3.2 Separação Write/Read no Application Layer

#### Write Side (Commands)
```csharp
// Catalog.Application/Write/Commands/CreateBook/CreateBookCommand.cs
public class CreateBookCommand : IRequest<Result<CreateBookResponse>>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Isbn { get; set; }
    public Guid MainAuthorId { get; set; }
    public Guid PublisherId { get; set; }
    public DateTime PublishedDate { get; set; }
    public int TotalCopies { get; set; }
}

// Catalog.Application/Write/Commands/CreateBook/CreateBookCommandHandler.cs
public class CreateBookCommandHandler : 
    IRequestHandler<CreateBookCommand, Result<CreateBookResponse>>
{
    private readonly IBookRepository repository;
    
    public CreateBookCommandHandler(IBookRepository repository)
    {
        this.repository = repository;
    }
    
    public async Task<Result<CreateBookResponse>> Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken)
    {
        // Validar IDs de outros contextos podem ser verificados aqui ou no domínio
        var isbn = new IsbnNumber(request.Isbn);
        
        var bookResult = Book.Create(
            request.Title,
            request.Description,
            isbn,
            new ContributorId(request.MainAuthorId),
            new PublisherId(request.PublisherId),
            request.PublishedDate,
            request.TotalCopies);
        
        if (bookResult.IsFailed)
            return Result.Fail(bookResult.Errors);
        
        var book = bookResult.Value;
        
        // Persistir
        await repository.AddAsync(book, cancellationToken);
        
        // ReturnDto
        return Result.Ok(new CreateBookResponse 
        { 
            Id = book.Id.Value,
            Title = book.Title 
        });
    }
}

// Catalog.Application/Write/DTOs/CreateBookDto.cs
public class CreateBookResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}
```

#### Read Side (Queries)
```csharp
// Catalog.Application/Read/Queries/GetBook/GetBookQuery.cs
public class GetBookQuery : IRequest<Result<GetBookResponse>>
{
    public Guid BookId { get; set; }
}

// Catalog.Application/Read/Queries/GetBook/GetBookQueryHandler.cs
public class GetBookQueryHandler : IRequestHandler<GetBookQuery, Result<GetBookResponse>>
{
    private readonly IBookQueryService queryService;
    
    public GetBookQueryHandler(IBookQueryService queryService)
    {
        this.queryService = queryService;
    }
    
    public async Task<Result<GetBookResponse>> Handle(
        GetBookQuery request,
        CancellationToken cancellationToken)
    {
        var book = await queryService.GetBookAsync(request.BookId, cancellationToken);
        
        if (book == null)
            return Result.Fail($"Livro {request.BookId} não encontrado");
        
        return Result.Ok(book);
    }
}

// Catalog.Application/Read/Queries/ListBooks/ListBooksQuery.cs
public class ListBooksQuery : IRequest<Result<PagedResult<BookReadDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
}

public class ListBooksQueryHandler : 
    IRequestHandler<ListBooksQuery, Result<PagedResult<BookReadDto>>>
{
    private readonly IBookQueryService queryService;
    
    public async Task<Result<PagedResult<BookReadDto>>> Handle(
        ListBooksQuery request,
        CancellationToken cancellationToken)
    {
        var result = await queryService.ListBooksAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.CategoryId,
            cancellationToken);
        
        return Result.Ok(result);
    }
}

// Catalog.Application/Read/DTOs/BookReadDto.cs
public class BookReadDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Isbn { get; set; }
    public string MainAuthor { get; set; }
    public int AvailableCopies { get; set; }
    public string Status { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

### 3.3 Infrastructure: Repository e Query Services

```csharp
// Catalog.Infrastructure/Persistence/Write/Repositories/BookRepository.cs
public class BookRepository : IBookRepository
{
    private readonly CatalogWriteContext dbContext;
    
    public BookRepository(CatalogWriteContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        var entity = BookEntityMapper.ToEntity(book);
        await dbContext.Books.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id.Value, cancellationToken);
        
        return entity == null ? null : BookEntityMapper.ToDomain(entity);
    }
    
    public async Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var entity = BookEntityMapper.ToEntity(book);
        dbContext.Books.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

// Catalog.Application/Read/Services/IBookQueryService.cs
public interface IBookQueryService
{
    Task<BookReadDto?> GetBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<PagedResult<BookReadDto>> ListBooksAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);
}

// Catalog.Infrastructure/Persistence/Read/QueryServices/BookQueryService.cs
public class BookQueryService : IBookQueryService
{
    private readonly CatalogWriteContext readContext; // ou ReadModelContext para MongoDB
    private readonly IMapper mapper;
    
    public BookQueryService(CatalogWriteContext readContext, IMapper mapper)
    {
        this.readContext = readContext;
        this.mapper = mapper;
    }
    
    public async Task<BookReadDto?> GetBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await readContext.Books
            .Include(b => b.Contributors)
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        
        return book == null ? null : mapper.Map<BookReadDto>(book);
    }
    
    public async Task<PagedResult<BookReadDto>> ListBooksAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = readContext.Books.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm));
        
        if (categoryId.HasValue)
            query = query.Where(b => b.Categories.Any(c => c.Id == categoryId.Value));
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => mapper.Map<BookReadDto>(b))
            .ToListAsync(cancellationToken);
        
        return new PagedResult<BookReadDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
```

### 3.4 Dependency Injection do Catalog Context

```csharp
// Catalog.Infrastructure/DependencyInjection.cs
public static class CatalogDependencyInjection
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext para escrita
        services.AddDbContext<CatalogWriteContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5))));
        
        // Repositories (escrita)
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IContributorRepository, ContributorRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        // Query Services (leitura)
        services.AddScoped<IBookQueryService, BookQueryService>();
        services.AddScoped<IContributorQueryService, ContributorQueryService>();
        services.AddScoped<IPublisherQueryService, PublisherQueryService>();
        services.AddScoped<ICategoryQueryService, CategoryQueryService>();
        
        // Mapper (AutoMapper ou similar)
        services.AddScoped(typeof(IMapper), typeof(CatalogMapper));
        
        // Domain Services (se necessário)
        services.AddScoped<BookDomainService>();
        
        return services;
    }
}

// API/Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddCatalogModule(builder.Configuration)      // ← Registra todo o contexto
    .AddAuthModule(builder.Configuration)          // ← Registra Auth
    .AddLoanModule(builder.Configuration)         // ← Registra Loan
    .AddReportModule(builder.Configuration);      // ← Registra Report

// MediatR já registrado
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

---

## 4. DESACOPLAMENTO: ESTRATÉGIAS ENTRE BOUNDED CONTEXTS

### 4.1 **Loan Context → Catalog & Auth (Referências Seguras)**

#### ❌ Problema Atual:
```csharp
public class BookCheckout : RelationalDbModel<BookCheckout>
{
    public required User User { get; set; }          // Acoplamento direto!
    public required Book Book { get; set; }          // Acoplamento direto!
}
```

#### ✅ Solução: Value Objects para IDs de Outros Contextos

```csharp
// Loan.Domain/ValueObjects/UserId.cs
public class UserId : ValueObject
{
    public Guid Value { get; }
    
    public UserId(Guid value) => Value = value;
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}

// Loan.Domain/ValueObjects/BookId.cs  (idêntico a Catalog.Domain/Aggregates/Book/BookId.cs)
public class BookId : ValueObject
{
    public Guid Value { get; }
    
    public BookId(Guid value) => Value = value;
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}

// Loan.Domain/Aggregates/BookCheckout/BookCheckout.cs
public class BookCheckout : AggregateRoot<BookCheckoutId>
{
    public UserId UserId { get; private set; }           // Apenas ID, não o agregado User
    public BookId BookId { get; private set; }           // Apenas ID, não o agregado Book
    public DateTime CheckoutDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public CheckoutStatusEnum Status { get; private set; }
    
    public static Result<BookCheckout> Create(
        UserId userId,
        BookId bookId,
        DateTime checkoutDate,
        DateOnly dueDate)
    {
        if (dueDate <= DateOnly.FromDateTime(checkoutDate))
            return Result.Fail("Data de devolução deve ser após a data de checkout");
        
        var checkout = new BookCheckout
        {
            Id = BookCheckoutId.New(),
            UserId = userId,
            BookId = bookId,
            CheckoutDate = checkoutDate,
            DueDate = dueDate,
            Status = CheckoutStatusEnum.Active
        };
        
        checkout.RaiseDomainEvent(new BookCheckedOutEvent(checkout.Id, userId, bookId));
        return Result.Ok(checkout);
    }
    
    public Result ReturnBook(DateTime returnDate)
    {
        if (Status == CheckoutStatusEnum.Returned)
            return Result.Fail("Livro já foi devolvido");
        
        ReturnDate = returnDate;
        Status = CheckoutStatusEnum.Returned;
        RaiseDomainEvent(new BookReturnedEvent(Id, UserId, BookId, returnDate));
        
        return Result.Ok();
    }
}
```

#### Service Adapter Pattern (Bridge entre contextos)

```csharp
// Loan.Infrastructure/Persistence/Adapters/IAuthServiceAdapter.cs
public interface IAuthServiceAdapter
{
    Task<bool> ValidateUserExistsAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<UserSummary?> GetUserSummaryAsync(UserId userId, CancellationToken cancellationToken = default);
}

// Loan.Infrastructure/Persistence/Adapters/ICatalogServiceAdapter.cs
public interface ICatalogServiceAdapter
{
    Task<bool> ValidateBookExistsAsync(BookId bookId, CancellationToken cancellationToken = default);
    Task<BookSummary?> GetBookSummaryAsync(BookId bookId, CancellationToken cancellationToken = default);
}

// Loan.Infrastructure/Persistence/Adapters/Implementations/AuthServiceAdapter.cs
public class AuthServiceAdapter : IAuthServiceAdapter
{
    private readonly IUserRepository userRepository;    // Injetado de Auth.Infrastructure
    
    public AuthServiceAdapter(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    public async Task<bool> ValidateUserExistsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            new Auth.Domain.Aggregates.User.UserId(userId.Value), 
            cancellationToken);
        return user != null;
    }
    
    public async Task<UserSummary?> GetUserSummaryAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            new Auth.Domain.Aggregates.User.UserId(userId.Value), 
            cancellationToken);
        
        return user == null ? null : new UserSummary 
        { 
            Id = user.Id.Value, 
            Name = $"{user.FirstName} {user.LastName}" 
        };
    }
}

// Loan.Application/Write/Commands/CheckoutBook/CheckoutBookCommand.cs
public class CheckoutBookCommand : IRequest<Result<CheckoutBookResponse>>
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
}

public class CheckoutBookCommandHandler : 
    IRequestHandler<CheckoutBookCommand, Result<CheckoutBookResponse>>
{
    private readonly IBookCheckoutRepository repository;
    private readonly IAuthServiceAdapter AuthAdapter;
    private readonly ICatalogServiceAdapter catalogAdapter;
    
    public CheckoutBookCommandHandler(
        IBookCheckoutRepository repository,
        IAuthServiceAdapter AuthAdapter,
        ICatalogServiceAdapter catalogAdapter)
    {
        this.repository = repository;
        this.AuthAdapter = AuthAdapter;
        this.catalogAdapter = catalogAdapter;
    }
    
    public async Task<Result<CheckoutBookResponse>> Handle(
        CheckoutBookCommand request,
        CancellationToken cancellationToken)
    {
        // Validar existência em outros contextos via adapters
        var userExists = await AuthAdapter.ValidateUserExistsAsync(
            new UserId(request.UserId), 
            cancellationToken);
        if (!userExists)
            return Result.Fail("Usuário não encontrado");
        
        var bookExists = await catalogAdapter.ValidateBookExistsAsync(
            new BookId(request.BookId), 
            cancellationToken);
        if (!bookExists)
            return Result.Fail("Livro não encontrado");
        
        // Criar agregado com IDs desacoplados
        var checkoutResult = BookCheckout.Create(
            new UserId(request.UserId),
            new BookId(request.BookId),
            DateTime.UtcNow,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)));
        
        if (checkoutResult.IsFailed)
            return Result.Fail(checkoutResult.Errors);
        
        await repository.AddAsync(checkoutResult.Value, cancellationToken);
        
        return Result.Ok(new CheckoutBookResponse { Id = checkoutResult.Value.Id.Value });
    }
}
```

**Observações sobre Adapters:**
- **Isolamento**: Loan Context não conhece implementações de Auth ou Catalog
- **Testabilidade**: Mocks de adapters são triviais
- **Escalabilidade**: Se Auth ou Catalog viram serviços, apenas o adapter muda

---

### 4.2 **Auth Context: SQL para Escrita + NoSQL Opcional para Leitura (CQRS)**

#### Problema Atual:
Auth (User, Role, Permission) inteiro em NoSQL é subótimo para escrita (sem ACID transacional).

#### Solução: Dual-Database CQRS

```csharp
// Auth.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddAuthModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ===== ESCRITA: SQL =====
    services.AddDbContext<AuthWriteContext>(options =>
        options.UseSqlServer(
            configuration.GetConnectionString("SqlConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5))));
    
    // Repositories (escrita no SQL)
    services.AddScoped<IUserRepository, SqlUserRepository>();
    services.AddScoped<IRoleRepository, SqlRoleRepository>();
    services.AddScoped<IPermissionRepository, SqlPermissionRepository>();
    
    // ===== LEITURA: SQL ou MongoDB (Read Models) =====
    // Opção A: Usar SQL para read também (simples, sem duplicação)
    services.AddScoped<IUserQueryService, SqlUserQueryService>();
    services.AddScoped<IRoleQueryService, SqlRoleQueryService>();
    
    // Opção B: MongoDB para read models (melhor para queries complexas)
    // services.Configure<MongoDbConfiguration>(configuration.GetSection("MongoDbDatabase:Auth"));
    // services.AddScoped<IUserQueryService, MongoDbUserQueryService>();
    // services.AddScoped<IRoleQueryService, MongoDbRoleQueryService>();
    
    // Domain Services
    services.AddScoped<UserDomainService>();
    
    return services;
}
```

**Fluxo CQRS Simples (SQL → SQL):**
```
Write: CreateUserCommand → Handler → User Aggregate → UserRepository → SQL
                                        ↓
                                  RaiseDomainEvent
                                        ↓
                           UserCreatedEvent → Event Handler → SQL (pode atualizar cache)

Read: GetUserQuery → Handler → UserQueryService → SQL
```

**Fluxo CQRS Avançado (SQL → MongoDB para Reads):**
```
Write: CreateUserCommand → Handler → User Aggregate → UserRepository → SQL
                                        ↓
                                  RaiseDomainEvent
                                        ↓
                           UserCreatedEvent → Event Handler → Sincroniza MongoDB

Read: GetUserQuery → Handler → UserQueryService → MongoDB ReadModel
```

---

## 5. PADRÕES & PRINCÍPIOS A APLICAR

### 5.1 DDD (Domain-Driven Design)

| Elemento | Padrão | Localização |
|----------|--------|-----------|
| **AggregateRoot** | Classe base para agregados | `Core/Domain/AggregateRoot.cs` |
| **Entity** | Classe base para entidades | `Core/Domain/Entity.cs` |
| **ValueObject** | Imutável, sem identidade | `[Context].Domain/ValueObjects/` |
| **Repository** | Interface em Domain, impl. em Infra | `[Context].Domain/Repositories/` + `[Context].Infrastructure/` |
| **Domain Event** | Eventos de negócio | `[Context].Domain/DomainEvents/` |
| **Bounded Context** | Limite bem definido | Cada pasta em `src/` |
| **Ubiquitous Language** | Nomenclatura consistente | Em toda aplicação |

### 5.2 CQRS com DDD

| Responsabilidade | Onde | Padrão |
|------------------|------|--------|
| **Escrita** | `Application/Write/` | Command → CommandHandler → Domain → Repository |
| **Leitura** | `Application/Read/` | Query → QueryHandler → QueryService → DTO |
| **Validação** | Domain (Aggregates) | Métodos factory ou Value Objects validam |
| **Persistência Escrita** | `Infrastructure/Persistence/Write/` | SQL (ACID) |
| **Persistência Leitura** | `Infrastructure/Persistence/Read/` | SQL ou MongoDB (otimizado) |

### 5.3 SOLID & Clean Code

| Princípio | Aplicação |
|-----------|-----------|
| **S** - Single Responsibility | Cada handler faz uma coisa; cada service tem uma responsabilidade |
| **O** - Open/Closed | Novos Commands/Queries sem alterar existentes |
| **L** - Liskov Substitution | IRepository: implementações são intercambiáveis |
| **I** - Interface Segregation | IUserRepository ≠ IRoleRepository (não bloated) |
| **D** - Dependency Inversion | Depend de abstrações, não de implementações |

### 5.4 Clean Architecture

```
        ┌─────────────────────────────────────────┐
        │       Frameworks & Drivers              │
        │  (EF Core, MediatR, FluentValidation)   │
        └────────────────┬────────────────────────┘
                         │
        ┌────────────────▼────────────────────────┐
        │    Interface Adapters (Controllers,     │
        │    Repositories, ExternalServices)      │
        └────────────────┬────────────────────────┘
                         │
        ┌────────────────▼────────────────────────┐
        │    Application (Use Cases)              │
        │  (Commands, Queries, Services)          │
        └────────────────┬────────────────────────┘
                         │
        ┌────────────────▼────────────────────────┐
        │    Domain (Business Rules)              │
        │  (Aggregates, ValueObjects, Events)     │
        └─────────────────────────────────────────┘
```

**Regra**: Camadas internas não devem conhecer camadas externas.

---

## 6. ESTRUTURA DE PASTAS: CATALOG CONTEXT (IMPLEMENTAÇÃO)

```
src/
├── Catalog/
│   ├── Catalog.Domain/
│   │   ├── Catalog.Domain.csproj
│   │   ├── Aggregates/
│   │   │   ├── Book/
│   │   │   │   ├── Book.cs
│   │   │   │   ├── BookId.cs
│   │   │   │   ├── BookStatus.cs
│   │   │   │   └── Events/
│   │   │   │       ├── BookCreatedEvent.cs
│   │   │   │       └── BookDeactivatedEvent.cs
│   │   │   ├── Contributor/
│   │   │   │   ├── Contributor.cs
│   │   │   │   ├── ContributorId.cs
│   │   │   │   └── ContributorRole.cs
│   │   │   ├── Publisher/
│   │   │   │   ├── Publisher.cs
│   │   │   │   └── PublisherId.cs
│   │   │   └── Category/
│   │   │       ├── Category.cs
│   │   │       └── CategoryId.cs
│   │   ├── ValueObjects/
│   │   │   └── IsbnNumber.cs
│   │   ├── Repositories/
│   │   │   ├── IBookRepository.cs
│   │   │   ├── IContributorRepository.cs
│   │   │   ├── IPublisherRepository.cs
│   │   │   └── ICategoryRepository.cs
│   │   └── Exceptions/
│   │       └── CatalogDomainException.cs
│   │
│   ├── Catalog.Application/
│   │   ├── Catalog.Application.csproj
│   │   ├── Write/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateBook/
│   │   │   │   │   ├── CreateBookCommand.cs
│   │   │   │   │   └── CreateBookCommandHandler.cs
│   │   │   │   ├── UpdateBook/
│   │   │   │   ├── DeactivateBook/
│   │   │   │   └── ...
│   │   │   ├── Services/
│   │   │   │   └── BookDomainService.cs
│   │   │   └── DTOs/
│   │   │       ├── CreateBookRequest.cs
│   │   │       └── CreateBookResponse.cs
│   │   ├── Read/
│   │   │   ├── Queries/
│   │   │   │   ├── GetBook/
│   │   │   │   │   ├── GetBookQuery.cs
│   │   │   │   │   └── GetBookQueryHandler.cs
│   │   │   │   ├── ListBooks/
│   │   │   │   ├── SearchBooks/
│   │   │   │   └── ...
│   │   │   └── DTOs/
│   │   │       └── BookReadDto.cs
│   │   ├── Mappers/
│   │   │   └── CatalogMapper.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Catalog.Infrastructure/
│   │   ├── Catalog.Infrastructure.csproj
│   │   ├── Persistence/
│   │   │   ├── Write/
│   │   │   │   ├── CatalogWriteContext.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── BookConfiguration.cs
│   │   │   │   │   ├── ContributorConfiguration.cs
│   │   │   │   │   ├── PublisherConfiguration.cs
│   │   │   │   │   └── CategoryConfiguration.cs
│   │   │   │   └── Repositories/
│   │   │   │       ├── BookRepository.cs
│   │   │   │       ├── ContributorRepository.cs
│   │   │   │       ├── PublisherRepository.cs
│   │   │   │       └── CategoryRepository.cs
│   │   │   └── Read/
│   │   │       ├── CatalogReadContext.cs (opcional)
│   │   │       └── QueryServices/
│   │   │           ├── BookQueryService.cs
│   │   │           ├── ContributorQueryService.cs
│   │   │           └── ...
│   │   └── DependencyInjection.cs
│   │
│   └── Catalog.API/ (opcional, endpoints podem ficar em API/)
│       ├── Catalog.API.csproj
│       └── Endpoints/
│           ├── CreateBookEndpoint.cs
│           ├── GetBooksEndpoint.cs
│           └── ...
```

---

## 7. IMPLEMENTAÇÃO GRADUAL (Roadmap)

### Fase 1: Modelagem Base (Semana 1)
- [ ] Criar `src/Core/Core.csproj` com:
  - [ ] `Domain/AggregateRoot.cs`
  - [ ] `Domain/Entity.cs`
  - [ ] `Domain/ValueObject.cs`
  - [ ] `Domain/IDomainEvent.cs`
- [ ] Criar `src/Catalog/` com estrutura (3 projetos)
- [ ] Refatorar entidades existentes para novos projetos
  - Manter dados iguais, apenas reorganizar

### Fase 2: Domain Layer (Semana 2)
- [ ] Implementar agregados Book, Contributor, Publisher, Category
- [ ] Criar Value Objects (IsbnNumber, etc)
- [ ] Definir Domain Events
- [ ] Criar interfaces de Repository no Domain

### Fase 3: Application Layer (Semana 3)
- [ ] Separar explicitamente `Write/` (Commands) e `Read/` (Queries)
- [ ] Implementar CommandHandlers para Create, Update, Deactivate
- [ ] Implementar QueryHandlers para Get, List
- [ ] Criar DTOs específicos

### Fase 4: Infrastructure (Semana 4)
- [ ] Implementar Repositories (sql)
- [ ] Implementar QueryServices
- [ ] Configurar EF Core Configurations
- [ ] DependencyInjection.cs do contexto

### Fase 5: Integração (Semana 5)
- [ ] Registrar Catalog no Program.cs
- [ ] Migrar Endpoints para usar novos Commands/Queries
- [ ] Testar fluxo completo
- [ ] Remover estrutura antiga

### Fase 6: Outros Contextos (Subsequente)
- [ ] Repetir processo para Auth
- [ ] Repetir para Loan (com ênfase em desacoplamento via Adapters)
- [ ] Repetir para Report

---

## 8. CHECKLIST: CRIAR ESTRUTURA COMPLETA

### Estrutura Completa para Todos os Contextos

Execute os seguintes comandos na pasta `backend/`:

```bash
# ===== CRIAR PASTAS =====

# Core
mkdir src\Core

# Catalog
mkdir src\Catalog\Catalog.Domain
mkdir src\Catalog\Catalog.Application
mkdir src\Catalog\Catalog.Infrastructure

# Auth
mkdir src\Auth\Auth.Domain
mkdir src\Auth\Auth.Application
mkdir src\Auth\Auth.Infrastructure

# Loan
mkdir src\Loan\Loan.Domain
mkdir src\Loan\Loan.Application
mkdir src\Loan\Loan.Infrastructure

# Report
mkdir src\Report\Report.Domain
mkdir src\Report\Report.Application
mkdir src\Report\Report.Infrastructure

# ===== CRIAR PROJETOS CLASSLIB =====

# Core
dotnet new classlib -n Core -o src\Core --force

# Catalog
dotnet new classlib -n Catalog.Domain -o src\Catalog\Catalog.Domain --force
dotnet new classlib -n Catalog.Application -o src\Catalog\Catalog.Application --force
dotnet new classlib -n Catalog.Infrastructure -o src\Catalog\Catalog.Infrastructure --force

# Auth
dotnet new classlib -n Auth.Domain -o src\Auth\Auth.Domain --force
dotnet new classlib -n Auth.Application -o src\Auth\Auth.Application --force
dotnet new classlib -n Auth.Infrastructure -o src\Auth\Auth.Infrastructure --force

# Loan
dotnet new classlib -n Loan.Domain -o src\Loan\Loan.Domain --force
dotnet new classlib -n Loan.Application -o src\Loan\Loan.Application --force
dotnet new classlib -n Loan.Infrastructure -o src\Loan\Loan.Infrastructure --force

# Report
dotnet new classlib -n Report.Domain -o src\Report\Report.Domain --force
dotnet new classlib -n Report.Application -o src\Report\Report.Application --force
dotnet new classlib -n Report.Infrastructure -o src\Report\Report.Infrastructure --force

# ===== ADICIONAR PROJETOS À SOLUÇÃO =====

# Core
dotnet sln add src\Core\Core.csproj

# Catalog
dotnet sln add src\Catalog\Catalog.Domain\Catalog.Domain.csproj
dotnet sln add src\Catalog\Catalog.Application\Catalog.Application.csproj
dotnet sln add src\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj

# Auth
dotnet sln add src\Auth\Auth.Domain\Auth.Domain.csproj
dotnet sln add src\Auth\Auth.Application\Auth.Application.csproj
dotnet sln add src\Auth\Auth.Infrastructure\Auth.Infrastructure.csproj

# Loan
dotnet sln add src\Loan\Loan.Domain\Loan.Domain.csproj
dotnet sln add src\Loan\Loan.Application\Loan.Application.csproj
dotnet sln add src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj

# Report
dotnet sln add src\Report\Report.Domain\Report.Domain.csproj
dotnet sln add src\Report\Report.Application\Report.Application.csproj
dotnet sln add src\Report\Report.Infrastructure\Report.Infrastructure.csproj

# ===== CONFIGURAR REFERÊNCIAS: CATALOG =====

# Catalog.Application → Catalog.Domain + Core
dotnet add src\Catalog\Catalog.Application\Catalog.Application.csproj reference src\Catalog\Catalog.Domain\Catalog.Domain.csproj
dotnet add src\Catalog\Catalog.Application\Catalog.Application.csproj reference src\Core\Core.csproj

# Catalog.Infrastructure → Catalog.Domain + Core
dotnet add src\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj reference src\Catalog\Catalog.Domain\Catalog.Domain.csproj
dotnet add src\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj reference src\Core\Core.csproj

# ===== CONFIGURAR REFERÊNCIAS: Auth =====

# Auth.Application → Auth.Domain + Core
dotnet add src\Auth\Auth.Application\Auth.Application.csproj reference src\Auth\Auth.Domain\Auth.Domain.csproj
dotnet add src\Auth\Auth.Application\Auth.Application.csproj reference src\Core\Core.csproj

# Auth.Infrastructure → Auth.Domain + Core
dotnet add src\Auth\Auth.Infrastructure\Auth.Infrastructure.csproj reference src\Auth\Auth.Domain\Auth.Domain.csproj
dotnet add src\Auth\Auth.Infrastructure\Auth.Infrastructure.csproj reference src\Core\Core.csproj

# ===== CONFIGURAR REFERÊNCIAS: LOAN =====

# Loan.Domain → Core
dotnet add src\Loan\Loan.Domain\Loan.Domain.csproj reference src\Core\Core.csproj

# Loan.Application → Loan.Domain + Core
dotnet add src\Loan\Loan.Application\Loan.Application.csproj reference src\Loan\Loan.Domain\Loan.Domain.csproj
dotnet add src\Loan\Loan.Application\Loan.Application.csproj reference src\Core\Core.csproj

# Loan.Infrastructure → Loan.Domain + Core + Auth.Domain (para adapters) + Catalog.Domain (para adapters)
dotnet add src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj reference src\Loan\Loan.Domain\Loan.Domain.csproj
dotnet add src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj reference src\Core\Core.csproj
dotnet add src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj reference src\Auth\Auth.Domain\Auth.Domain.csproj
dotnet add src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj reference src\Catalog\Catalog.Domain\Catalog.Domain.csproj

# ===== CONFIGURAR REFERÊNCIAS: REPORT =====

# Report.Application → Core
dotnet add src\Report\Report.Application\Report.Application.csproj reference src\Core\Core.csproj

# Report.Infrastructure → Core
dotnet add src\Report\Report.Infrastructure\Report.Infrastructure.csproj reference src\Core\Core.csproj

# ===== ADICIONAR REFERÊNCIAS AO PROJETO API =====

# API → todos os Infrastructure dos contextos
dotnet add API\API.csproj reference src\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj
dotnet add API\API.csproj reference src\Auth\Auth.Infrastructure\Auth.Infrastructure.csproj
dotnet add API\API.csproj reference src\Loan\Loan.Infrastructure\Loan.Infrastructure.csproj
dotnet add API\API.csproj reference src\Report\Report.Infrastructure\Report.Infrastructure.csproj
```

### Resumo da Estrutura Criada

Após executar os comandos, você terá:

```
backend/
├── src/
│   ├── Core/                          (3 arquivos base: AggregateRoot, Entity, ValueObject)
│   │   └── Core.csproj
│   │
│   ├── Catalog/
│   │   ├── Catalog.Domain/
│   │   ├── Catalog.Application/
│   │   └── Catalog.Infrastructure/
│   │
│   ├── Auth/
│   │   ├── Auth.Domain/
│   │   ├── Auth.Application/
│   │   └── Auth.Infrastructure/
│   │
│   ├── Loan/
│   │   ├── Loan.Domain/
│   │   ├── Loan.Application/
│   │   └── Loan.Infrastructure/
│   │
│   └── Report/
│       ├── Report.Domain/
│       ├── Report.Application/
│       └── Report.Infrastructure/
│
├── API/
├── Application/         (será removido gradualmente)
├── Domain/             (será removido gradualmente)
├── Infrastructure/     (será removido gradualmente)
├── LibraryApp.sln
└── ...
```

### ✅ Estrutura Criada com Sucesso!

**Status**: Todos os projetos foram criados e compilados com sucesso (0 erros, apenas warnings do código legado).

**Projetos Criados:**
```
✅ src/Core/Core.csproj
✅ src/Catalog/Catalog.Domain.csproj
✅ src/Catalog/Catalog.Application.csproj
✅ src/Catalog/Catalog.Infrastructure.csproj
✅ src/Auth/Auth.Domain.csproj
✅ src/Auth/Auth.Application.csproj
✅ src/Auth/Auth.Infrastructure.csproj
✅ src/Loan/Loan.Domain.csproj
✅ src/Loan/Loan.Application.csproj
✅ src/Loan/Loan.Infrastructure.csproj
✅ src/Report/Report.Domain.csproj
✅ src/Report/Report.Application.csproj
✅ src/Report/Report.Infrastructure.csproj
```

**Referências Configuradas:**
- ✅ Core + todos os Application/Infrastructure → Core
- ✅ Application/Infrastructure → seus Domain respectivos
- ✅ Loan.Infrastructure → Auth.Domain + Catalog.Domain (para adapters)
- ✅ API → todos os Infrastructure (Catalog, Auth, Loan, Report)
- ✅ Todos os .csproj em .NET 10 (LTS)

---

### Próximos Passos (Implementação)

1. **Implementar Core** (prioridade alta - ~1-2 horas)
   - [ ] `src/Core/Domain/AggregateRoot.cs`
   - [ ] `src/Core/Domain/Entity.cs`
   - [ ] `src/Core/Domain/ValueObject.cs`
   - [ ] `src/Core/Domain/IDomainEvent.cs`
   - [ ] `src/Core/Application/DomainException.cs`

2. **Implementar Catalog** (piloto - ~2-3 dias)
   - [ ] Domain Layer (Aggregates, ValueObjects, Events, Repositories interfaces)
   - [ ] Application Layer (Commands, Queries com separação Write/Read)
   - [ ] Infrastructure Layer (Repositories, QueryServices, DependencyInjection)

3. **Integrar com API** (~4 horas)
   - [ ] Registrar `AddCatalogModule()` no `Program.cs`
   - [ ] Migrar Endpoints para usar novos Commands/Queries

4. **Repetir para Auth, Loan, Report** na mesma sequência (~1-2 semanas)

---

### Como Continuar

**Imediato**: Criar arquivos base no Core (veja Seção 8.1 abaixo)

**Checklist para Começar:**
```bash
# 1. Criar Domain namespace folder em Core
mkdir src\Core\Domain
mkdir src\Core\Application

# 2. Começar com AggregateRoot.cs (copiar exemplo da Seção 8.1)
# 3. Validar compilação após cada classe
# 4. Mover para Catalog.Domain
```

---

## 8.1 PRIMEIRO ARQUIVO A CRIAR: Core/Domain/AggregateRoot.cs

Após a estrutura ser criada, o primeiro passo é implementar as classes base no Core.

### Passo 1: Criar AggregateRoot.cs

```csharp
// src/Core/Domain/AggregateRoot.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Domain;

/// <summary>
/// Classe base para todos os agregados do domínio.
/// Um agregado é uma entidade que atua como raiz de consistência.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

    /// <summary>
    /// Coleção somente leitura dos eventos de domínio não publicados.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    /// <summary>
    /// Levanta um evento de domínio que será armazenado para posterior publicação.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        domainEvents.Add(@event);
    }

    /// <summary>
    /// Limpa todos os eventos de domínio após publicação.
    /// </summary>
    public void ClearDomainEvents()
    {
        domainEvents.Clear();
    }
}
```

### Passo 2: Criar Entity.cs

```csharp
// src/Core/Domain/Entity.cs
using System;

namespace Core.Domain;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Uma entidade é um objeto com identidade única.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public TId Id { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    /// <summary>
    /// Duas entidades são iguais se têm o mesmo Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
```

### Passo 3: Criar ValueObject.cs

```csharp
// src/Core/Domain/ValueObject.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Domain;

/// <summary>
/// Classe base para todos os Value Objects do domínio.
/// Um Value Object é um objeto sem identidade, imutável, definido pelos seus atributos.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna os valores atômicos que definem este Value Object.
    /// Implementar esta propriedade nas classes derivadas.
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Dois Value Objects são iguais se todos os seus valores atômicos são iguais.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is ValueObject valueObject && Equals(valueObject);
    }

    public bool Equals(ValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    private bool ValuesAreEqual(ValueObject other)
    {
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(int), (hashcode, value) =>
            {
                var valueHashCode = value?.GetHashCode() ?? 0;
                return HashCode.Combine(hashcode, valueHashCode);
            });
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
```

### Passo 4: Criar IDomainEvent.cs

```csharp
// src/Core/Domain/IDomainEvent.cs
using System;

namespace Core.Domain;

/// <summary>
/// Marcador para eventos de domínio.
/// Um evento de domínio representa algo importante que aconteceu no domínio.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// ID único do evento.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Data/hora quando o evento ocorreu.
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Classe base para eventos de domínio tipados.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Passo 5: Criar DomainException.cs

```csharp
// src/Core/Domain/DomainException.cs
using System;

namespace Core.Domain;

/// <summary>
/// Exceção levantada quando uma invariante de domínio é violada.
/// Usar APENAS para situações inesperadas que indicam bug no código.
/// Para erros esperados (validação, etc), usar FluentResults.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

---

## 8.2 Validar Compilação

Após criar os arquivos acima, execute:

```bash
dotnet build src\Core\Core.csproj
```

**Resultado esperado**: Build com sucesso (0 erros).

---

## 8.3 Próximo: Começar Catalog.Domain

---

## 9. REFERÊNCIAS & LITERATURA

### Padrões & Best Practices
- **Domain-Driven Design** - Eric Evans (2003)
- **Implementing Domain-Driven Design** - Vaughn Vernon (2013)
- **Clean Architecture** - Robert Martin (2017)
- **Building Microservices** - Sam Newman (2015) - padrões aplicáveis a modular monoliths

### .NET Specific
- **Ardalis Clean Architecture Template** - https://github.com/ardalis/CleanArchitecture
- **NorthwindTraders** - Microsoft DDD Reference
- **eShopOnContainers** - Microsoft CQRS Reference
- **Mediator Pattern** - https://martinfowler.com/articles/patterns-of-distributed-systems/

---

## 10. FAQ & TROUBLESHOOTING

### P: Não será muito lento com tantos projetos?
**R**: Não. Cada projeto é compilado uma vez. A estrutura modular facilita compilação incremental. Com tempo de setup inicial (~30min), economiza horas em manutenção.

### P: Como faço referência a um ValueObject de outro contexto?
**R**: **Não faça**. Cada contexto define seus próprios ValueObjects. Se precisa compartilhar um ID, use apenas o Guid primitivo ou crie um ValueObject específico do contexto que encapsula o Guid.

### P: E se duas queries precisarem dos mesmos dados?
**R**: Crie uma interface `IQueryService` no Application e implemente no Infrastructure. Exemplo:
```csharp
// Catalog.Application/Read/Services/IBookQueryService.cs
public interface IBookQueryService
{
    Task<BookReadDto?> GetByIdAsync(Guid id);
}

// Catalog.Infrastructure/Persistence/Read/QueryServices/BookQueryService.cs
public class BookQueryService : IBookQueryService { ... }
```

### P: Preciso realmente de Write e Read separados?
**R**: Para começar, não. Use SQL tanto para write quanto read. Quando queries ficarem lentas, implemente Read Models no MongoDB. A separação de pastas já deixa essa transição fácil.

### P: Exception Domain específica é mesmo necessária?
**R**: Apenas uma classe base `DomainException` para cenários onde invariantes são violadas. Erros esperados (validação) devem retornar `Result.Fail()`, não exceções.

---



---

## 4. PADRÕES & PRINCÍPIOS A APLICAR

### 4.1 DDD (Domain-Driven Design)

| Elemento | Padrão | Localização |
|----------|--------|-----------|
| **Aggregate** | Raiz com entidades | `[Context].Domain/Aggregates/` |
| **Value Object** | Sem identidade, imutável | `[Context].Domain/ValueObjects/` |
| **Repository** | Interface em Domain, impl. em Infra | `[Context].Domain/Repositories/` + `[Context].Infrastructure/Repositories/` |
| **Domain Event** | Eventos de domínio imutáveis | `[Context].Domain/DomainEvents/` |
| **Bounded Context** | Limite bem definido | Cada pasta em `Modules/` |
| **Ubiquitous Language** | Nomenclatura consistente | Em todo código |

### 4.2 CQRS (Command Query Responsibility Segregation)

**Padrão:**
```
┌─────────────────────────────────────────────────────────┐
│                      API / Presentation                  │
└────────────────┬────────────────────────────┬───────────┘
                 │                            │
          [Command]                      [Query]
                 │                            │
    ┌────────────▼─────────────┐   ┌────────▼──────────┐
    │ Application Layer         │   │ Application Layer  │
    │ Commands & Handlers       │   │ Queries & Handlers │
    └────────────┬─────────────┘   └────────┬──────────┘
                 │                           │
    ┌────────────▼─────────────┐   ┌────────▼──────────┐
    │ Domain Layer              │   │ Read Model        │
    │ Business Logic            │   │ Projections       │
    └────────────┬─────────────┘   └────────┬──────────┘
                 │                           │
    ┌────────────▼─────────────┐   ┌────────▼──────────┐
    │ Write DB (SQL/NoSQL)      │   │ Read DB (MongoDB) │
    │ Normalized & Transactional│   │ Denormalized      │
    └───────────────────────────┘   └───────────────────┘
```

### 4.3 SOLID & Clean Code

```
S - Single Responsibility
    ✓ Cada Handler tem uma responsabilidade
    ✓ Services específicos por função (auth, validation, etc)

O - Open/Closed
    ✓ Aberto para extensão: novos Commands/Queries sem alterar existentes
    ✓ Fechado para modificação: interfaces estáveis

L - Liskov Substitution
    ✓ Implementações de IRepository são intercambiáveis
    ✓ Mock para testes

I - Interface Segregation
    ✓ Interfaces pequenas e focadas
    ✓ IUserRepository ≠ IRoleRepository

D - Dependency Inversion
    ✓ Dependências injetadas via DI
    ✓ Abstração via interfaces
```

---

## 5. IMPACTO MÍNIMO: MIGRAÇÃO GRADUAL

### 5.1 **Fase 1: Modelagem (o que você quer agora)**
- [ ] Criar estrutura de pastas para módulos
- [ ] Criar novos .csproj por bounded context
- [ ] Refatorar Domain Layer mantendo dados iguais
- [ ] Implementar Value Objects para desacoplamento

### 5.2 **Fase 2: Aplicação (próximos passos)**
- [ ] Separar Commands e Queries explicitamente
- [ ] Criar handlers com novo padrão
- [ ] Manter handlers antigos por compatibilidade

### 5.3 **Fase 3: Infraestrutura**
- [ ] Migrar Repositories para novos projetos
- [ ] Separar Sql DbContexts por contexto (opcional)
- [ ] Implementar Read Models se necessário

### 5.4 **Fase 4: API**
- [ ] Criar Endpoints por módulo
- [ ] Migrar gradualmente da estrutura antiga

---

## 6. CHECKLIST: PRÓXIMAS AÇÕES

### Imediato (Modelagem)
- [ ] Criar pasta `Modules/`
- [ ] Criar estrutura padrão para um contexto (ex: Catalog)
- [ ] Criar novos .csproj:
  - [ ] Catalog.Domain.csproj
  - [ ] Catalog.Application.csproj
  - [ ] Catalog.Infrastructure.csproj
  - [ ] Catalog.API.csproj
- [ ] Implementar Value Objects para desacoplamento
- [ ] Criar interfaces de adapters (IAuthService, ICatalogService)

### Curto Prazo
- [ ] Implementar padrão Repository completo
- [ ] Separar MediatR Commands e Queries
- [ ] Criar Domain Events

### Médio Prazo
- [ ] Migrar Context Auth para SQL
- [ ] Implementar CQRS Queries (opcional: com MongoDB Read Models)
- [ ] Refatorar Endpoints por módulo

---

## 7. REFERÊNCIAS

### Patterns & Best Practices
- **Domain-Driven Design** - Eric Evans
- **Implementing Domain-Driven Design** - Vaughn Vernon
- **Building Microservices** - Sam Newman (para padrões aplicáveis a monolito modular)
- **Clean Architecture** - Robert Martin

### .NET Specific
- **Ardalis Clean Architecture** - https://github.com/ardalis/CleanArchitecture
- **NorthwindTraders** - Microsoft Reference Architecture
- **eShopOnContainers** - Referência MS para DDD + CQRS

---

## 8. ESTRUTURA DE EXEMPLO DETALHADA: CATALOG CONTEXT

Vejo no próximo documento específico detalhes da implementação.

