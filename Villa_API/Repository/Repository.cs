using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbset;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbset = _db.Set<T>();
        }


        public async Task CreateAsync(T entity)
        {
            await dbset.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> querry = dbset;
            if (!tracked)
            {
                querry = querry.AsNoTracking();
            }
            if (filter != null)
            {
                querry = querry.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    querry = querry.Include(property);
                }
            }
            return await querry.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null,
			int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<T> querry = dbset;
            if (filter != null)
            {
                querry = querry.Where(filter);
            }
			if (pageSize > 0)
			{
				if (pageSize > 100)
				{
					pageSize = 100;
				}
				querry = querry.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
			}
			if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    querry = querry.Include(property);
                }
            }
            return await querry.ToListAsync();

        }

        public async Task RemoveAsync(T entity)
        {
            dbset.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

    }
}
