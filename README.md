# Shopping Cart

Shopping Cart is the core module of our eCommerce store. This enables customers to add items, adjust quantities, and
view final cost before they can make a purchase.

In this exercise, we are going to pair on implementing a Shopping Cart in ASP.NET Core Web API using Sqlite database.
We've provided an **interface** and an **API spec** that we'd like you to satisfy. Your goal is to fill in the implementation,
while also making sure to remain align to the [API Spec provided here](./ShoppingCart.API/docs/api-specs/shoppingcart-api-v1.json).

## Guidelines

- **Database:**
  For the purpose of this exercise, we have produced a JSON file containing sample dataset. This dataset is made
  available in Sqlite database when the application runs for first time. All database operations will be performed in
  this database while you implement your changes.

- **ORM:**
  Provided code is using EF Core as an ORM and all set up is already done including setting up DbContext. However, you
  can use a different ORM if you prefer to, but that may require doing all set up on your own!

- **Swagger UI:**
  Swagger is configured to document and test APIs. ShoppingCart.API project will load at following URL
  by default while running the application:
  > <ins>https://localhost:[port]/swagger/index.html</ins> *([port] is a placeholder for port number on which the app
  is running).*

- **Tests:**
  ShoppingCartControllerTests contains set of xUnit tests to confirm your
  implementation is satisfying the requirements mentioned for API.

## Functional Expectations

**Background:**

There exist 3 entities:

- <ins>Product</ins> - Products master.
- <ins>Cart</ins> - A customer can manage and track their shopping items. *Only
  one cart per customer to keep this exercise simple.*
- <ins>CartItem</ins> - A cart can have multiple items (i.e., Products) withdifferent quantities.
  > [!NOTE]
  > Just to avoid complexity, there is no Customer master table. Instead, Customer
  > name is being stored in Cart table itself. However, in an ideal world, we
  > should adhere to database design principles.

**Requirements:**

You will need to implement **AddProductToCart** API in
**ShoppingCartController** to meet the following requirements:

- Create or Update a customer's Cart for given Product and Quantity:
  - If Cart doesn't exist for a customer then create a new Cart.
  - If Product doesn't exist then throw an error.
  - If Cart exists but Product doesn't exist in the Cart (i.e., CartItem object) then create a new item in the Cart.
  - If Product exists in the Cart then update Quantity.
- API should return Cart object with calculated amount for each line and overall Cart.
- Determine what will be your best approach for calculating Amount and TotalAmount. Feel free to do changes in existing
  code such as DTOs, etc if you need to.
- Make this transaction atomic.
- Perform all required validations.
- Make sure all tests are passing; also add any new tests you can think of.
- Try to follow programming best principles and achieve as much as maintainability, re-usability,  readability, and
  scalability possible.

## Getting Started

**Pre-requisites:**

- .NET 8.
- Visual Studio 2022 or VS Code (with all extensions installed for .NET development).
- Live Share feature in Visual Studio or VS Code for pair programming.

**Steps:**

- Extract the supplied ZIP file in your local system.
- Launch IDE (example, Visual Studio 2022).
- Open solution (ShoppingCart.sln).
- Build/ Rebuild the solution (also ensure all Nuget packages are restored in build operation).
- Run "ShoppingCart.API" project which should launch the application in Swagger.
- Also check ShoppingCart.API folder to ensure `shoppingcart.db` file is created.
- Open "ShoppingCartController.cs" and find non-implemented API "AddProductToCart". You will need to make this API
  working and satisfying the requirements.
  > [!NOTE]
  > ShoppingCartDbContext is being set through Program.cs and is injected in ShoppingCartController, so you don't need
  > to worry about generating DbContext. All you need to do is use it for CRUD operations to meet the requirements.

You are all set now! Happy coding!
