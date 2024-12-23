using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Stock;
using api.Interfaces;
using api.Mapper;
using api.Models;
using Newtonsoft.Json;

namespace api.Service
{
    public class FMPService : IFMPService//Financial Modeling Prep is a free stock API
    {
        private HttpClient _httpClient;
        private IConfiguration _config;
        //we need the store the key in appsettings.json that why we are using IConfiguration
        public FMPService(HttpClient httpClient, IConfiguration config) //you could use http client factory but this is a small use 
                                      //HttpClient, .NET ortamında HTTP istekleri yapmak ve HTTP yanıtlarını almak için kullanılan bir sınıftır. 
                                      //Web API'leri, RESTful hizmetleri veya diğer HTTP tabanlı servislerle iletişim kurmak için
                                      //Sunucudan gelen HTTP yanıtını işler, HTTP başlıkları, sorgu parametreleri ve kimlik doğrulama bilgileri ile özelleştirilebilir.
                                      //HttpClient nesnesi sık sık oluşturulup yok edilirse, sistemin bağlantı havuzunu verimsiz kullanır. Bu, soket tükenmesine ve performans sorunlarına neden olabilir,
                                      //Tek bir HttpClient nesnesi tüm uygulama boyunca kullanılırsa, DNS değişikliklerini fark etmeme gibi sorunlar ortaya çıkabilir.
                                      //HttpClientFactory bağlantı havuzlarını daha iyi yönetir, soket tükenmesini önler, yaşam döngüsünü otomatik olarak yönetir, Eski DNS kayıtları gibi sorunları ortadan kaldırır.
                                      //Büyük veya uzun ömürlü uygulamalarda, Polly gibi yeniden deneme veya devre kesici stratejilerine ihtiyaç duyuyorsanız: HttpClientFactory kullanımı önerilir.
        {
            _httpClient = httpClient;
            _config = config;
            
        }
        public async Task<Stock> FindStockBySymbolAsync(string symbol)
        {
            //its going off to the internet and son anything can happen so we need to use try catch
            try
            {
                var result = await _httpClient.GetAsync($"https://financialmodelingprep.com/api/v3/profile/{symbol}?apikey={_config["FMPKey"]}" ); // we are gonna pass into url
                if(result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync(); //the conten is going to return in the form of string and then you can use json convert or Newton Soft to convert to an object
                    var tasks = JsonConvert.DeserializeObject<FMPStock[]>(content); //FMPStock is the type we are gonna turn to ( [] ile array e döndürüyor (FMP öyle döndürüyor çünkü))
                    var stock = tasks[0]; //arraydeki ilk veriyi alıyoruz
                    if(stock != null) return stock.ToStockFromFMP(); //we are gonna map this fmpMpodeling to astock to resembles a model in a our db
                    return null;
                }
                return null; //if it not a succsessful 
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}