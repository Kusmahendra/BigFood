using System.Security.Claims;
using BigFood.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BigFood.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] {"ADMIN"})]
        public IQueryable<UserRole> SeeRolesAsAdmin([Service] BigFoodContext context) =>
            context.UserRoles;
//------------------------------- Manage Order ----------------------------------//
        //Read all Order
        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<Order> GetOrderByManager([Service] BigFoodContext context) =>
            context.Orders.Include(o=>o.OrderDetails);

//-------------------------------------------------------------------------------//

//------------------------------- Buyer Action ----------------------------------//
        //Track Order
        [Authorize(Roles = new[] {"BUYER"})]
        public IQueryable<Order?> TrackOrderByBuyer([Service] BigFoodContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u=>u.Username == userToken.Name).FirstOrDefault();
            if (user !=null)
            {
                var order = context.Orders.Where(p=>p.UserId == user.Id && p.Complete == false).Include(o=>o.OrderDetails);
                return order.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
        //view orders(buyer)
        [Authorize(Roles = new[] {"BUYER"})]
        public IQueryable<Order?> GetOrderByBuyer([Service] BigFoodContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u=>u.Username == userToken.Name).FirstOrDefault();
            if (user !=null)
            {
                var order = context.Orders.Where(p=>p.UserId == user.Id).Include(o=>o.OrderDetails);
                return order.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
        
        //View Foods
        [Authorize(Roles = new[] {"BUYER"})]
        public IQueryable<Food> GetFoodsByBuyer([Service] BigFoodContext context) =>
            context.Foods;
//-------------------------------------------------------------------------------//

//------------------------------- Manage Food By Manager ----------------------------------//
        //Get food by manager
        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<Food> GetFoods([Service] BigFoodContext context) =>
            context.Foods;
//-----------------------------------------------------------------------------------------//

//---------------------------------- User Managemenet -------------------------------------//

        //View Profile
        [Authorize]
        public IQueryable<Profile?> GetProfileByUser([Service] BigFoodContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u=>u.Username == userToken.Name).FirstOrDefault();
            if (user !=null)
            {
                var profiles = context.Profiles.Where(p=>p.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }
//-----------------------------------------------------------------------------------------//

//------------------------------- Manage User By Admin -----------------------------------//
        //Manage User(Read)
        [Authorize(Roles = new[] {"ADMIN"})]
        public IQueryable<UserData> GetUsers([Service] BigFoodContext context) =>
        context.Users.Select(p => new UserData()
        {
            Id = p.Id,
            Email = p.Email,
            Username = p.Username
        });
//----------------------------------------------------------------------------------------//       
        
    }
}