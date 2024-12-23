using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Comment;
using api.Models;


namespace api.Mapper
{
    public static class CommentMapper
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
        {
            return new CommentDto //this is what we gonna return
            {
                Id = commentModel.Id,
                Title = commentModel.Title,
                Content = commentModel.Content,
                CreatedOn = commentModel.CreatedOn,
                StockId = commentModel.StockId,
                CreatedBy = commentModel.AppUser.UserName
            };
        }

        public static Comment ToCommentFromCreateDto(this CreateCommentDto CreateCommentDto, int stockId)
        {
            return new Comment
            {
                Title = CreateCommentDto.Title,
                Content = CreateCommentDto.Content,
                StockId = stockId
            };
        }
        public static Comment ToCommentFromUpdateDto(this UpdateCommentRequestDto UpdateCommentDto)
        {
            return new Comment
            {
                Title = UpdateCommentDto.Title,
                Content = UpdateCommentDto.Content,
            };
        }
    }
}