using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Comment;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using api.Mapper;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        //private readonly ApplicationDBContext _context;
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFMPService _fmpService;
        public CommentController(ICommentRepository commentRepo, IStockRepository stockRepo, UserManager<AppUser> userManager, IFMPService fmpService) 
        {
            _commentRepo = commentRepo;   
            _stockRepo = stockRepo;
            _userManager = userManager;
            _fmpService = fmpService;
        }
       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject queryObject) //IActionResult simply a return method. 
                                                                                //Our comment endpoints dumps the entire table we want search by the symbol and sort by the newest --> CommentQueryObject
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);
            
            var comments = await _commentRepo.GetAllAsync(queryObject); 
            var commentDto = comments.Select(s => s.ToCommentDto()); //seleect is basicly .net versin mapp
             
            return Ok(commentDto);
        }

        [HttpGet]
        [Route("{id:int}")] //:int -> this is url constrains (for data validation)(you dont have to put :int)
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var comment = await _commentRepo.GetByIdAsync(id);
            if(comment == null) return NoContent();
            return Ok(comment.ToCommentDto());
        }

        [HttpPost]
        [Route("{symbol:alpha}")]  //we canged it from stockId:int to symbol:alpha for fmpService
        public async Task<IActionResult> Create([FromRoute] string symbol, CreateCommentDto commentDto) //int stockId
        {          
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            //we are not gonna use this cause se use FMPService now (ang it is using the symbol)
            //if(!await _stockRepo.StockExists(stockId)) return BadRequest("Stock does not exist");

            var stock = await _stockRepo.GetBySymbolAsync(symbol);
            if(stock == null) 
            { 
                stock = await _fmpService.FindStockBySymbolAsync(symbol);//this where we are gonna try to populate from the FMPService 
                
                if(stock == null ) return BadRequest("The stock doesn't exists.");
                else await _stockRepo.CreateAsync(stock);
            } 


            var username = User.GetUsername();
            var appuser = await _userManager.FindByNameAsync(username);

            var commentModel = commentDto.ToCommentFromCreateDto(stock.Id); //before the fmp service -> stockId
            commentModel.AppUserId = appuser.Id; //repository de include eklemezsek appuser gözükmez !!!
            await _commentRepo.CreateAsync(commentModel);
            return CreatedAtAction(nameof(GetById), new { id = commentModel.Id}, commentModel.ToCommentDto());
        }

        [HttpPut]
        [Route("{id:int}")] 
        public async Task<IActionResult> Update([FromRoute] int id,[FromBody] UpdateCommentRequestDto updateDto)
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var comment = await _commentRepo.UpdateAsync(id, updateDto.ToCommentFromUpdateDto());
            if(comment == null) return NotFound("Comment not found.");

            return Ok(comment.ToCommentDto());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if(!ModelState.IsValid ) return BadRequest(ModelState);

            var commentModel = await _commentRepo.DeleteAsync(id);
            if(commentModel == null) return NotFound("Comment does not exist.");

            return NoContent();
        }

    }
}