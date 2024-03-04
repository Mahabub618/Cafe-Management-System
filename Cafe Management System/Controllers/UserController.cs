using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("signup")]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users
                .Where(u => u.email == user.email).FirstOrDefault();

                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Successfully Registered" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Email Already Exist" });
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }


        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = db.Users
                .Where(u => (u.email == user.email && u.password == user.password)).FirstOrDefault();

                if (userObj != null)
                {
                    if (userObj.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { token = TokenManager.GenerateToken(userObj.email, userObj.role) });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Wait for Admin Approval" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Incorrect Username or Password" });
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage checkToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        [HttpGet, Route("getAllUser")]
        [CustomAuthenticationFilter]
       
        public HttpResponseMessage GetAllUser()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                var result = db.Users
                    .Select(u=> new {u.id, u.name, u.contactNumber, u.email, u.status, u.role})
                    .Where(x=> x.role == "user")
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("updateUserStatus")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateUserStatus(User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User userObj = db.Users.Find(user.id);
                if(userObj == null)
                {
                    response.message = "User id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "User Status Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("changePassword")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                User userObj = db.Users
                    .Where(user => (user.email == tokenClaim.Email && user.password == changePassword.OldPassword)).FirstOrDefault();

                if(userObj != null)
                {
                    userObj.password = changePassword.NewPassword;
                    db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    response.message = "Password Updated Successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    response.message = "Incorrect Old Password";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

    }
}
