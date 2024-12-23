using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _context;

        public StockRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            var stocks =  _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).AsQueryable();//before query we used this .ToListAsync();
            
            //now that we have AsQueryable we can add more logic
            if(!string.IsNullOrWhiteSpace(query.CompanyName)) //we are gonna pass in that we get back from our query
            //you can add more than just CompanyName
            //IsNullOrWhiteSpace -> string değişkenin null, boş ("") veya yalnızca boşluk karakterleri içerip içermediğini kont(bunlar varsa true döner)
            {
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName)); //Contains is going to the search
            }
        
            if(!string.IsNullOrWhiteSpace(query.Symbol)) 
            {
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            }

            if(!string.IsNullOrWhiteSpace(query.SortBy))
            {
                //this is what actually trigger the call
                //you can add more if statement for sortby but we just use symbol
                if(query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase)) 
                //metin karşılaştırmaları yapılırken büyük/küçük harf farkını göz ardı ederek ve dil bağımsız (ordinal) şekilde karşılaştırma
                //bir metnin harflerinin Unicode sırasına göre karşılaştırılması
                {
                    //this whats actually going to trigger the ascending or descending (azalan sıraya göre düzenlenip düzenlenmediği)
                    stocks = query.IsDecsending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol); //: trigger when its false
                }
            }

            var skipNumber = (query.PageNumber - 1) *query.PageSize;

            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }
        public async Task<Stock> CreateAsync(Stock stockModel)
        {
           await _context.Stocks.AddAsync(stockModel);
           await _context.SaveChangesAsync();
           return stockModel;
        }
        public async Task<Stock?> DeleteAsync(int id)
        {
            var stockModel = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            if(stockModel == null) return null;
            _context.Stocks.Remove(stockModel);
            await  _context.SaveChangesAsync();
            return stockModel;
        }
        public async Task<Stock?> GetByIdAsync(int id)
        {
            var stock = await _context.Stocks.Include(c => c.Comments).FirstOrDefaultAsync(i => i.Id == id); //FindAsync include dan önce buydu 
            if(stock == null) return null;
            return stock;
        }
        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            var stockModel = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            
            stockModel.Symbol = stockDto.Symbol;
            stockModel.CompanyName = stockDto.CompanyName;
            stockModel.Purchase = stockDto.Purchase;
            stockModel.LastDiv = stockDto.LastDiv;
            stockModel.Industry = stockDto.Industry;
            stockModel.MarketCap = stockDto.MarketCap;
            
            await  _context.SaveChangesAsync();

            return stockModel;
        }
        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
        }
        public Task<bool> StockExists(int id)
        {
            return _context.Stocks.AnyAsync(s => s.Id == id);
        }

    }
}