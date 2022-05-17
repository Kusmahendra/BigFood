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