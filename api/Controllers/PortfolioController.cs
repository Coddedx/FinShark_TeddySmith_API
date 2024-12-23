using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{
    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPortfolioRepository _portfolioRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IFMPService _fmpService;
        
        public PortfolioController(UserManager<AppUser> userManager, IPortfolioRepository portfolioRepo, IStockRepository stockRepo, IFMPService fmpService ) 
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portfolioRepo = portfolioRepo;
            _fmpService = fmpService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio()
        {
            if (User == null || !User.Identity.IsAuthenticated)return Unauthorized("User is not authenticated.");

            var username = User.GetUsername();  //we dont have GetUsername so we need to make it (Extensions -> ClaimsExtensions)
            //(user is inherited from the controller base whenever you utilize an endpoint (mesela https://api.example.com/products) it http context ) an Http context is created
            //and this user object will allow you reach in and grab it (also u can reach claim (we get our claims from tokens !! (when we creating token we add email and username in it and thats what we are reaching in) ))
            //  SO WE ARE GONNA REACH İNTO THAT TOKEN NOT THE ACTUAL TOKEN İTSELF BUT THE HTTP CONTEXT AND THE CLAİMS THAT WERE GİVEN TO US THROUGH THE TOKENS
            if (string.IsNullOrEmpty(username)) return BadRequest("Username claim is missing.");

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return NotFound("User not found.");

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            return Ok(userPortfolio);
        }
        
        [HttpPost]
        [Authorize] //we need the claims from the token so wee need the authorize
        public async Task<IActionResult> AddPortfolio(string symbol) //we are gonna get stock by symbol
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username); // authorize is gonna check for user is null for us 
            var stock = await _stockRepo.GetBySymbolAsync(symbol);

            if(stock == null) 
            { 
                stock = await _fmpService.FindStockBySymbolAsync(symbol);//this where we are gonna try to populate from the FMPService 
                
                if(stock == null ) return BadRequest("The stock doesn't exists.");
                else await _stockRepo.CreateAsync(stock);
            } 

            if (stock == null) return BadRequest("Stock not found");

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            //birden fazla kez eklenmesini engellemeyi amaçlayan bir kontrol 
            if (userPortfolio.Any(e => e.Symbol.ToLower() == symbol.ToLower())) return BadRequest("Cannot add same stock to portfolio");

            var portfolioModel = new Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id
            };
            await _portfolioRepo.CreateAsync(portfolioModel);

            if (portfolioModel == null) return StatusCode(500, "Could not create");
            else  return Created(); //varsayılan olarak HTTP 201 (Created) durum kodunu döner // Created($"api/portfolio/{portfolioModel.AppUserId}", portfolioModel);  //oluşturulan kaynağın nerede olduğunu belirtmek için Location header veya bir içerik sağlanma

        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);

            var filteredStock = userPortfolio.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList(); //we can compare the user portfolio with the symbol that was pass into the actuall API point and we can filter it out

            if (filteredStock.Count() == 1) await _portfolioRepo.DeletePortfolio(appUser, symbol);
            else return BadRequest("Stock not in your portfolio");
            
            return Ok();
        }
    }
}