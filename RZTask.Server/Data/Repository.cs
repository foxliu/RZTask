using Microsoft.EntityFrameworkCore;
using RZTask.Server.Models;
using System.Linq.Expressions;

namespace RZTask.Server.Data
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _appDbContext;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<Repository<T>> _logger;


        public Repository(ILogger<Repository<T>> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<T>> GetListAsync(QueryParameters queryParameters)
        {
            IQueryable<T> query = _dbSet;

            var perdicate = queryParameters.QueryType.ToUpper() == "OR"
                ? PredicateBuilder.False<T>()  // 初始化为 False，用于 OR 拼接
                : PredicateBuilder.True<T>();  // 初始化为 True，用于 AND 拼接

            foreach (var condition in queryParameters.Query)
            {
                Expression<Func<T, bool>> expression = condition.Type.ToLower() switch
                {
                    "=" => e => EF.Property<object>(e, condition.Key).Equals(condition.Value),
                    ">" => e => EF.Property<IComparable>(e, condition.Key).CompareTo(condition.Value) > 0,
                    ">=" => e => EF.Property<IComparable>(e, condition.Key).CompareTo(condition.Value) >= 0,
                    "<" => e => EF.Property<IComparable>(e, condition.Key).CompareTo(condition.Value) < 0,
                    "<=" => e => EF.Property<IComparable>(e, condition.Key).CompareTo(condition.Value) <= 0,
                    "like" => e => EF.Property<string>(e, condition.Key).Contains(condition.Value.ToString()),
                    "in" => e => IsIn(EF.Property<object>(e, condition.Key), condition.Value.ToString()),
                    _ => e => true
                };

                perdicate = queryParameters.QueryType.ToUpper() == "OR"
                    ? perdicate.Or(expression)
                    : perdicate.And(expression);
            }

            query = query.Where(perdicate);

            if (!string.IsNullOrEmpty(queryParameters.OrderBy))
            {
                query = queryParameters.IsAscending
                    ? query.OrderBy(e => EF.Property<object>(e, queryParameters.OrderBy))
                    : query.OrderByDescending(e => EF.Property<object>(e, queryParameters.OrderBy));
            }

            int offset = queryParameters.Page < 1 ? 0 : queryParameters.Page - 1;
            int skip = offset * queryParameters.Limit;

            return await query.Skip(skip).Take(queryParameters.Limit).ToListAsync();
        }

        private static bool IsIn<V>(V value, string csvValues)
        {
            var values = csvValues.Split(',');
            return values.Select(v => Convert.ChangeType(v, typeof(V))).Contains(value!);
        }
    }
}
