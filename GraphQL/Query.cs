using System.Security.Claims;
using BigFood.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BigFood.GraphQL
{
    public class Query
    {
        //View Foods
        [Authorize(Roles = new[] {"BUYER"})]
        public IQueryable<Food> GetFoodsByBuyer([Service] BigFoodContext context) =>
            context.Foods;

//------------------------------- Manage Food By Manager ----------------------------------//
        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<Food> GetFoods([Service] BigFoodContext context) =>
            context.Foods;
//-----------------------------------------------------------------------------------------//

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
//------------------------------- Manage User By Admin ----------------------------------//
        //Manage User(Read)
        [Authorize(Roles = new[] {"ADMIN"})]
        public IQueryable<UserData> GetUsers([Service] BigFoodContext context) =>
        context.Users.Select(p => new UserData()
        {
            Id = p.Id,
            Email = p.Email,
            Username = p.Username
        });
//------------------------------------------------------------------------------//       
        
    }
}