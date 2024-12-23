using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Dtos.Stock;
using api.Helpers;

namespace api.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> GetAllAsync(QueryObject query);  //QueryObject query
        Task<Stock?> GetByIdAsync(int id); //FirstorDefault can be null so we need to put ?
        Task<Stock?> GetBySymbolAsync(string symbol);
        Task<Stock> CreateAsync(Stock stockModel);
        Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto); 
        Task<Stock?> DeleteAsync(int id);
        Task<bool> StockExists(int id);

    }
}