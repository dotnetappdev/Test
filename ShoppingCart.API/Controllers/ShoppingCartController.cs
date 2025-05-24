using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Data.Entity;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShoppingCart.API.Controllers
{
    /// <summary>
    /// API Controller for ShoppingCart API
    /// </summary>
    /// <param name="context"></param>
    [ApiController]
    [Route("[controller]")]
    public class ShoppingCartController(ShoppingCartDbContext context) : ControllerBase
    {

        private readonly ShoppingCartDbContext _context = context;


        /// <summary>
        /// Adds a product to the cart and returns the updated cart with the total price calculated.
        /// </summary>
        /// <remarks>
        /// <b>Input:</b><br/>
        /// - All parameters are mandatory.<br/>
        /// - productId must be of an existing product (Refer: seededData.json dataset).<br/>
        /// - quantity must be more than 0.<br/>
        /// <br/>
        /// <b>Requirements:</b><br/>
        /// - Only one cart per customer (Customer name is assumed as unique).<br/>
        /// - A new cart is created if one doesn't exist for the customer.<br/>
        /// - If the product already exists in the cart, the quantity is updated.<br/>
        /// - A new cart item is added if the product is added for the first time.<br/>
        /// <br/>
        /// <b>Output:</b><br/>
        /// - Success: HttpStatusCode.OK with the updated cart object including all cart items and calculated amount.<br/>
        /// - Failure:<br/>
        ///   1) HttpStatusCode.BadRequest - if quantity is less than 1 or product doesn't exist for the given productId.<br/>
        ///   2) HttpStatusCode.InternalServerError - Any other exception.<br/>
        /// </remarks>
        /// <param name="customerName">Customer for whom items are added in the cart</param>
        /// <param name="productId">ID of the product being added to the cart</param>
        /// <param name="quantity">Quantity (in numbers)</param>
        /// <returns>Cart object including all cart items and calculated amount</returns>
        [HttpPost("AddProductToCart")]
        [ProducesResponseType(typeof(Cart), ((int)HttpStatusCode.OK))]
        [ProducesResponseType(((int)HttpStatusCode.BadRequest))]
        [ProducesResponseType(((int)HttpStatusCode.InternalServerError))]
        public async Task<IActionResult> AddProductToCart(string customerName, int productId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(customerName) || productId < 1 || quantity < 1)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid parameters.");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ID == productId);
            if (product == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Product does not exist.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerName = customerName,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductID == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.Amount = cartItem.Quantity * product.PricePerQuantity;
                cartItem.Product = product;
                _context.CartItems.Update(cartItem);
            }
            else
            {
                cartItem = new CartItem
                {
                    CartID = cart.ID,
                    ProductID = productId,
                    Quantity = quantity,
                    Amount = quantity * product.PricePerQuantity,
                    Product = product
                };
                _context.CartItems.Add(cartItem);
            }

            foreach (var item in cart.CartItems)
            {
                if (item.Product == null)
                {
                    item.Product = await _context.Products.FirstOrDefaultAsync(p => p.ID == item.ProductID);
                }
                item.Amount = item.Quantity * (item.Product?.PricePerQuantity ?? 0);
            }

            // Recalculate total amount using the latest CartItems from the database (to ensure all are included)
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Amount);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }

            // Reload cart with items and products for return
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ID == cart.ID);

            return Ok(updatedCart);
        }
    }
}
