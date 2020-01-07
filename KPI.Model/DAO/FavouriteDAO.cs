using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPI.Model.helpers;
using KPI.Model.ViewModel;

namespace KPI.Model.DAO
{
    public class FavouriteDAO
    {
        KPIDbContext _dbContext = null;
        public FavouriteDAO()
        {
            this._dbContext = new KPIDbContext();
        }
        public async Task<bool> Add(EF.Favourite entity)
        {
            try
            {
               
                _dbContext.Favourites.Add(entity);
               await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return false;
            }
        }
        public async Task<bool> Delete(int id)
        {
            try
            {
                var item =await _dbContext.Favourites.FirstOrDefaultAsync(x => x.ID == id);
                _dbContext.Favourites.Remove(item);
               await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return false;
            }
        }
        public async Task<object> LoadData(int userid, int page, int pageSize)
        {

            try
            {
             
                var model =(await _dbContext.Favourites
               .Where(x => x.UserID == userid).Join(
                    _dbContext.KPILevels,
                    f => f.KPILevelCode,
                    kpilevel => kpilevel.KPILevelCode,
                    (f, kpilevel) => new
                    {
                        f.UserID,kpilevel.KPIID,kpilevel.LevelID,f.CreateTime,kpilevel.KPILevelCode,f.Period,f.ID
                    }).ToListAsync())
               .Select(x => new FavouriteVM
               {
                   KPIName = _dbContext.KPIs.FirstOrDefault(k => k.ID == x.KPIID).Name,
                   Username = _dbContext.Users.FirstOrDefault(u => u.ID == x.UserID).Username,
                   TeamName = _dbContext.Levels.FirstOrDefault(l => l.ID == x.LevelID).Name,
                   Level = _dbContext.KPIs.FirstOrDefault(k => k.ID == x.KPIID).LevelID,
                   CreateTime = x.CreateTime,
                   KPILevelCode = x.KPILevelCode,
                   Period = x.Period,
                   ID = x.ID
               })
               .OrderByDescending(x => x.CreateTime)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToList();
                int totalRow = model.Count();
                return new
                {
                    status = true,
                    data = model,
                    total = totalRow
                };

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }

        }
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
