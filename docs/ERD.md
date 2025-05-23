# Entity Relationship Diagram

```mermaid
erDiagram

  CASHIER ||--o{ ORDER : handles
  CATEGORY ||--o{ PRODUCT : contains
  PRODUCT ||--o{ ORDER_PRODUCT : is_part_of
  ORDER ||--o{ ORDER_PRODUCT : contains

  CASHIER {
    int Id PK
    string FirstName
    string LastName
  }

  CATEGORY {
    int Id PK
    string CategoryName
  }

  PRODUCT {
    int Id PK
    string ProductName
    decimal Price
    string Brand
    int CategoryId FK
  }

  ORDER {
    int Id PK
    int CashierId FK
    DateTime PaidOnDate
  }

  ORDER_PRODUCT {
    int OrderId PK, FK
    int ProductId PK, FK
    int Quantity
  }
```

## Tables and Keys

| Table         | Primary Key(s)         | Foreign Key(s)                                     |
| ------------- | ---------------------- | -------------------------------------------------- |
| Cashiers      | `Id`                   | —                                                  |
| Categories    | `Id`                   | —                                                  |
| Products      | `Id`                   | `CategoryId → Categories(Id)`                      |
| Orders        | `Id`                   | `CashierId → Cashiers(Id)`                         |
| OrderProducts | `OrderId`, `ProductId` | `OrderId → Orders(Id)`, `ProductId → Products(Id)` |





