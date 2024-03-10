using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("addNewProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage AddNewProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                response.message = "Product Added Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("getAllProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var result = from Products in db.Products
                             join Category in db.Categories
                             on Products.categoryId equals Category.id
                             select new
                             {
                                 Products.id,
                                 Products.name,
                                 Products.description,
                                 Products.price,
                                 Products.status,
                                 categoryId = Category.id,
                                 categoryName = Category.name
                             };
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet, Route("getProductByCategory/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetProductByCategory(int id)
        {
            try
            {
                var result = db.Products
                    .Where(x => x.categoryId == id && x.status == "true")
                    .Select(x => new { x.id, x.name }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet, Route("getProductById/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetProductById(int id)
        {
            try
            {
                Product productObj = db.Products.Find(id);
                if(productObj == null)
                {
                    response.message = "Product id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                return Request.CreateResponse(HttpStatusCode.OK, productObj);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("updateProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(product.id);
                if(productObj == null)
                {
                    response.message = "Product id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                productObj.name = product.name;
                productObj.description = product.description;
                productObj.price = product.price;
                productObj.categoryId = product.categoryId;

                db.Entry(productObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "Product Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("deleteProduct/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage DeleteProduct(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(id);

                if(productObj == null)
                {
                    response.message = "Product id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                db.Products.Remove(productObj);
                db.SaveChanges();
                response.message = "Product Deleted Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("updateProductStatus")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateProductStatus([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(product.id);
                if(productObj == null)
                {
                    response.message = "Product id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                productObj.status = product.status;
                db.Entry(productObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "Product Status Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
