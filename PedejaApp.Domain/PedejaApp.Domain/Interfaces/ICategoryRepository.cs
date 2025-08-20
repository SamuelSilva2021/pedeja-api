using PedejaApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesOrderedAsync();
        Task<bool> HasProductsAsync(int categoryId);
        Task<int> GetNextDisplayOrderAsync();
        Task UpdateDisplayOrdersAsync(Dictionary<int, int> categoryDisplayOrders);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
