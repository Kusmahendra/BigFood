using BigFood.Models;
using BigFood.GraphQL;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using HotChocolate.AspNetCore.Authorization;

namespace BigFood.GraphQL
{


    public class Mutation
    {
//-------------------------------Courier Action ----------------------------------//

        [Authorize(Roles = new[] {"COURIER"})]
        public async Task<Order> UpdateOrderByCourierAsync(
            UpdateOrderInput input,
            [Service] BigFoodContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var courier = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var order = context.Orders.Where(o=>o.Complete == false && o.CourierId == courier.Id).FirstOrDefault();
            var locCourier = context.Statuses.Where(s=>s.UserId == courier.Id).FirstOrDefault();
            var locUser = context.Statuses.Where(s=>s.UserId == order.UserId).FirstOrDefault();
            if(locCourier!=null)
            {
                locCourier.LocationLat = Convert.ToString(input.LocationLat);
                locCourier.LocationLong = Convert.ToString(input.LocationLong);
                context.Statuses.Update(locCourier);
                await context.SaveChangesAsync();
            }

            var calDis = new CalculateDistance();
            var newDistance = "";
            if(locUser.LocationLat == locCourier.LocationLat && locUser.LocationLong == locCourier.LocationLong)
            {
                newDistance = "0";
            }
            else
            {
                newDistance = Convert.ToString(calDis.distance(Convert.ToDouble(locUser.LocationLat), 
                Convert.ToDouble(locUser.LocationLong), 
                Convert.ToDouble(locCourier.LocationLat), 
                Convert.ToDouble(locCourier.LocationLong)));
            }
            
            switch(input.StepCount)
            {
                case 1:
                order.OrderStatus = $"Pesanan sedang dipesan oleh {courier.Username}";
                order.Distance = newDistance;
                break;

                case 2:
                order.OrderStatus = $"Pesanan sedang diantar oleh {courier.Username}";
                order.Distance = newDistance;
                break;
                
                case 3:
                order.OrderStatus = $"Pesanan sudah sampai";
                order.Distance = newDistance;
                break;
            }
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return await Task.FromResult(order);
        }
        
        [Authorize(Roles = new[] {"COURIER"})]
        public async Task<Order> CompleteOrderByCourierAsync(
            [Service] BigFoodContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var courier = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var order = context.Orders.Where(o=>o.Complete == false && o.CourierId == courier.Id).FirstOrDefault();
            var orderDetail = context.OrderDetails.Where(d=>d.OrderId == order.Id).FirstOrDefault();
            var status = context.Statuses.Where(s=>s.UserId == courier.Id).FirstOrDefault();

            using var transaction = context.Database.BeginTransaction();
            if(order.Distance == "0")
            {
                try
                {
                if(order!=null)
                {
                    order.Complete = true;
                    context.Orders.Update(order);
                    //await context.SaveChangesAsync();

                    status.Status1 = "AVAILABLE";
                    context.Statuses.Update(status);
                    //await context.SaveChangesAsync();

                    orderDetail.EndDate = DateTime.Now;
                    context.OrderDetails.Update(orderDetail);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return await Task.FromResult(order);
                }
                }
                catch
                {
                transaction.Rollback();
                }
            }

            return await Task.FromResult(order);
        }
//--------------------------------------------------------------------------------//        

//-------------------------------Buyer Action ----------------------------------//        
        
