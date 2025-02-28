using System.Security.Claims;
using BookStoreApi.DTO.OrderDTO;
using BookStoreApi.Models;
using BookStoreApi.Sevices;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : Controller
{
    private readonly DataContext _db;
    private readonly PaypalService _paypalService;

    public OrderController(DataContext context, PaypalService paypalService)
    {
        _db = context;
        _paypalService = paypalService;
    }

    [HttpPost]
    [Route("payOrder")]
    public async Task<IActionResult> AddOrder([FromBody] AddOrderDTO addOrderDto)
    {
        var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);

        if (!isUserLoggedIn)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Tienes que logearte primero para poder pagar"
            });
        }

        try
        {
            int idUser = int.Parse(currentUserName);
            var price = addOrderDto.Total;
            var currency = "PEN";

            var reference = "INV001";


            var Order = new Order()
            {
                UserId = idUser,
                OrderDate = DateTime.Now,
                Total = addOrderDto.Total,
                OrderDetails = new List<OrderDetail>()
            };

            await _db.Orders.AddAsync(Order);

            foreach (var orderDetail in addOrderDto.OrderDetails)
            {
                var newOrderDetail = new OrderDetail()
                {
                    Id = Order.Id,
                    BookId = orderDetail.BookId,
                    Quantity = orderDetail.Quantity,
                    Price = orderDetail.Price,
                };

                Order.OrderDetails.Add(newOrderDetail);
            }

            await _db.SaveChangesAsync();

            var response = await _paypalService.CreateOrder(price, currency);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
    
}