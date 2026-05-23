CREATE TABLE Products (
    Id SERIAL PRIMARY KEY,
    Name TEXT NOT NULL,
    Cost NUMERIC NOT NULL,
    UsefulLife INTEGER NOT NULL
);

CREATE TABLE Orders (
    Id SERIAL PRIMARY KEY,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,

    CONSTRAINT FK_Orders_Products
    FOREIGN KEY (ProductId)
    REFERENCES Products(Id)
);