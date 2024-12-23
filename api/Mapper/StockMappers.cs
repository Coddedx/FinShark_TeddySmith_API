using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Stock;
using api.Models;

namespace api.Mapper
{
    public static class StockMappers //we could use aouto mapper
    {
        public static StockDto ToStockDto(this Stock stockModel)
        {
            return new StockDto
            {
                Id = stockModel.Id,
                Symbol = stockModel.Symbol,
                CompanyName = stockModel.CompanyName,
                Purchase = stockModel.Purchase,
                LastDiv = stockModel.LastDiv,
                Industry = stockModel.Industry,
                MarketCap = stockModel.MarketCap,
                Comments = stockModel.Comments.Select(c => c.ToCommentDto()).ToList()
            };
        }

        public static Stock ToStockFromCreateDTO(this CreateStockRequestDto stockDto)
        {
            //our dto is in the dto type and when we submit data to our add it has to be on the form of a Stock Model
            return new Stock
            {
                Symbol = stockDto.Symbol,
                CompanyName = stockDto.CompanyName,
                Purchase = stockDto.Purchase,
                LastDiv = stockDto.LastDiv,
                Industry = stockDto.Industry,
                MarketCap = stockDto.MarketCap
            };
        }
        public static Stock ToStockFromFMP(this FMPStock fMPStock)
        {
            //our dto is in the dto type and when we submit data to our add it has to be on the form of a Stock Model
            return new Stock
            {
                Symbol = fMPStock.symbol,
                CompanyName = fMPStock.companyName,
                Purchase = (decimal)fMPStock.price,
                LastDiv = (decimal)fMPStock.lastDiv,
                Industry = fMPStock.industry,
                MarketCap = fMPStock.mktCap
            };
        }
    }
}