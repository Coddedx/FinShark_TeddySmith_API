using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IStockRepository _stockRepo;
        public StockController(ApplicationDBContext context,IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;
           _context = context; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query) //if we want to filter(where),limit,add more linq command... to our data before .ToList()
                                                                                    //AsQueryable() is gonna delay the ToList so we can filter...  
                                                                                    //so this is what we are going to use actual filtering for our api
                                                                                    //we dont want to pass in through to route, we want to pass in through to query paramaters(https://localhost:3020/api?companyname=palantir)(companyname=palantir)
                                                                                    //Helpers-> QueryObject
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var stocks = await _stockRepo.GetAllAsync(query); // before filtering we wasnt using query     
            var stockDto = stocks.Select(s => s.ToStockDto()).ToList(); //seleect is basicly .net versin mapp
            
            return Ok(stockDto);  //stocks.ToStockDto() yapamayız çünkü stocks bir liste ama ToStockDto tek tek yapıyor o yüzden yukardaki gibi select ile yap
        }

        //so we can get variables one at a time
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id) //FromRoute ->>bir parametrenin değerini URL yolundan (route) almak için
                                                                    //when we returning one stock we have to tell them which one we want
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var stock = await _stockRepo.GetByIdAsync(id);
            if(stock == null) return NotFound();

            return Ok(stock.ToStockDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stockDto)//FromBody->> our data is gonna send json we are not gonna pass the data from url 
                                                                                        //we are gonna pass the data from the actual html body!!
                                                                                        //CreateStockRequest ->> we are gonna create a request portion of our dto. we are doing this cause we dot want users to submit all the data.
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var stockModel = stockDto.ToStockFromCreateDTO();
            await _stockRepo.CreateAsync(stockModel);
            
            //CreatedAtAction ->> its going to run GetById method (nameof(GetById)) and its going to pass this data (id) in the form of the id and its going to return ToStockDto (Yönlendirilmiş aksiyona gönderilecek parametreler)
            return CreatedAtAction(nameof(GetById), new { id = stockModel.Id}, stockModel.ToStockDto());
            //and because of the CreateStockRequestDto the Id is not gonna show in the psot method 
        }
       
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto updateDto)//IActionResult simply a return method
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var stockModel = await _stockRepo.UpdateAsync(id,updateDto);

            return Ok(stockModel.ToStockDto());  //we are going back to stockDto (stockModel is in form of Stock Models but we are passing the StocDto form)
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);
            
            var stockModel = await _stockRepo.DeleteAsync(id);
            if(stockModel == null) return NotFound("Stock does not exist.");

            return NoContent();
        }

    }
}