        //Buy Foods
        [Authorize(Roles = new[] {"BUYER"})]
        public async Task<OrderMessage> AddOrderByBuyerAsync(
            OrderInput input,
            [Service] BigFoodContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            //instantiate calculate distance method
            var calDis = new CalculateDistance();
        
            var userName = claimsPrincipal.Identity.Name;
            //User(Buyer)
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            //User(Courier)
            var courier = context.Users.Where(u=>u.Id == input.CourierId).FirstOrDefault();
            //BuyerStatus(location)
            var locUser = context.Statuses.Where(s=>s.UserId == user.Id).FirstOrDefault();
            //CourierStatus(location)
            var locCourier = context.Statuses.Where(s=>s.UserId == input.CourierId).FirstOrDefault();

            //call calculate distance method
            var jarak = calDis.distance(Convert.ToDouble(locUser.LocationLat), 
                Convert.ToDouble(locUser.LocationLong), 
                Convert.ToDouble(locCourier.LocationLat), 
                Convert.ToDouble(locCourier.LocationLong));
            var jarak2 = Convert.ToString(jarak);

            using var transaction = context.Database.BeginTransaction();
            try
            {
                if(locCourier.Status1 == "AVAILABLE")
                {
                    var order = new Order
                    {
                        UserId = user.Id,
                        CourierId = input.CourierId,
                        Complete = false,
                        Distance =  jarak2,
                        OrderStatus = $"Pesanan akan segera diproses oleh Kurir {courier.Username}"
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    var orderDetail = new OrderDetail
                    {
                        Quantity = input.Quantity,
                        StartDate = DateTime.Now,
                        OrderId = order.Id,
                        FoodId = input.FoodId
                    };
                    context.OrderDetails.Add(orderDetail);
                    await context.SaveChangesAsync();

                    locCourier.Status1 = "UNAVAILABLE";
                    context.Statuses.Update(locCourier);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return await Task.FromResult(new OrderMessage(input.CourierId, order.Distance, 
                        order.OrderStatus));
                }
                else
                {
                    throw new Exception("Courier Not Available");
                }
            }
            catch
            {
                transaction.Rollback();
            }
            return await Task.FromResult(new OrderMessage(input.CourierId, "0", "Gagal Memesan"));
        }


//------------------------------------------------------------------------------//        

//------------------------------- Manage Food By Manager ----------------------------------//
        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<Food> AddFoodByManagerAsync(
            FoodInput input,
            [Service] BigFoodContext context)
        {
            var food = new Food
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                Created = DateTime.Now
            };

            var ret = context.Foods.Add(food);
            await context.SaveChangesAsync();

            return ret.Entity;
        }
        
        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<Food> UpdateFoodByManagerAsync(
            FoodInput input,
            [Service] BigFoodContext context)
        {
            var food = context.Foods.Where(o => o.Id == input.Id).FirstOrDefault();
            if (food != null)
            {
                food.Name = input.Name;
                food.Stock = input.Stock;
                food.Price = input.Price;

                context.Foods.Update(food);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(food);
        }

        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<Food> DeleteFoodByManagerAsync(
            int input,
            [Service] BigFoodContext context)
        {
            var food = context.Foods.Where(o=>o.Id == input).FirstOrDefault();
            if(food != null)
            {
                context.Foods.Remove(food);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(food);
        }
//-----------------------------------------------------------------------------------------//
        //Change Password
        [Authorize]
        public async Task<UserData> ChangePasswordAsync(
            ChangePassword input,
            [Service] BigFoodContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u=>u.Username == userToken.Name).FirstOrDefault();
            if(user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password); //encrypt password
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new UserData { 
                Id=user.Id,
                Username=user.Username,
                Email =user.Email,
            });
        }
//------------------------------- Manage User By Admin ----------------------------------//
        [Authorize(Roles = new[] { "ADMIN"})]
        public async Task<User> DeleteUserByAdminAsync(
            int input,
            [Service] BigFoodContext context)
        {
            var user = context.Users.Where(o=>o.Id == input).FirstOrDefault();
            if(user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(user);
        }

        [Authorize(Roles = new[] { "ADMIN"})]
        public async Task<UserData> UpdateUserByAdminAsync(
            RegisterUser input,
            [Service] BigFoodContext context)
        {
            var user = context.Users.Where(o=>o.Id == input.Id).FirstOrDefault();
            if(user != null)
            {
                user.Username = input.UserName;
                user.Email = input.Email;
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password); 
                var ret = context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new UserData { 
                Id=user.Id,
                Username=user.Username,
                Email =user.Email,
            });
        }

        [Authorize(Roles = new[] { "ADMIN"})]
        public async Task<UserData> CreateUserByAdminAsync(
            RegisterUser input,
            [Service] BigFoodContext context)
        {
            var user = context.Users.Where(o=>o.Username == input.UserName).FirstOrDefault();
            if(user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Email = input.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData { 
                Id=newUser.Id,
                Username=newUser.Username,
                Email =newUser.Email,
            });
        }
//------------------------------------------------------------------------------//        

        //RegisterNew User
        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] BigFoodContext context)
        {
            var user = context.Users.Where(o=>o.Username == input.UserName).FirstOrDefault();
            if(user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.UserName,
                Email = input.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) //encrypt password
            };

            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData { 
                Id=newUser.Id,
                Username=newUser.Username,
                Email =newUser.Email,
            });
        }

        //Login
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, //setting token
            [Service] BigFoodContext context) //ef
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null,null,"Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password,user.Password);
            if (valid)
            {
                //generate JWT Token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                //JWT gayload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.Id == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o=>o.Id == userRole.RoleId).FirstOrDefault();
                    if(role!=null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,   
                    claims: claims, //jwt gayload
                    signingCredentials: credentials //signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }



        
    }
}