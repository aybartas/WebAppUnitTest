using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppUnitTest.Models;

namespace WebAppUnitTest.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly WebAppTestDbContext dbContext;
        private readonly DbSet<T> dbSet;

        public Repository(WebAppTestDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<T>();
        }

        public async Task Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }

        public void Delete(T entity)
        {
             dbSet.Remove(entity);
             dbContext.SaveChanges();
        }

        public async  Task<IEnumerable<T>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public void Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
    }
}
